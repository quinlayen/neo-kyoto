# ── Sector 14 — Drone Dispatch ─────────────────
#
# Write your program below.
#
# Your available commands:
#   check_next()  — finds the next broken drone
#   reroute()     — fixes MISROUTED drones
#   repair()      — fixes GROUNDED drones
#
# ────────────────────────────────────────────────

while True:
    status = check_next()
    if status == "MISROUTED":
        reroute()
    if status == "GROUNDED":
        repair()
    