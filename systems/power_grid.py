import random


class PowerGrid:
    ALL_SECTORS = [
        "S-01", "S-02", "S-03", "S-04", "S-05",
        "S-06", "S-07", "S-08", "S-09", "S-10",
        "S-11", "S-12", "S-13", "S-14", "S-15",
    ]

    def __init__(self):
        self.sectors = {}
        broken = random.sample(self.ALL_SECTORS, random.randint(8, 12))
        for sid in self.ALL_SECTORS:
            if sid in broken:
                self.sectors[sid] = random.choice(["OFFLINE", "DEGRADED"])
            else:
                self.sectors[sid] = "ONLINE"

    def scan_grid(self):
        print("  ┌─────────┬────────────┐")
        print("  │ Sector  │ Status     │")
        print("  ├─────────┼────────────┤")
        for sid in sorted(self.sectors):
            print(f"  │ {sid:<7s} │ {self.sectors[sid]:<10s} │")
        print("  └─────────┴────────────┘")
        offline = sum(1 for s in self.sectors.values() if s == "OFFLINE")
        degraded = sum(1 for s in self.sectors.values() if s == "DEGRADED")
        print(f"    {offline} offline, {degraded} degraded")

    def get_broken_sectors(self):
        broken = [sid for sid in sorted(self.sectors)
                  if self.sectors[sid] != "ONLINE"]
        print(f"    {len(broken)} sectors need repair:")
        for sid in broken:
            print(f"      {sid} — {self.sectors[sid]}")
        return broken

    def get_status(self, sector_id):
        if sector_id not in self.sectors:
            print(f"    Error: unknown sector '{sector_id}'")
            return "UNKNOWN"
        state = self.sectors[sector_id]
        print(f"    {sector_id}: {state}")
        return state

    def repair(self, sector_id):
        if sector_id not in self.sectors:
            print(f"    Error: unknown sector '{sector_id}'")
            return
        state = self.sectors[sector_id]
        if state == "OFFLINE":
            self.sectors[sector_id] = "ONLINE"
            print(f"    {sector_id}: repaired → ONLINE")
        elif state == "DEGRADED":
            self.sectors[sector_id] = "ONLINE"
            print(f"    {sector_id}: stabilized → ONLINE")
        else:
            print(f"    {sector_id}: already online.")

    def is_goal_met(self):
        return all(s == "ONLINE" for s in self.sectors.values())

    def get_status_text(self):
        online = sum(1 for s in self.sectors.values() if s == "ONLINE")
        total = len(self.sectors)
        indicator = "[OK]" if online == total else "[!!]"
        return f"""  CENTRAL GRID — POWER RESTORATION
  Status:     {indicator} {online}/{total} sectors online
"""
