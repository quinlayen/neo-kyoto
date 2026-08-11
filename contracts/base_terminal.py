from systems.virtual_fs import VirtualFilesystem
from systems.terminal import TerminalInterpreter


class BaseTerminalContract:
    CONTRACT_ID = None
    TITLE = ""
    LOCATION = ""
    CONTRACT_TYPE = "terminal"

    def __init__(self):
        self.completed = False
        self._completion_announced = False
        self.fs = None
        self.terminal = None
        self.reset_system()

    def build_filesystem(self) -> VirtualFilesystem:
        raise NotImplementedError

    def get_briefing(self) -> str:
        raise NotImplementedError

    def get_completion_message(self) -> str:
        raise NotImplementedError

    def get_completed_banner(self) -> str:
        raise NotImplementedError

    def is_goal_met(self) -> bool:
        raise NotImplementedError

    def get_status_text(self) -> str:
        raise NotImplementedError

    def reset_system(self):
        self.fs = self.build_filesystem()
        self.terminal = TerminalInterpreter(self.fs)

    def on_command(self, command_line):
        output = self.terminal.execute(command_line)
        self.update_completion()
        return output

    def update_completion(self) -> bool:
        if not self.completed and self.is_goal_met():
            self.completed = True
        return self.completed

    def consume_completion_announcement(self) -> bool:
        self.update_completion()
        if self.completed and not self._completion_announced:
            self._completion_announced = True
            return True
        return False

    def get_prompt(self):
        return self.terminal.get_prompt()
