from contracts.base import BaseContract
from systems.drone_router import DroneRouter


class Contract02(BaseContract):
    CONTRACT_ID = "contract_02"
    TITLE = "Drone Route Cleanup"
    LOCATION = "Sector 12"
    SCRIPT_FILE = "player_scripts/drone_zone.py"
    BASE_CREDITS = 100
    STAR_THRESHOLDS = (10, 15)

    def __init__(self):
        super().__init__()
        self.router = DroneRouter()

    def get_commands(self):
        return {
            "scan_drones": self.router.scan_drones,
            "reroute_next": self.router.reroute_next,
        }

    def reset_system(self):
        self.router = DroneRouter()

    def is_goal_met(self):
        return self.router.is_goal_met()

    def get_status_text(self):
        return self.router.get_status_text()

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ONCALL:// SYSTEMS CONTRACTOR              ║
    ║   Contract #2478 – Drone Route Cleanup      ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    Good work on Block 7. Word travels fast —
    Sector 12 needs your help now.

    The delivery drones in this sector are flying
    wrong routes. The routing table got corrupted
    and all 8 drones need manual correction.

    ─── YOUR COMMANDS ───

        scan_drones()   — shows all drone statuses
        reroute_next()  — fixes the next misrouted
                          drone

    Some commands give back a value when they
    run. You can store it in a variable with =
    and use print() to see it.

    ─── YOUR GOAL ───

    Correct all 8 misrouted drones. You could
    call reroute_next() eight times — but you
    have a loop now.

    Your script file is: drone_zone.py
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2478 COMPLETE  ★             ║
    ║   Sector 12 Drone Grid — ALL CORRECTED      ║
    ╚══════════════════════════════════════════════╝

    All drones are back on course. Deliveries are
    flowing again. Sector 12 management sends their
    thanks.

    ─── WHAT YOU JUST DID ───

    You used a loop to automate a repetitive task.
    Instead of writing the same command eight times,
    you wrote it once inside a loop and let the
    computer handle the repetition.

    If you used variables to catch what reroute_next()
    gave back, or printed values to see what was
    happening — good. Those skills will be critical
    from here on.

    ─── THE LIMITATION ───

    Your loop fixed all 8 drones, but think about
    what it could NOT do.

    reroute_next() fixes drones in whatever order
    it finds them. But what if some drones were not
    just misrouted — what if some were GROUNDED and
    needed a completely different fix? Your program
    had no way to look at a drone's status and
    choose between two actions.

    scan_drones() showed you priorities and statuses
    on screen. You could read them. But your program
    could not make decisions based on what it saw. It
    did the same thing to every drone, regardless.

    What you need is a way for your program to ask
    a question — "is this drone misrouted or
    grounded?" — and take a different action depending
    on the answer.

    ─── NEW TOOL: CONDITIONALS ───

    You can now use if and else to make decisions.

    An if statement asks a yes-or-no question. If
    the answer is yes (true), the indented code
    underneath runs. If the answer is no (false),
    the indented code is skipped entirely:

        if <condition>:
            <do this>

    The question you ask is called a condition. You
    write it using comparison operators — symbols
    that compare two values:

        ==   "is the left equal to the right?"
        !=   "is the left different from the right?"
        >    "is the left greater than the right?"
        <    "is the left less than the right?"

    Important: == and = are different things.

    A single = is assignment — it means "store this
    value." It is a statement, not a question:

        status = "STUCK"     (store "STUCK" in status)

    A double == is comparison — it asks a question
    and the answer is either true or false:

        status == "STUCK"    (is status equal to "STUCK"?)

    For example, you might check a variable:

        if status == "STUCK":
            <handle the stuck case>

    The computer looks at what is stored in status
    and asks "is this equal to STUCK?" If yes, the
    indented code runs. If no, it is skipped.

    You can add else to handle the other case:

        if status == "STUCK":
            <handle stuck>
        else:
            <handle everything else>

    The program takes one path or the other, never
    both. This is called branching — your program
    can now follow different routes depending on
    what it finds.

    You can also use multiple if statements in a
    row, each checking a different condition. Each
    one is its own independent question.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2478 COMPLETE — Sector 12 Drones Corrected ★\n"
            "New tool unlocked: if/else conditionals\n"
        )
