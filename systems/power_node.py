class PowerNode:
    def __init__(self):
        self.status = "FLICKERING"
        self.load = 0.97
        self.rebalance_count = 0

    def rebalance(self):
        self.rebalance_count += 1
        self.load = max(0.4, self.load - 0.05)

        if self.rebalance_count >= 12 and self.status != "STABLE":
            self.status = "STABLE"

        msg = f"    Rebalance #{self.rebalance_count} — load {self.load:.2f} — Status: {self.status}"
        print(msg)
        return msg

    def is_goal_met(self):
        """Contract 1 goal: node is stable (4+ rebalances)."""
        return self.status == "STABLE"

    def get_status_text(self):
        if self.status == "FLICKERING":
            indicator = "[!!]"
        elif self.status == "STABLE":
            indicator = "[OK]"
        else:
            indicator = "[??]"

        return f"""  BLOCK 7 POWER NODE
  Status:     {indicator} {self.status}
  Load:       {self.load:.2f}
  Rebalances: {self.rebalance_count}
"""
