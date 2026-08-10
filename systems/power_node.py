
import time

class PowerNode:
    def __init__(self):
        self.status = "FLICKERING"
        self.load = 0.91
        self.rebalance_count = 0
        self.stable_since = None
        self.required_stable_seconds = 8  # short for prototype

    def rebalance(self):
        self.rebalance_count += 1
        self.load = max(0.4, self.load - 0.12)
        
        if self.rebalance_count >= 4 and self.status != "STABLE":
            self.status = "STABLE"
            self.stable_since = time.time()
            return "Node rebalanced. Status → STABLE"
        
        return f"Rebalance called ({self.rebalance_count}). Status: {self.status}"

    def tick(self):
        """Call this periodically to check if still stable."""
        if self.status == "STABLE" and self.stable_since:
            elapsed = time.time() - self.stable_since
            if elapsed >= self.required_stable_seconds:
                return True  # contract complete
        return False

    def get_status_text(self):
        return f"""Block 7 Power Node
Status:     {self.status}
Load:       {self.load:.2f}
Rebalances: {self.rebalance_count}
"""