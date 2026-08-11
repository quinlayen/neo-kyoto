# ── Harbor District Warehouse 7 ─────────────────
#
# Write your program below.
#
# Your available commands:
#   check_slot(n)            — returns correction needed
#   get_slot_type(n)         — returns slot type
#   adjust_slot(n, amount)   — adjust STANDARD slots
#   gentle_adjust(n, amount) — adjust FRAGILE slots
#   unlock_slot(n)           — unlock LOCKED slots
#
# Slot numbers: 1 through 6
#
# ────────────────────────────────────────────────

slot_type = get_slot_type(1)
print(slot_type)

correction = check_slot(1)
print(correction)

adjust_slot(1, 4)

if slot_type == "STANDARD":
    adjust_slot(slot, )