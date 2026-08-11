import random


class Warehouse:
    SLOT_DEFS = {
        1: {"type": "STANDARD", "drift": -4},
        2: {"type": "STANDARD", "drift": 3},
        3: {"type": "FRAGILE",  "drift": -2},
        4: {"type": "STANDARD", "drift": 5},
        5: {"type": "LOCKED",   "drift": -3},
        6: {"type": "STANDARD", "drift": 4},
    }

    def __init__(self):
        self.slots = {}
        for n, defn in self.SLOT_DEFS.items():
            self.slots[n] = {
                "type": defn["type"],
                "drift": defn["drift"],
                "locked": defn["type"] == "LOCKED",
            }

    def _validate_slot(self, slot_number):
        if slot_number not in self.slots:
            print(f"    Error: no slot {slot_number}. Valid slots are 1-6.")
            return None
        return self.slots[slot_number]

    def get_slot_type(self, slot_number):
        slot = self._validate_slot(slot_number)
        if slot is None:
            return "UNKNOWN"
        print(f"    Slot {slot_number}: type is {slot['type']}")
        return slot["type"]

    def check_slot(self, slot_number):
        slot = self._validate_slot(slot_number)
        if slot is None:
            return 0

        return -slot["drift"]

    def adjust_slot(self, slot_number, amount):
        slot = self._validate_slot(slot_number)
        if slot is None:
            return

        if slot["type"] == "FRAGILE":
            print(f"    Slot {slot_number}: FAILED — too rough for fragile items.")
            print(f"    Use gentle_adjust() on FRAGILE slots.")
            return

        if slot["locked"]:
            print(f"    Slot {slot_number}: FAILED — slot is locked.")
            print(f"    Use unlock_slot() first.")
            return

        slot["drift"] += amount
        if slot["drift"] == 0:
            print(f"    Slot {slot_number}: adjusted by {amount:+d} → BALANCED")
        else:
            remaining = slot["drift"]
            direction = "over" if remaining > 0 else "under"
            print(f"    Slot {slot_number}: adjusted by {amount:+d} → still {abs(remaining)} {direction}")

    def gentle_adjust(self, slot_number, amount):
        slot = self._validate_slot(slot_number)
        if slot is None:
            return

        if slot["type"] != "FRAGILE":
            print(f"    Slot {slot_number}: gentle_adjust not needed — use adjust_slot().")
            return

        slot["drift"] += amount
        if slot["drift"] == 0:
            print(f"    Slot {slot_number}: gently adjusted by {amount:+d} → BALANCED")
        else:
            remaining = slot["drift"]
            direction = "over" if remaining > 0 else "under"
            print(f"    Slot {slot_number}: gently adjusted by {amount:+d} → still {abs(remaining)} {direction}")

    def unlock_slot(self, slot_number):
        slot = self._validate_slot(slot_number)
        if slot is None:
            return

        if not slot["locked"]:
            print(f"    Slot {slot_number}: already unlocked.")
            return

        slot["locked"] = False
        print(f"    Slot {slot_number}: unlocked. You can now adjust it.")

    def is_goal_met(self):
        return all(s["drift"] == 0 for s in self.slots.values())

    def get_status_text(self):
        balanced = sum(1 for s in self.slots.values() if s["drift"] == 0)
        total = len(self.slots)
        indicator = "[OK]" if balanced == total else "[!!]"

        return f"""  HARBOR DISTRICT WAREHOUSE 7
  Status:     {indicator} {balanced}/{total} slots balanced
"""
