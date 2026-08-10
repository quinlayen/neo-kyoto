from systems.power_node import PowerNode

class Contract01:
    def __init__(self):
        self.node = PowerNode()
        self.completed = False
        self.loops_unlocked = False

    def get_briefing(self):
        return """From: District Power Coordinator – Lower Neo-Kyoto
Subject: Block 7 Power Node

The automatic balancer on Residential Block 7 is offline.
Lights are flickering and people are getting angry.

You have temporary access and one command that works:

    rebalance()

Call it a few times and get the node stable. That’s all I need right now.
"""

    def check_completion(self):
        if self.node.tick() and not self.completed:
            self.completed = True
            self.loops_unlocked = True
            return True
        return False