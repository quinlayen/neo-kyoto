import random


class WaterProcessor:
    ALL_STATIONS = ["PS-01", "PS-02", "PS-03", "PS-04"]
    ALL_VALVES = ["IV-01", "IV-02", "IV-03"]

    def __init__(self):
        self.stations = {}
        self.valves = {}
        self.valves_revealed = False

        broken_stations = random.sample(self.ALL_STATIONS, random.randint(3, 4))
        for sid in self.ALL_STATIONS:
            if sid in broken_stations:
                self.stations[sid] = "CONTAMINATED"
            else:
                self.stations[sid] = "OPERATIONAL"

        for vid in self.ALL_VALVES:
            self.valves[vid] = "CONTAMINATED"

    def get_broken_stations(self):
        broken = [sid for sid in sorted(self.stations)
                  if self.stations[sid] != "OPERATIONAL"]
        print(f"    {len(broken)} pump stations need repair:")
        for sid in broken:
            print(f"      {sid} — {self.stations[sid]}")
        return broken

    def get_broken_valves(self):
        if not self.valves_revealed:
            print("    Error: intake valve system not yet accessible.")
            print("    Fix all pump stations first.")
            return []
        broken = [vid for vid in sorted(self.valves)
                  if self.valves[vid] != "OPERATIONAL"]
        print(f"    {len(broken)} intake valves need repair:")
        for vid in broken:
            print(f"      {vid} — {self.valves[vid]}")
        return broken

    def _repair_unit(self, unit_id, registry, label):
        if unit_id not in registry:
            print(f"    Error: unknown {label} '{unit_id}'")
            return

        state = registry[unit_id]
        if state == "OPERATIONAL":
            print(f"    {unit_id}: already operational.")
            return

        if state != "DRAINED":
            print(f"    {unit_id}: must drain before repair.")
            print(f"    Current state: {state}")
            return

        print(f"    Error: {unit_id} is drained but not flushed.")

    def drain(self, unit_id):
        registry = self._find_registry(unit_id)
        if registry is None:
            return

        state = registry[unit_id]
        if state == "OPERATIONAL":
            print(f"    {unit_id}: already operational. No action needed.")
        elif state == "CONTAMINATED":
            registry[unit_id] = "DRAINED"
            print(f"    {unit_id}: drained → DRAINED")
        elif state == "DRAINED":
            print(f"    {unit_id}: already drained.")
        else:
            print(f"    {unit_id}: cannot drain in state {state}.")

    def flush(self, unit_id):
        registry = self._find_registry(unit_id)
        if registry is None:
            return

        state = registry[unit_id]
        if state == "DRAINED":
            registry[unit_id] = "FLUSHED"
            print(f"    {unit_id}: flushed → FLUSHED")
        elif state == "CONTAMINATED":
            print(f"    {unit_id}: must drain first.")
        elif state == "OPERATIONAL":
            print(f"    {unit_id}: already operational.")
        else:
            print(f"    {unit_id}: cannot flush in state {state}.")

    def refill(self, unit_id):
        registry = self._find_registry(unit_id)
        if registry is None:
            return

        state = registry[unit_id]
        if state == "FLUSHED":
            registry[unit_id] = "REFILLED"
            print(f"    {unit_id}: refilled → REFILLED")
        elif state == "DRAINED":
            print(f"    {unit_id}: must flush before refilling.")
        elif state == "CONTAMINATED":
            print(f"    {unit_id}: must drain and flush first.")
        elif state == "OPERATIONAL":
            print(f"    {unit_id}: already operational.")
        else:
            print(f"    {unit_id}: cannot refill in state {state}.")

    def restart(self, unit_id):
        registry = self._find_registry(unit_id)
        if registry is None:
            return

        state = registry[unit_id]
        if state == "REFILLED":
            registry[unit_id] = "OPERATIONAL"
            print(f"    {unit_id}: restarted → OPERATIONAL")
            if all(s == "OPERATIONAL" for s in self.stations.values()):
                if not self.valves_revealed:
                    self.valves_revealed = True
                    print()
                    print("    ── SYSTEM ALERT ──")
                    print("    All pump stations operational.")
                    print("    Secondary failure detected: intake")
                    print("    valves are also contaminated.")
                    print("    Use get_broken_valves() to see them.")
                    print("    Same repair procedure applies.")
        elif state == "FLUSHED":
            print(f"    {unit_id}: must refill before restarting.")
        elif state == "DRAINED":
            print(f"    {unit_id}: must flush and refill first.")
        elif state == "CONTAMINATED":
            print(f"    {unit_id}: must drain, flush, and refill first.")
        elif state == "OPERATIONAL":
            print(f"    {unit_id}: already operational.")

    def _find_registry(self, unit_id):
        if unit_id in self.stations:
            return self.stations
        if unit_id in self.valves:
            if not self.valves_revealed:
                print(f"    Error: '{unit_id}' not accessible yet.")
                return None
            return self.valves
        print(f"    Error: unknown unit '{unit_id}'")
        return None

    def scan_system(self):
        print("  ┌──────────┬──────────────┐")
        print("  │ Unit     │ Status       │")
        print("  ├──────────┼──────────────┤")
        for sid in sorted(self.stations):
            print(f"  │ {sid:<8s} │ {self.stations[sid]:<12s} │")
        if self.valves_revealed:
            print("  ├──────────┼──────────────┤")
            for vid in sorted(self.valves):
                print(f"  │ {vid:<8s} │ {self.valves[vid]:<12s} │")
        print("  └──────────┴──────────────┘")

    def is_goal_met(self):
        stations_ok = all(s == "OPERATIONAL" for s in self.stations.values())
        valves_ok = all(v == "OPERATIONAL" for v in self.valves.values())
        return stations_ok and valves_ok

    def get_status_text(self):
        station_ok = sum(1 for s in self.stations.values() if s == "OPERATIONAL")
        station_total = len(self.stations)

        if self.valves_revealed:
            valve_ok = sum(1 for v in self.valves.values() if v == "OPERATIONAL")
            valve_total = len(self.valves)
            all_ok = self.is_goal_met()
            indicator = "[OK]" if all_ok else "[!!]"
            return f"""  UNDERGROUND PLANT — WATER TREATMENT
  Status:          {indicator}
  Pump stations:   {station_ok}/{station_total} operational
  Intake valves:   {valve_ok}/{valve_total} operational
"""
        else:
            indicator = "[OK]" if station_ok == station_total else "[!!]"
            return f"""  UNDERGROUND PLANT — WATER TREATMENT
  Status:          {indicator}
  Pump stations:   {station_ok}/{station_total} operational
"""
