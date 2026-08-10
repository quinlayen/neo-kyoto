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
            "adjust_slot": self.warehouse.adjust_slot,
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

    Harbor District Warehouse 7 has a problem. The
    inventory management system drifted overnight —
    slot counts no longer match the physical stock.

    Some slots have too many items, some too few. Each
    one needs a specific correction. Your job is to
    check each slot and apply the right adjustment.

    ─── YOUR NEW TOOLS ───

    You now have variables and function arguments.

    A variable stores a value under a name:

        correction = check_slot(1)

    The command check_slot gives back a number — the
    correction that slot needs. The = sign catches that
    number and stores it as "correction."

    You can then pass that stored value to another command:

        adjust_slot(1, correction)

    The values inside the parentheses are called arguments.
    The first argument says which slot. The second says
    how much to adjust.

    You can also use a variable as a counter in a loop:

        slot = 1
        ...
        slot = slot + 1

    This increases the counter by 1 each time.

    ─── YOUR COMMANDS ───

        check_slot(n)          — check slot n, returns
                                 the correction needed
        adjust_slot(n, amount) — adjust slot n by amount

    ─── YOUR GOAL ───

    Balance all 6 slots. For each slot, you need to check
    what correction it needs, then apply that correction.

    Think about how to handle all 6 slots without writing
    the same code six separate times.

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

    You used variables to capture data from one command
    and pass it to another. Your program made decisions
    based on real values, not just blind repetition.

    ─── THE LIMITATION ───

    Did you notice the notes on some slots? Slot 3 was
    marked FRAGILE, slot 5 was RESTRICTED. Your program
    treated every slot the same way — and it worked this
    time. But what if fragile slots needed gentler
    adjustments? What if restricted slots needed a
    different procedure entirely?

    Right now your program cannot make choices. It does
    the same thing every time, regardless of the situation.

    ─── NEW TOOLS UNLOCKED ───

    You can now use if and else to make decisions.

    The if keyword checks a condition and only runs the
    indented code if the condition is true:

        if <condition>:
            <do this>

    The else keyword handles the other case:

        if <condition>:
            <do this>
        else:
            <do that instead>

    You also have comparison operators:

        ==   means "is equal to"
        !=   means "is not equal to"
        >    means "is greater than"
        <    means "is less than"
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2479 COMPLETE — Warehouse Balanced ★\n"
            "New tools unlocked: if/else conditionals\n"
        )
