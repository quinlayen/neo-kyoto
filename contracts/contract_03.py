from contracts.base import BaseContract
from systems.drone_dispatch import DroneDispatch


class Contract03(BaseContract):
    CONTRACT_ID = "contract_03"
    TITLE = "Drone Dispatch"
    LOCATION = "Sector 14"
    SCRIPT_FILE = "player_scripts/dispatch.py"

    def __init__(self):
        super().__init__()
        self.dispatch = DroneDispatch()

    MAX_CALLS = 20

    def get_commands(self):
        return {
            "check_next": self.dispatch.check_next,
            "reroute": self.dispatch.reroute,
            "repair": self.dispatch.repair,
        }

    def reset_system(self):
        self.dispatch = DroneDispatch()

    def is_goal_met(self):
        return self.dispatch.is_goal_met()

    def get_status_text(self):
        return self.dispatch.get_status_text()

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2479 – Drone Dispatch           ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    Your work in Sector 12 got noticed. Sector 14
    has a bigger problem — their drone fleet is
    down, and it is not as simple as last time.

    Some drones are MISROUTED — flying wrong paths,
    same as Sector 12. But others are GROUNDED —
    completely offline, hardware fault. A reroute
    will not help a grounded drone. It needs a
    different kind of fix.

    Your old approach — blindly fixing every drone
    the same way — will not work here. You need to
    check what is wrong first, then choose the
    right response.

    ─── YOUR COMMANDS ───

        check_next()  — finds the next broken drone
                        and tells you what is wrong
        reroute()     — fixes a MISROUTED drone
        repair()      — fixes a GROUNDED drone

    check_next() gives back the drone's problem as
    text — either "MISROUTED" or "GROUNDED". Catch
    it in a variable so your program can use it to
    decide what to do.

    If you use the wrong fix, the system will tell
    you.

    ─── YOUR GOAL ───

    Get all 8 drones operational.

    ─── HOW TO WORK ───

    Your script file is: dispatch.py

    1. Type  edit    → open the file in your editor
    2. Write your program
    3. Save the file
    4. Come back here and type  run
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2479 COMPLETE  ★             ║
    ║   Sector 14 — ALL DRONES OPERATIONAL        ║
    ╚══════════════════════════════════════════════╝

    All drones are back in the air. Sector 14
    sends payment.

    ─── WHAT YOU JUST DID ───

    You wrote a program that adapts. Instead of
    doing the same thing to every drone, your code
    checked each one's problem and chose the right
    fix. That is the power of conditionals — your
    programs can now make decisions.

    ─── THE LIMITATION ───

    Your while True loop ran until the sandbox
    stopped it. That worked here, but think about
    what would happen without the sandbox — your
    loop would run forever.

    And what if you needed your program to do
    something AFTER the loop? With while True,
    there is no "after" — the loop never ends on
    its own, so any code below it never runs.

    You need a way to make the loop stop when the
    job is done.

    ─── NEW TOOL: CONTROLLED WHILE LOOPS ───

    You have been using while True — a loop that
    repeats forever. But the word after "while"
    does not have to be True. It can be any
    condition — and when that condition becomes
    false, the loop stops.

        count = 0
        while count < 8:
            <do something>
            count = count + 1

    Here is what happens:

    Before the loop starts, count is 0. The
    computer checks: is 0 < 8? Yes — so it runs
    the body. Inside, count goes from 0 to 1.

    Back to the top: is 1 < 8? Yes — run again.
    count goes to 2. Then 3, 4, 5, 6, 7.

    When count reaches 8: is 8 < 8? No — the
    loop stops. The program moves to whatever
    comes after the loop.

    This pattern — set a counter, check it in the
    while condition, increase it inside the body —
    lets you repeat something an exact number of
    times and then move on.

    Any code you write after the loop (without
    indentation) will only run once the loop has
    finished.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2479 COMPLETE — Sector 14 Drones Operational ★\n"
            "New tool unlocked: controlled while loops\n"
        )
