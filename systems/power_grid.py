class PowerGrid:
    SECTOR_DEFS = [
        ("S-01", "OFFLINE"),
        ("S-02", "DEGRADED"),
        ("S-03", "OFFLINE"),
        ("S-04", "ONLINE"),
        ("S-05", "OFFLINE"),
        ("S-06", "DEGRADED"),
        ("S-07", "OFFLINE"),
        ("S-08", "ONLINE"),
        ("S-09", "DEGRADED"),
        ("S-10", "OFFLINE"),
    ]

    def __init__(self):
        self.sectors = {}
        for sector_id, state in self.SECTOR_DEFS:
            self.sectors[sector_id] = state

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
