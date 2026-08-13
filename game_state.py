class GameState:
    UNLOCK_SEQUENCE = [
        "loops",
        "conditionals",
        "for_loops",
        "functions",
    ]

    RANKS = [
        (0, "Junior Contractor"),
        (6, "Contractor"),
        (13, "Senior Contractor"),
        (21, "Systems Engineer"),
        (29, "Chief Architect"),
    ]

    def __init__(self):
        self.completed_contracts = set()
        self.unlocked_features = set()
        self.retired_commands = {}
        self.stars = {}
        self.credits = 0
        self.bonus_completed = {}

    def mark_completed(self, contract_id, unlock_index):
        self.completed_contracts.add(contract_id)
        if 0 <= unlock_index < len(self.UNLOCK_SEQUENCE):
            feature = self.UNLOCK_SEQUENCE[unlock_index]
            self.unlocked_features.add(feature)

    def is_unlocked(self, feature):
        return feature in self.unlocked_features

    def is_contract_completed(self, contract_id):
        return contract_id in self.completed_contracts

    def unlock_all(self, contract_defs):
        self.unlocked_features = set(self.UNLOCK_SEQUENCE)
        for cdef in contract_defs:
            self.completed_contracts.add(cdef["id"])

    def retire_commands(self, commands):
        for name in commands:
            self.retired_commands[name] = self._make_retired(name)

    def _make_retired(self, name):
        def retired_cmd(*args, **kwargs):
            print(f"  [{name}] System already stable. No action needed.")
        return retired_cmd

    def record_stars(self, contract_id, new_stars, base_credits):
        prev_stars = self.stars.get(contract_id, 0)
        if new_stars > prev_stars:
            credit_delta = (new_stars * base_credits) - (prev_stars * base_credits)
            self.credits += credit_delta
            self.stars[contract_id] = new_stars
            return credit_delta
        return 0

    def record_bonus(self, contract_id, bonus_id, bonus_credits=50):
        if contract_id not in self.bonus_completed:
            self.bonus_completed[contract_id] = set()
        if bonus_id not in self.bonus_completed[contract_id]:
            self.bonus_completed[contract_id].add(bonus_id)
            self.credits += bonus_credits
            return bonus_credits
        return 0

    def is_bonus_completed(self, contract_id, bonus_id):
        return bonus_id in self.bonus_completed.get(contract_id, set())

    def get_best_stars(self, contract_id):
        return self.stars.get(contract_id, 0)

    def get_total_stars(self):
        return sum(self.stars.values())

    def get_max_stars(self, contract_count):
        return contract_count * 3

    def get_rank(self):
        total = self.get_total_stars()
        rank = "Junior Contractor"
        for threshold, title in self.RANKS:
            if total >= threshold:
                rank = title
        return rank

    def format_stars(self, count):
        return "★" * count + "☆" * (3 - count)
