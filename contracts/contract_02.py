from contracts.base import BaseContract
from systems.drone_router import DroneRouter


class Contract02(BaseContract):
    CONTRACT_ID = "contract_02"
    TITLE = "Drone Route Cleanup"
    LOCATION = "Sector 12"
    SCRIPT_FILE = "player_scripts/drone_zone.py"

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
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2478 – Drone Route Cleanup      ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    Good work on Block 7. Word travels fast —
    Sector 12 needs your help now.

    The automated delivery drones in this sector
    are flying wrong routes. Packages are arriving
    at the wrong buildings, or not arriving at all.
    The routing table got corrupted and the drones
    need manual correction.

    ─── A NEW TOOL: VARIABLES ───

    Before we get to the drones, there is a
    fundamental concept you need: variables.

    A variable is a name that holds a value. You
    create one with the = sign:

        speed = 30

    After this line runs, the name speed holds
    the number 30. Anywhere you write speed from
    now on, the computer reads it as 30.

    The name you choose matters. Pick something
    that describes what the value represents.
    "speed" is clear. "x" is not. When your
    programs get longer, meaningful names help
    you remember what each value is for.

    The value can be anything — a number, text
    in quotes, or the result a command gives back:

        drone_count = 8
        district = "Sector 12"
        load = 0.5

    ─── CATCHING RETURN VALUES ───

    Some commands give back a value when they run.
    This is called a return value. On its own, that
    value appears and immediately vanishes — the
    computer moves on and the value is gone.

    You can catch it by assigning it to a variable:

        fixed_drone = reroute_next()

    Now fixed_drone holds the ID of the drone
    that was just fixed. You can see it with
    print():

        print(fixed_drone)

    Remember print() from the last contract? You
    can print a variable to see its value, or
    print a command directly to see what it gives
    back:

        print(reroute_next())

    Both work. The first stores the value so you
    can use it later. The second just displays it.

    ─── COUNTERS ───

    You can change a variable by assigning it a
    new value. A common pattern is using a variable
    as a counter:

        count = 0
        count = count + 1

    Here is what happens on the second line: the
    computer looks at the right side first. It
    finds count (which is 0) and adds 1 to it,
    getting 1. Then it takes that result and
    stores it back into count, replacing the old
    value. Now count holds 1.

    If that line runs again — say, inside a loop —
    the computer reads count (now 1), adds 1 to
    get 2, and stores 2 back into count. Each
    time through the loop, count goes up by 1:
    0, 1, 2, 3, 4...

    You do not strictly need variables for this
    contract, but practice using them. Try catching
    what reroute_next() gives back. Try printing
    it. These skills become essential starting
    next contract.

    ─── YOUR COMMANDS ───

        scan_drones()   — shows all drone statuses
                          (returns count of misrouted)
        reroute_next()  — fixes the next misrouted drone
                          (returns the ID it fixed)

    ─── YOUR GOAL ───

    Correct all 8 misrouted drones in the sector.

    You could call reroute_next() eight times by
    hand. But you have a loop now — think about
    how to let the computer handle the repetition.

    The sandbox will stop your program safely
    after enough calls.

    ─── HOW TO WORK ───

    Your script file is: drone_zone.py

    1. Type  edit    → open the file in your editor
    2. Write your program
    3. Save the file
    4. Come back here and type  run
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

        ==   "is equal to"
        !=   "is not equal to"
        >    "is greater than"
        <    "is less than"

    For example, you might check a variable:

        if status == "STUCK":
            <handle the stuck case>

    The computer compares what is stored in status
    to the text "STUCK". If they match, the indented
    code runs. If not, it is skipped.

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
