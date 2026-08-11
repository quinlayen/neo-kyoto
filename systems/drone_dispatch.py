class DroneDispatch:
    DRONE_DEFS = [
        ("D-01", "MISROUTED"),
        ("D-02", "GROUNDED"),
        ("D-03", "MISROUTED"),
        ("D-04", "MISROUTED"),
        ("D-05", "GROUNDED"),
        ("D-06", "MISROUTED"),
        ("D-07", "GROUNDED"),
        ("D-08", "MISROUTED"),
    ]

    def __init__(self):
        self.drone_ids = [drone_id for drone_id, _ in self.DRONE_DEFS]
        self.drones = {}
        for drone_id, state in self.DRONE_DEFS:
            self.drones[drone_id] = state
        self.current = None
        self._pointer = 0

    def check_next(self):
        for i in range(len(self.drone_ids)):
            idx = (self._pointer + i) % len(self.drone_ids)
            drone_id = self.drone_ids[idx]
            if self.drones[drone_id] in ("MISROUTED", "GROUNDED"):
                self.current = drone_id
                self._pointer = (idx + 1) % len(self.drone_ids)
                state = self.drones[drone_id]
                print(f"    {drone_id}: {state}")
                return state
        print(f"    All drones operational.")
        self.current = None
        return "DONE"

    def reroute(self):
        if self.current is None:
            print(f"    No drone selected. Use check_next() first.")
            return
        state = self.drones[self.current]
        if state == "MISROUTED":
            self.drones[self.current] = "OPERATIONAL"
            print(f"    {self.current}: rerouted → OPERATIONAL")
        elif state == "GROUNDED":
            print(f"    {self.current}: not misrouted — reroute won't help.")
            print(f"    This drone is GROUNDED. Try repair().")
        else:
            print(f"    {self.current}: already operational.")

    def repair(self):
        if self.current is None:
            print(f"    No drone selected. Use check_next() first.")
            return
        state = self.drones[self.current]
        if state == "GROUNDED":
            self.drones[self.current] = "OPERATIONAL"
            print(f"    {self.current}: repaired → OPERATIONAL")
        elif state == "MISROUTED":
            print(f"    {self.current}: not grounded — repair won't help.")
            print(f"    This drone is MISROUTED. Try reroute().")
        else:
            print(f"    {self.current}: already operational.")

    def is_goal_met(self):
        return all(s == "OPERATIONAL" for s in self.drones.values())

    def get_status_text(self):
        operational = sum(1 for s in self.drones.values() if s == "OPERATIONAL")
        total = len(self.drones)
        indicator = "[OK]" if operational == total else "[!!]"
        return f"""  SECTOR 14 — DRONE DISPATCH
  Status:     {indicator} {operational}/{total} drones operational
"""
