from contracts.base import BaseContract
from systems.warehouse import Warehouse


class Contract03(BaseContract):
    CONTRACT_ID = "contract_03"
    TITLE = "Inventory Drift"
    LOCATION = "Harbor District"
    SCRIPT_FILE = "player_scripts/warehouse.py"

    def __init__(self):
        super().__init__()
        self.warehouse = Warehouse()

    def get_commands(self):
        return {
            "check_slot": self.warehouse.check_slot,
            "get_slot_type": self.warehouse.get_slot_type,
            "adjust_slot": self.warehouse.adjust_slot,
            "gentle_adjust": self.warehouse.gentle_adjust,
            "unlock_slot": self.warehouse.unlock_slot,
        }

    def is_goal_met(self):
        return self.warehouse.is_goal_met()

    def get_status_text(self):
        return self.warehouse.get_status_text()

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2479 – Inventory Drift          ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    Harbor District Warehouse 7 has a problem.

    The warehouse has 6 storage slots, numbered 1
    through 6. Each slot is supposed to hold a
    specific number of items, but the counts are
    wrong. Some have too many, others too few.

    Here is the complication: not all slots are the
    same. Most are STANDARD — you can adjust them
    normally. But one is FRAGILE — standard
    adjustment is too rough and will fail. Another
    is LOCKED — it must be unlocked before you can
    adjust it at all.

    If you use the wrong command on the wrong type
    of slot, it will fail and tell you why. Your
    program needs to check each slot's type and
    choose the right approach.

    ─── USING CONDITIONALS ───

    This is your first contract where you need to
    make decisions. You just unlocked if and else —
    here is how they work on this job.

    get_slot_type gives back a piece of text — the
    type of a slot, like "STANDARD", "FRAGILE", or
    "LOCKED". You can catch that text in a variable:

        slot_type = get_slot_type(1)

    Then you use if to check what it is:

        if slot_type == "FRAGILE":
            <handle the fragile case>

    The == operator asks "are these two values the
    same?" If they match, the indented code runs.
    If they do not match, it is skipped.

    For each slot, you need to:
    1. Check the slot type with get_slot_type
    2. Check the correction with check_slot
    3. Choose the right action based on the type

    STANDARD slots: use adjust_slot
    FRAGILE slots: use gentle_adjust
    LOCKED slots: use unlock_slot first, then
    adjust_slot

    You can use multiple if statements in a row
    to check for each type separately.

    ─── YOUR COMMANDS ───

        check_slot(n)           — returns the
                                  correction needed
        get_slot_type(n)        — returns the slot type
                                  as text
        adjust_slot(n, amount)  — adjust a STANDARD or
                                  unlocked slot
        gentle_adjust(n, amount)— adjust a FRAGILE slot
        unlock_slot(n)          — unlock a LOCKED slot

    Slot numbers are 1 through 6.

    ─── YOUR GOAL ───

    Balance all 6 slots. Each needs the right
    command for its type.

    ─── HOW TO WORK ───

    Your script file is: warehouse.py

    1. Type  edit    → open the file in your editor
    2. Write your program
    3. Save the file
    4. Come back here and type  run
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2479 COMPLETE  ★             ║
    ║   Harbor District Warehouse — BALANCED      ║
    ╚══════════════════════════════════════════════╝

    All slots are back in sync. The warehouse is
    operational again. Harbor District sends payment.

    ─── WHAT YOU JUST DID ───

    You wrote a program that inspects data and makes
    decisions. Your code checked each slot's type and
    chose the right action — standard adjust, gentle
    adjust, or unlock first. Your programs can now
    adapt to what they find.

    ─── THE LIMITATION ───

    Look at your code. You probably wrote a similar
    check-type-then-act block for each of the 6
    slots, changing only the slot number each time.
    The logic was identical. Only the data differed.

    What if there were 60 slots? 600? Writing the
    same block hundreds of times is not practical.
    You need a way to say "here is a collection of
    items — do this same thing for each one."

    ─── NEW TOOL: FOR LOOPS AND LISTS ───

    A list is a way to hold multiple values under
    one name. You create one with square brackets:

        ids = ["E-01", "E-02", "E-03"]

    After this line, ids holds all three values
    together. A list can hold numbers, text, or
    any mix of values, in order.

    A for loop repeats code once for each item
    in a collection:

        for item in collection:
            <do something with item>

    The variable before "in" (here called item)
    automatically takes on each value, one at a
    time. First loop: item is the first value.
    Second loop: the second value. And so on.

    range() creates a sequence of numbers:

        range(10)    gives you 0 through 9
        range(1, 7)  gives you 1 through 6

    Use it with a for loop to repeat code a
    specific number of times, or to count through
    a sequence of slot numbers, component IDs, etc.

    len() tells you how many items are in a list:

        len(ids)   gives you 3
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2479 COMPLETE — Warehouse Balanced ★\n"
            "New tools unlocked: for loops, lists, range(), len()\n"
        )
