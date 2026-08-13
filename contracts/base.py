class BaseContract:
    CONTRACT_ID = None
    TITLE = ""
    LOCATION = ""
    SCRIPT_FILE = ""
    MAX_CALLS = 20
    BASE_CREDITS = 100
    STAR_THRESHOLDS = (0, 0)

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

    def check_bonus_objectives(self):
        return set()

    def get_star_rating(self, call_count):
        three_star, two_star = self.STAR_THRESHOLDS
        if three_star > 0 and call_count <= three_star:
            return 3
        if two_star > 0 and call_count <= two_star:
            return 2
        return 1

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
