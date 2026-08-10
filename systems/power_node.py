class PowerNode:
    def __init__(self):
        self.status = "FLICKERING"
        self.load = 0.91
        self.rebalance_count = 0

    def rebalance(self):
        self.rebalance_count += 1
        self.load = max(0.4, self.load - 0.12)

        if self.rebalance_count >= 4 and self.status != "STABLE":
            self.status = "STABLE"
            return "Node rebalanced. Status → STABLE"

        return f"Rebalance called ({self.rebalance_count}). Status: {self.status}"

    def is_goal_met(self):
        """Contract 1 goal: node is stable (4+ rebalances)."""
        return self.status == "STABLE"

    def get_status_text(self):
        return f"""Block 7 Power Node
Status:     {self.status}
Load:       {self.load:.2f}
Rebalances: {self.rebalance_count}
"""
