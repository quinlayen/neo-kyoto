from contracts.base_terminal import BaseTerminalContract


class BaseCombinedContract(BaseTerminalContract):
    CONTRACT_TYPE = "combined"
    SCRIPT_FILE = None
    MAX_CALLS = 30

    def get_commands(self):
        raise NotImplementedError

    def get_prompt(self):
        return self.terminal.get_prompt()
