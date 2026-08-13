import random


class ProductionFloor:
    ALL_LINES = [
        "L-01", "L-02", "L-03", "L-04", "L-05",
        "L-06", "L-07", "L-08", "L-09", "L-10",
        "L-11", "L-12",
    ]

    FAILURE_MODES = ["JAMMED", "OVERHEATED"]

    def __init__(self):
        self.lines = {}
        broken = random.sample(self.ALL_LINES, random.randint(8, 10))
        for lid in self.ALL_LINES:
            if lid in broken:
                self.lines[lid] = random.choice(self.FAILURE_MODES)
            else:
                self.lines[lid] = "OPERATIONAL"

    def get_broken_lines(self):
        broken = [lid for lid in sorted(self.lines)
                  if self.lines[lid] != "OPERATIONAL"]
        print(f"    {len(broken)} production lines need repair:")
        for lid in broken:
            print(f"      {lid} — {self.lines[lid]}")
        return broken

    def diagnose(self, line_id):
        if line_id not in self.lines:
            print(f"    Error: unknown line '{line_id}'")
            return "UNKNOWN"
        state = self.lines[line_id]
        print(f"    {line_id}: {state}")
        return state

    def clear_jam(self, line_id):
        if line_id not in self.lines:
            print(f"    Error: unknown line '{line_id}'")
            return
        state = self.lines[line_id]
        if state == "JAMMED":
            self.lines[line_id] = "CLEARED"
            print(f"    {line_id}: jam cleared → CLEARED")
        elif state == "OVERHEATED":
            print(f"    {line_id}: not jammed — this line is overheated.")
        elif state == "OPERATIONAL":
            print(f"    {line_id}: already operational.")
        else:
            print(f"    {line_id}: already cleared.")

    def cool_down(self, line_id):
        if line_id not in self.lines:
            print(f"    Error: unknown line '{line_id}'")
            return
        state = self.lines[line_id]
        if state == "OVERHEATED":
            self.lines[line_id] = "COOLED"
            print(f"    {line_id}: cooled down → COOLED")
        elif state == "JAMMED":
            print(f"    {line_id}: not overheated — this line is jammed.")
        elif state == "OPERATIONAL":
            print(f"    {line_id}: already operational.")
        else:
            print(f"    {line_id}: already cooled.")

    def restart_line(self, line_id):
        if line_id not in self.lines:
            print(f"    Error: unknown line '{line_id}'")
            return
        state = self.lines[line_id]
        if state in ("CLEARED", "COOLED"):
            self.lines[line_id] = "OPERATIONAL"
            print(f"    {line_id}: restarted → OPERATIONAL")
        elif state == "JAMMED":
            print(f"    {line_id}: still jammed. Clear the jam first.")
        elif state == "OVERHEATED":
            print(f"    {line_id}: still overheated. Cool it down first.")
        elif state == "OPERATIONAL":
            print(f"    {line_id}: already operational.")

    def scan_floor(self):
        print("  ┌─────────┬──────────────┐")
        print("  │ Line    │ Status       │")
        print("  ├─────────┼──────────────┤")
        for lid in sorted(self.lines):
            print(f"  │ {lid:<7s} │ {self.lines[lid]:<12s} │")
        print("  └─────────┴──────────────┘")
        jammed = sum(1 for s in self.lines.values() if s == "JAMMED")
        overheated = sum(1 for s in self.lines.values() if s == "OVERHEATED")
        operational = sum(1 for s in self.lines.values() if s == "OPERATIONAL")
        print(f"    {jammed} jammed, {overheated} overheated, {operational} operational")

    def is_goal_met(self):
        return all(s == "OPERATIONAL" for s in self.lines.values())

    def get_status_text(self):
        operational = sum(1 for s in self.lines.values() if s == "OPERATIONAL")
        total = len(self.lines)
        indicator = "[OK]" if operational == total else "[!!]"
        return f"""  INDUSTRIAL ZONE — PRODUCTION FLOOR
  Status:     {indicator} {operational}/{total} lines operational
"""
