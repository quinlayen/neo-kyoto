class AccessController:
    def __init__(self):
        self.components = {
            "E-01": {"state": "STUCK",     "reset_target": "NOMINAL"},
            "E-02": {"state": "UNSTABLE",  "reset_target": "NOMINAL"},
            "E-03": {"state": "STUCK",     "reset_target": "UNSTABLE"},
            "E-04": {"state": "UNSTABLE",  "reset_target": "NOMINAL"},
            "E-05": {"state": "NOMINAL",   "reset_target": "NOMINAL"},
        }

    def get_state(self, component_id):
        if component_id not in self.components:
            print(f"    Error: unknown component '{component_id}'.")
            print(f"    Valid IDs: {', '.join(sorted(self.components))}")
            return "UNKNOWN"

        state = self.components[component_id]["state"]
        print(f"    {component_id}: {state}")
        return state

    def reset_component(self, component_id):
        if component_id not in self.components:
            print(f"    Error: unknown component '{component_id}'.")
            return

        comp = self.components[component_id]
        if comp["state"] == "STUCK":
            comp["state"] = comp["reset_target"]
            print(f"    {component_id}: reset applied → {comp['state']}")
        elif comp["state"] == "NOMINAL":
            print(f"    {component_id}: already nominal. No action needed.")
        else:
            print(f"    {component_id}: not stuck — reset has no effect.")

    def set_watchdog(self, component_id):
        if component_id not in self.components:
            print(f"    Error: unknown component '{component_id}'.")
            return

        comp = self.components[component_id]
        if comp["state"] == "UNSTABLE":
            comp["state"] = "NOMINAL"
            print(f"    {component_id}: watchdog attached → NOMINAL")
        elif comp["state"] == "NOMINAL":
            print(f"    {component_id}: already nominal. Watchdog not needed.")
        else:
            print(f"    {component_id}: stuck — cannot attach watchdog. Reset first.")

    def is_goal_met(self):
        return all(c["state"] == "NOMINAL" for c in self.components.values())

    def get_status_text(self):
        nominal = sum(1 for c in self.components.values() if c["state"] == "NOMINAL")
        total = len(self.components)
        indicator = "[OK]" if nominal == total else "[!!]"

        lines = [f"  MIDTOWN ELEVATOR GRID"]
        lines.append(f"  Status:     {indicator} {nominal}/{total} components nominal")
        lines.append("")
        for cid in sorted(self.components):
            state = self.components[cid]["state"]
            lines.append(f"    {cid}: {state}")
        lines.append("")
        return "\n".join(lines)
