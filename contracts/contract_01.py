from systems.power_node import PowerNode

class Contract01:
    def __init__(self):
        self.node = PowerNode()
        self.completed = False
        self._completion_announced = False

    def get_briefing(self):
        return """=== Neo-Kyoto Systems Contractor ===
    Prototype – Contract 1: Keep the Lights On

    ----------------------------------------
    WELCOME
    ----------------------------------------

    You are a systems contractor in Neo-Kyoto.
    Your job is to fix broken city systems by writing short programs.

    This first contract is deliberately simple so you can learn the basics.

    ----------------------------------------
    WHAT IS A PROGRAM?
    ----------------------------------------

    A program is just a list of instructions that the computer follows
    from top to bottom, one line at a time.

    Each instruction is written as a function call.
    A function call looks like this:

        rebalance()

    The name is `rebalance`.
    The parentheses `()` tell the system to actually run that command.

    ----------------------------------------
    YOUR FIRST TOOL
    ----------------------------------------

    Right now you only have one useful command:

        rebalance()

    This tells the power node to recalculate its priorities once.

    Because the node is unstable, you will need to call this command
    several times in a row.

    ----------------------------------------
    YOUR GOAL
    ----------------------------------------

    Write a short program that calls rebalance() enough times
    to make the power node STABLE.

    When you are ready:
    1. Type  edit
    2. Add several lines of rebalance()
    3. Save the file
    4. Type  run
    """

    def get_completion_message(self):
        return """
★ CONTRACT COMPLETE ★

You just wrote and ran your first program.

You also unlocked a new tool: loops.

A loop lets you repeat instructions automatically
instead of writing the same line many times.

The new form looks like this:

    while True:
        rebalance()

This means: "Keep doing rebalance() forever."

Try editing your script and replacing the repeated lines
with the loop version, then run it again.

(The sandbox will auto-stop continuous loops after a few
dozen cycles so you get control back.)
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT COMPLETE ★\n"
            "Unlock: while True loops are now available.\n"
            "Try refactoring your script to use a loop.\n"
        )

    def update_completion(self):
        """Mark complete when the node goal is met. Idempotent."""
        if not self.completed and self.node.is_goal_met():
            self.completed = True
        return self.completed

    def consume_completion_announcement(self):
        """
        Return True exactly once after the contract becomes complete,
        so the full celebration message is shown once.
        """
        self.update_completion()
        if self.completed and not self._completion_announced:
            self._completion_announced = True
            return True
        return False
