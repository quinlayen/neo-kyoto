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

    Harbor District Warehouse 7 has a problem.

    The warehouse has 6 storage slots, numbered 1
    through 6. Each slot is supposed to hold a specific
    number of items, but the counts are wrong. Some
    slots have too many items, others have too few.

    Nobody knows exactly how far off each slot is.
    Your job is to check each one, find out what
    correction it needs, and apply that correction.

    ─── PUTTING VARIABLES TO WORK ───

    This is your first contract where you need to
    remember information and pass it between commands.

    The command check_slot takes a slot number as an
    argument. You tell it which slot to inspect by
    putting the number inside the parentheses:

        check_slot(1)

    When check_slot runs, it does two things: it prints
    the slot's status on screen, and it gives back a
    number — the correction that slot needs. This
    "giving back" is called a return value.

    If you just call check_slot(1) on its own, that
    number appears and immediately vanishes. The
    computer moves on to the next line and the number
    is gone forever.

    To hold on to it, you catch it in a variable using
    the = sign:

        correction = check_slot(1)

    Now the name "correction" holds whatever number
    check_slot gave back. You can use that name later
    as if it were the number itself.

    The command adjust_slot takes two arguments,
    separated by a comma: which slot to fix, and how
    much to adjust it by. You can pass your variable
    as the second argument — the computer reads the
    value stored in it and hands it to the command.

    ─── WORKING THROUGH ALL 6 SLOTS ───

    You need to do this two-step process — check, then
    adjust — for each of the 6 slots.

    One approach: write the two steps out six times,
    using the number 1, then 2, then 3, and so on.
    That works, but it is a lot of repeated code.

    A smarter approach: use a variable to track which
    slot you are working on. Start it at 1, and each
    time through a loop, increase it by 1:

        slot = slot + 1

    The computer reads the right side first — takes
    the current value of slot, adds 1 — then stores
    the result back into slot. So if slot was 3, it
    becomes 4.

    Put this inside a while True loop along with your
    check and adjust steps, and the loop will work
    through slot 1, then 2, then 3, and so on. When
    it goes past slot 6, the system will just report
    that the slot does not exist, and the sandbox will
    stop the loop after enough calls.

    ─── YOUR COMMANDS ───

        check_slot(n)          — check slot n, returns
                                 the correction needed
        adjust_slot(n, amount) — adjust slot n by amount

    Slot numbers are 1 through 6.

    ─── YOUR GOAL ───

    Balance all 6 slots.

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
    and pass it to another. Your program read real
    information from the system, stored it, and used
    it to take the right action. That is a big step —
    your programs are no longer just blind repetition.
    They work with real data.

    ─── THE LIMITATION ───

    Did you notice the notes on some slots? Slot 3 was
    marked FRAGILE, slot 5 was RESTRICTED. Your program
    treated every slot the same way — and that worked
    this time. But what if fragile slots needed gentler
    adjustments? What if restricted slots needed a
    completely different procedure?

    Right now your program follows the same path every
    time, no matter what. It cannot look at a situation
    and choose between two different actions. It just
    does the same thing, regardless.

    ─── NEW TOOL: CONDITIONALS ───

    You can now use if and else to make decisions. This
    lets your program choose what to do based on what
    it finds.

    An if statement asks a yes-or-no question. If the
    answer is yes (true), the indented code underneath
    runs. If the answer is no (false), the indented
    code is skipped entirely.

    The question you ask is called a condition. You
    write it using comparison operators:

        ==   "is equal to"         (is this the same?)
        !=   "is not equal to"     (is this different?)
        >    "is greater than"     (is this bigger?)
        <    "is less than"        (is this smaller?)
        >=   "is greater than or equal to"
        <=   "is less than or equal to"

    For example, if you had a variable called status
    that held some text, you could check its value:

        if status == "FRAGILE":
            <handle the fragile case>

    The computer compares the value in status to the
    text "FRAGILE". If they match, the indented code
    runs. If they do not match, it is skipped.

    You can add else to handle the other case — what
    to do when the condition is NOT true:

        if status == "FRAGILE":
            <handle fragile>
        else:
            <handle everything else>

    The program takes one path or the other, never
    both. This is called branching — your program can
    now follow different routes depending on the data.

    You can also chain multiple if statements to check
    several conditions one after another. Each one is
    its own independent question.

    This is a turning point. Until now, your programs
    followed a single fixed path. Now they can adapt.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2479 COMPLETE — Warehouse Balanced ★\n"
            "New tools unlocked: if/else conditionals\n"
        )
