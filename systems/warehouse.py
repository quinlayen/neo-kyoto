import random


class Warehouse:
    SLOT_NOTES = {
        1: "STANDARD",
        2: "STANDARD",
        3: "FRAGILE",
        4: "STANDARD",
        5: "RESTRICTED",
        6: "STANDARD",
    }

    def __init__(self):
        random.seed(2189)
        self.slots = {}
        for n in range(1, 7):
            drift = random.choice([-5, -4, -3, -2, -1, 1, 2, 3, 4, 5])
            self.slots[n] = {"drift": drift, "note": self.SLOT_NOTES[n]}

    def check_slot(self, slot_number):
        if slot_number not in self.slots:
            print(f"    Error: no slot {slot_number}. Valid slots are 1-6.")
            return 0

        slot = self.slots[slot_number]
        correction = -slot["drift"]

        if slot["drift"] == 0:
            print(f"    Slot {slot_number}: BALANCED (no correction needed)")
        elif slot["drift"] > 0:
            print(f"    Slot {slot_number}: {slot['drift']} items over count [{slot['note']}]")
        else:
            print(f"    Slot {slot_number}: {abs(slot['drift'])} items under count [{slot['note']}]")

        return correction

    def adjust_slot(self, slot_number, amount):
        if slot_number not in self.slots:
            print(f"    Error: no slot {slot_number}. Valid slots are 1-6.")
            return

        slot = self.slots[slot_number]
        slot["drift"] += amount

        if slot["drift"] == 0:
            print(f"    Slot {slot_number}: adjusted by {amount:+d} → BALANCED")
        else:
            remaining = slot["drift"]
            direction = "over" if remaining > 0 else "under"
            print(f"    Slot {slot_number}: adjusted by {amount:+d} → still {abs(remaining)} {direction}")

    def is_goal_met(self):
        return all(s["drift"] == 0 for s in self.slots.values())

    def get_status_text(self):
        balanced = sum(1 for s in self.slots.values() if s["drift"] == 0)
        total = len(self.slots)
        indicator = "[OK]" if balanced == total else "[!!]"

        return f"""  HARBOR DISTRICT WAREHOUSE 7
  Status:     {indicator} {balanced}/{total} slots balanced
"""
