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

    Good work on Block 7. Word travels fast — Sector 12
    needs your help now.

    The automated delivery drones in this sector are
    flying wrong routes. Packages are arriving at the
    wrong buildings, or not arriving at all. The routing
    table got corrupted and the drones need manual
    correction.

    ─── YOUR LOOP ───

    Last contract, you unlocked the while loop. This is
    your chance to put it to real use.

    Remember how a while loop works: the computer checks
    the condition, and if it is true, it runs the
    indented instructions underneath. Then it goes back
    to the top and checks the condition again. This
    cycle repeats until the condition becomes false —
    or, if the condition is always true, the sandbox
    stops it safely after enough cycles.

    This contract has more work than the last one.
    Writing every command by hand would be tedious.
    A loop is the right tool here.

    ─── YOUR NEW COMMANDS ───

    You have two new commands for this job:

        scan_drones()   — shows the status of all drones
        reroute_next()  — fixes the next misrouted drone

    scan_drones() prints a table of every drone in the
    sector, showing its ID, priority level, and whether
    it is MISROUTED or CORRECTED. Use it to see the
    current state of things.

    reroute_next() finds the next misrouted drone and
    corrects its route. Each call fixes exactly one
    drone. If all drones are already corrected, it
    tells you so.

    ─── YOUR GOAL ───

    Correct all 8 misrouted drones in the sector.

    You could call reroute_next() eight times by hand.
    But you have a better tool now — think about how
    to let the computer handle the repetition for you.

    The sandbox will stop your program safely after
    enough command calls, so do not worry about it
    running out of control.

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

    All drones are back on course. Deliveries are flowing
    again. Sector 12 management sends their thanks.

    ─── WHAT YOU JUST DID ───

    You used a loop to automate a repetitive task.
    Instead of writing the same command eight times,
    you wrote it once inside a loop and let the computer
    handle the repetition. That is a fundamental idea
    in programming: write the pattern once, repeat it
    as many times as needed.

    ─── THE LIMITATION ───

    Your loop was powerful, but think about what it
    could not do.

    Every command you have used so far works like a
    button — you press it and something happens. But
    you have no control over what it does. reroute_next()
    picks which drone to fix. rebalance() just runs.
    You cannot tell a command to work on a specific
    item, or give it a number to use.

    And when scan_drones() showed you information — IDs,
    priorities, statuses — you could read it on screen,
    but your program could not do anything with it. The
    data appeared and vanished. You had no way to grab
    a value, hold on to it, and use it later.

    That is two problems:
    1. You cannot give a command specific instructions.
    2. You cannot remember or reuse information.

    The next contract will need both of these abilities.

    ─── NEW TOOL: VARIABLES ───

    A variable is a name that holds a value. You create
    one with the = sign:

        x = 5

    After this line runs, the name x holds the number 5.
    Anywhere you write x from now on, the computer sees 5.

    The value can be anything — a number, a piece of text,
    or even the result that a command gives back:

        speed = 30
        name = "Block 7"
        count = 0

    You can change a variable by giving it a new value:

        count = 0
        count = count + 1

    After the second line, count holds 1. The computer
    reads the right side first (0 + 1 = 1), then stores
    the result back into count.

    This is especially useful inside a loop — each time
    through, the value grows by 1, giving you a counter
    that tracks how many times the loop has run.

    ─── NEW TOOL: FUNCTION ARGUMENTS ───

    Until now, your commands took no inputs — you just
    wrote rebalance() or reroute_next() and they did
    their thing. But some commands need to know *what*
    to work on.

    You tell a command what to work on by putting values
    inside the parentheses:

        some_command(3)

    The value inside is called an argument. It is an
    input that the command uses to do its job.

    Some commands take more than one argument, separated
    by commas:

        some_command(3, 10)

    You can also pass a variable as an argument. The
    computer reads the variable's value and hands it
    to the command:

        slot = 3
        some_command(slot)

    This does the same thing as some_command(3), but
    now you can change which slot to work on by changing
    the variable — especially powerful inside a loop.

    ─── GIVING BACK VALUES ───

    Some commands give back a result when they run.
    On their own, that result disappears — nobody
    catches it. But with a variable, you can hold on
    to it:

        result = some_command(1)

    Now result holds whatever the command gave back.
    You can pass it to another command, print it, or
    use it in a calculation.

    This is how your programs start to work with real
    data — not just repeating commands blindly, but
    reading information and acting on it.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2478 COMPLETE — Sector 12 Drones Corrected ★\n"
            "New tools unlocked: variables and function arguments\n"
        )
