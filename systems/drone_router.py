class DroneRouter:
    def __init__(self):
        self.drones = [
            {"id": "D-01", "priority": "CRITICAL", "status": "MISROUTED"},
            {"id": "D-02", "priority": "STANDARD", "status": "MISROUTED"},
            {"id": "D-03", "priority": "CRITICAL", "status": "MISROUTED"},
            {"id": "D-04", "priority": "LOW",      "status": "MISROUTED"},
            {"id": "D-05", "priority": "STANDARD", "status": "MISROUTED"},
            {"id": "D-06", "priority": "CRITICAL", "status": "MISROUTED"},
            {"id": "D-07", "priority": "LOW",      "status": "MISROUTED"},
            {"id": "D-08", "priority": "STANDARD", "status": "MISROUTED"},
        ]

    def scan_drones(self):
        print("  ┌─────────┬───────────┬────────────┐")
        print("  │ Drone   │ Priority  │ Status     │")
        print("  ├─────────┼───────────┼────────────┤")
        for d in self.drones:
            print(f"  │ {d['id']:<7s} │ {d['priority']:<9s} │ {d['status']:<10s} │")
        print("  └─────────┴───────────┴────────────┘")

    def reroute_next(self):
        for d in self.drones:
            if d["status"] == "MISROUTED":
                d["status"] = "CORRECTED"
                print(f"    Drone {d['id']} ({d['priority']}) rerouted → CORRECTED")
                return
        print("    All drones already corrected.")

    def is_goal_met(self):
        return all(d["status"] == "CORRECTED" for d in self.drones)

    def get_status_text(self):
        corrected = sum(1 for d in self.drones if d["status"] == "CORRECTED")
        total = len(self.drones)
        if corrected == total:
            indicator = "[OK]"
        else:
            indicator = "[!!]"

        return f"""  SECTOR 12 DRONE GRID
  Status:     {indicator} {corrected}/{total} drones corrected
"""
