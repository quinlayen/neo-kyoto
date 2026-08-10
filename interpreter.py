import ast
import signal
import traceback


class SandboxStop(Exception):
    pass


class RestrictedInterpreter:
    MAX_CALLS = 100
    TIMEOUT_SECONDS = 5

    FEATURE_GATES = {
        "loops":        (ast.While,),
        "variables":    (ast.Assign, ast.AugAssign),
        "conditionals": (ast.If, ast.IfExp),
        "for_loops":    (ast.For,),
        "functions":    (ast.FunctionDef,),
    }

    FEATURE_NAMES = {
        "loops":        "while loops",
        "variables":    "variables (= assignment)",
        "conditionals": "if/else conditionals",
        "for_loops":    "for loops",
        "functions":    "function definitions (def)",
    }

    def __init__(self, game_state):
        self.game_state = game_state
        self.active_commands = {}
        self._call_count = 0

    def set_commands(self, active_commands, retired_commands=None):
        merged = {}
        if retired_commands:
            merged.update(retired_commands)
        merged.update(active_commands)
        self.active_commands = merged

    def _wrap_command(self, name, func):
        def wrapped(*args, **kwargs):
            self._call_count += 1
            if self._call_count > self.MAX_CALLS:
                raise SandboxStop(
                    f"Sandbox auto-stopped after {self.MAX_CALLS} "
                    f"total command calls (safety limit)."
                )
            return func(*args, **kwargs)
        return wrapped

    def _check_feature_gates(self, code):
        try:
            tree = ast.parse(code)
        except SyntaxError:
            return None
        for node in ast.walk(tree):
            for feature_key, node_types in self.FEATURE_GATES.items():
                if isinstance(node, node_types):
                    if not self.game_state.is_unlocked(feature_key):
                        name = self.FEATURE_NAMES[feature_key]
                        return (
                            f"Error: {name} are not available yet.\n"
                            "Complete the current contract to unlock new tools."
                        )
        return None

    @staticmethod
    def _timeout_handler(signum, frame):
        raise SandboxStop(
            "Sandbox auto-stopped: script ran for more than 5 seconds.\n"
            "Check for infinite loops that don't call any commands."
        )

    def execute(self, code):
        gate_error = self._check_feature_gates(code)
        if gate_error:
            return gate_error

        self._call_count = 0

        restricted_builtins = {}
        if self.game_state.is_unlocked("for_loops"):
            restricted_builtins["range"] = range
            restricted_builtins["len"] = len

        restricted_globals = {
            "__builtins__": restricted_builtins,
            "print": print,
        }

        for name, func in self.active_commands.items():
            restricted_globals[name] = self._wrap_command(name, func)

        old_handler = signal.getsignal(signal.SIGALRM)
        signal.signal(signal.SIGALRM, self._timeout_handler)
        signal.alarm(self.TIMEOUT_SECONDS)

        try:
            exec(code, restricted_globals, {})
            return "Script executed successfully."
        except SandboxStop as e:
            return str(e)
        except NameError as e:
            available = ", ".join(sorted(self.active_commands.keys()))
            return (
                f"Error: {e}\n\n"
                "That command does not exist yet.\n"
                f"Available commands: {available}"
            )
        except SyntaxError as e:
            return (
                f"Syntax error on line {e.lineno}: {e.msg}\n\n"
                "Check your code for typos."
            )
        except Exception as e:
            return f"Error: {e}\n{traceback.format_exc()}"
        finally:
            signal.alarm(0)
            signal.signal(signal.SIGALRM, old_handler)
