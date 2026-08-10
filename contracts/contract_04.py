from contracts.base import BaseContract
from systems.access_control import AccessController


class Contract04(BaseContract):
    CONTRACT_ID = "contract_04"
    TITLE = "Elevator Recovery"
    LOCATION = "Midtown"
    SCRIPT_FILE = "player_scripts/elevator.py"

    def __init__(self):
        super().__init__()
        self.controller = AccessController()

    def get_commands(self):
        return {
            "get_state": self.controller.get_state,
            "reset_component": self.controller.reset_component,
            "set_watchdog": self.controller.set_watchdog,
        }

    def is_goal_met(self):
        return self.controller.is_goal_met()

    def get_status_text(self):
        return self.controller.get_status_text()

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2480 – Elevator Recovery        ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    The Midtown elevator grid is in trouble. Five elevator
    components have entered various failure states and the
    building's residents are trapped on their floors.

    Each component needs different treatment depending on
    what went wrong. Some are STUCK — they need a hard
    reset. Others are UNSTABLE — they need a watchdog
    process to keep them steady.

    ─── YOUR NEW TOOLS ───

    You now have if/else conditionals. Your program can
    inspect a value and choose what to do based on what
    it finds.

    You also have comparison operators:
        ==  (is equal to)     !=  (is not equal to)
        >   (greater than)    <   (less than)

    ─── YOUR COMMANDS ───

        get_state(id)          — check a component's state
                                 (returns the state as text)
        reset_component(id)    — reset a STUCK component
        set_watchdog(id)       — stabilize an UNSTABLE component

    Component IDs are: "E-01", "E-02", "E-03", "E-04", "E-05"

    ─── YOUR GOAL ───

    Bring all 5 components to NOMINAL state.

    For each component: check its state, then take the
    right action. Not every component needs the same fix,
    and at least one may need more than one step.

    Be careful — applying the wrong fix does nothing.
    Check the status display to see what state each
    component is in.

    ─── HOW TO WORK ───

    Your script file is: elevator.py

    1. Type  edit    → open the file in your editor
    2. Write your program
    3. Save the file
    4. Come back here and type  run

    Hint: you may need to run your script more than once
    if a component changes state after your first fix.
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2480 COMPLETE  ★             ║
    ║   Midtown Elevator Grid — ALL NOMINAL       ║
    ╚══════════════════════════════════════════════╝

    All elevators are running again. Midtown residents
    can move freely between floors. Well done.

    ─── WHAT YOU JUST DID ───

    You wrote a program that inspects data and makes
    decisions. Your code checked each component's state
    and chose the right action — reset or watchdog.
    That is the core of almost all real software:
    look at data, decide what to do, act.

    ─── THE LIMITATION ───

    Look at your code. You probably wrote the same
    if/else block five times — once for each component,
    changing only the ID string each time.

    What if there were 50 components? 500? Writing the
    same block hundreds of times is not practical. You
    need a way to say "do this for each item in a list."

    ─── NEW TOOLS UNLOCKED ───

    You can now use for loops, lists, range(), and len().

    A list is a collection of values:

        ids = ["E-01", "E-02", "E-03"]

    A for loop repeats code once for each item:

        for item in ids:
            <do something with item>

    The variable "item" takes on each value in the list,
    one at a time.

    range() creates a sequence of numbers:

        for i in range(10):
            <runs 10 times, i goes from 0 to 9>

    len() tells you how many items are in a list:

        len(ids)   gives you 3
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2480 COMPLETE — Elevators Nominal ★\n"
            "New tools unlocked: for loops, lists, range(), len()\n"
        )
