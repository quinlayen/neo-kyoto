import traceback

class RestrictedInterpreter:
    def __init__(self, power_node):
        self.power_node = power_node
        self.allowed_builtins = {
            "rebalance": self.power_node.rebalance,
            "print": print,
        }
        self.loops_unlocked = False

    def unlock_loops(self):
        self.loops_unlocked = True

    def execute(self, code: str) -> str:
        # Extremely restricted environment
        restricted_globals = {
            "__builtins__": {},
            **self.allowed_builtins
        }

        # Very basic protection against while True if not unlocked
        if not self.loops_unlocked and "while" in code:
            return "Error: loops are not unlocked yet."

        try:
            exec(code, restricted_globals, {})
            return "Execution finished."
        except Exception as e:
            return f"Error: {e}\n{traceback.format_exc()}"