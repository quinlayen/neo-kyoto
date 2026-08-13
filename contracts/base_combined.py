from contracts.base_terminal import BaseTerminalContract


class BaseCombinedContract(BaseTerminalContract):
    CONTRACT_TYPE = "combined"
    SCRIPT_FILE = None
    MAX_CALLS = 30
    BASE_CREDITS = 200
    STAR_THRESHOLDS = (0, 0)

    def get_commands(self):
        raise NotImplementedError

    def reset_game_system(self):
        raise NotImplementedError

    def get_star_rating(self, call_count=None):
        if call_count is not None:
            three_star, two_star = self.STAR_THRESHOLDS
            if three_star > 0 and call_count <= three_star:
                return 3
            if two_star > 0 and call_count <= two_star:
                return 2
            return 1
        return super().get_star_rating()

    def get_prompt(self):
        return self.terminal.get_prompt()
