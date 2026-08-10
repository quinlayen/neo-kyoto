import ast
import traceback


class SandboxStop(Exception):
    """Raised when the sandbox cuts off a runaway / continuous script."""


class RestrictedInterpreter:
    # Continuous loops (while True) are valid after unlock, but this is a
    # terminal sandbox — exec would hang forever without a hard cap.
    MAX_REBALANCE_CALLS = 40

    def __init__(self, power_node):
        self.power_node = power_node
        self.loops_unlocked = False

    def unlock_loops(self):
        self.loops_unlocked = True

    def _make_rebalance(self):
        calls = {"n": 0}
        real = self.power_node.rebalance

        def rebalance():
            calls["n"] += 1
            if calls["n"] > self.MAX_REBALANCE_CALLS:
                raise SandboxStop(
                    f"Sandbox auto-stopped after {self.MAX_REBALANCE_CALLS} "
                    f"rebalance() calls (continuous loop safety)."
                )
            return real()

        return rebalance

    def _contains_while_loop(self, code: str) -> bool:
        """True only for real while statements, not the word in comments/strings."""
        try:
            tree = ast.parse(code)
        except SyntaxError:
            # Let exec report the syntax error later.
            return False
        return any(isinstance(node, ast.While) for node in ast.walk(tree))

    def execute(self, code: str) -> str:
        if not self.loops_unlocked and self._contains_while_loop(code):
            return "Error: loops are not unlocked yet."

        restricted_globals = {
            "__builtins__": {},
            "rebalance": self._make_rebalance(),
            "print": print,
        }

        try:
            exec(code, restricted_globals, {})
            return "Execution finished."
        except SandboxStop as e:
            return str(e)
        except Exception as e:
            return f"Error: {e}\n{traceback.format_exc()}"
