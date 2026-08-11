class GameState:
    UNLOCK_SEQUENCE = [
        "loops",
        "conditionals",
        "for_loops",
        "functions",
    ]

    def __init__(self):
        self.completed_contracts = set()
        self.unlocked_features = set()
        self.retired_commands = {}

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
