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

    The automated delivery drones in this sector are flying
    wrong routes. Packages are arriving at the wrong
    buildings, or not arriving at all. The routing table
    got corrupted and the drones need manual correction.

    ─── WHAT YOU HAVE LEARNED ───

    Last contract, you learned that a program is a list of
    instructions, read from top to bottom. You also unlocked
    a new tool: while loops.

    A while loop repeats instructions automatically.

    ─── YOUR NEW TOOLS ───

    You have two new commands for this job:

        scan_drones()   — shows the status of all drones
        reroute_next()  — fixes the next misrouted drone

    scan_drones() lets you see which drones need help.
    reroute_next() corrects one drone each time you call it.

    ─── YOUR GOAL ───

    Correct all 8 misrouted drones in the sector.

    You could call reroute_next() eight times by hand.
    But remember — you have a better tool now.

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

    You used a loop to automate a repetitive task. Instead
    of writing the same command eight times, you let the
    computer handle the repetition.

    ─── THE LIMITATION ───

    Notice what scan_drones() showed you: each drone has
    an ID and a priority level. Some were CRITICAL, some
    were LOW priority. But reroute_next() just fixes drones
    in order — you had no way to choose which one to fix
    first, or to skip ones that were already done.

    What if you could store information — like a drone's
    ID — and pass it to a command? What if commands could
    accept inputs so you could tell them exactly what to do?

    ─── NEW TOOLS UNLOCKED ───

    You can now use variables and function arguments.

    A variable lets you store a value and give it a name:

        x = 5

    The = sign means "store the value on the right under
    the name on the left." You can use that name later.

    Function arguments let you pass values to commands:

        some_command(value)

    The value inside the parentheses tells the command
    what to work on. Future contracts will use these.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2478 COMPLETE — Sector 12 Drones Corrected ★\n"
            "New tools unlocked: variables and function arguments\n"
        )
