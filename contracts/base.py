class BaseContract:
    CONTRACT_ID = None
    TITLE = ""
    LOCATION = ""
    SCRIPT_FILE = ""
    MAX_CALLS = 20

    def __init__(self):
        self.completed = False
        self._completion_announced = False

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

    def get_commands(self) -> dict:
        raise NotImplementedError

    def reset_system(self):
        raise NotImplementedError

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
