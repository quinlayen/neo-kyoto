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

    The Midtown elevator grid is in trouble. Eight
    elevator components have entered various failure
    states and the building's residents are trapped
    on their floors.

    Each component needs different treatment. Some
    are STUCK — they need a hard reset. Others are
    UNSTABLE — they need a watchdog process. At
    least one may need more than one step.

    ─── USING FOR LOOPS AND LISTS ───

    You just unlocked for loops and lists. This
    contract is where they pay off.

    There are 8 components with IDs "E-01" through
    "E-08". You could write the check-and-fix code
    for each one individually — but that is 8 blocks
    of nearly identical code. Last contract showed
    you how painful that gets.

    Instead, put the IDs in a list and use a for
    loop to work through them:

    A list holds multiple values under one name,
    written with square brackets and commas.

    A for loop walks through the list one item at
    a time. Each time through, the loop variable
    holds the current item. You use that variable
    as the argument to your commands.

    For each component: check its state with
    get_state, then use if/else to decide whether
    to reset it or set a watchdog. The for loop
    handles moving to the next component.

    ─── YOUR COMMANDS ───

        get_state(id)        — check a component's
                               state (returns text)
        reset_component(id)  — reset a STUCK one
        set_watchdog(id)     — stabilize UNSTABLE

    Component IDs are text values in quotes:
    "E-01" through "E-08"

    The wrong fix does nothing harmful — the system
    tells you it had no effect. Only the right fix
    moves a component to NOMINAL.

    ─── YOUR GOAL ───

    Bring all 8 components to NOMINAL state.

    Some may change state after your first fix —
    you may need to run your script more than once,
    or handle that case in your code.

    ─── HOW TO WORK ───

    Your script file is: elevator.py

    1. Type  edit    → open the file in your editor
    2. Write your program
    3. Save the file
    4. Come back here and type  run
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2480 COMPLETE  ★             ║
    ║   Midtown Elevator Grid — ALL NOMINAL       ║
    ╚══════════════════════════════════════════════╝

    All elevators are running again. Midtown
    residents can move freely. Well done.

    ─── WHAT YOU JUST DID ───

    You combined a for loop with conditionals to
    process 8 components automatically. Your
    program iterated through a list, checked each
    item's state, and took the right action.

    That is a powerful pattern: iterate, inspect,
    decide, act. Most real-world automation follows
    this same structure.

    ─── THE LIMITATION ───

    Your code works, but look at the structure.
    Inside your loop, you have a block of logic:
    get the state, check if it is stuck, check if
    it is unstable, take the right action. That
    block is the "fix a component" procedure.

    Now imagine you need that same procedure in a
    different script, or in a different part of
    this script. You would have to copy the whole
    block again. If you later need to change how
    fixing works, you would need to find and
    update every copy.

    What if you could give that block a name and
    call it whenever you need it — like a command
    you wrote yourself?

    ─── NEW TOOL: FUNCTION DEFINITIONS ───

    You can now create your own commands using def.

    The keyword def is followed by a name you
    choose, then (), then a colon. The indented
    lines underneath are the instructions that run
    when you call it:

        def <your_name>():
            <step 1>
            <step 2>
            <step 3>

    Writing a def does not run the code inside it.
    It teaches the computer a new command. The code
    only runs when you call the name later with ():

        <your_name>()

    You can name it anything descriptive. Once
    defined, you can call it as many times as you
    want, from anywhere in your program — including
    inside loops.

    You have been using functions since your very
    first contract. rebalance, scan_drones,
    check_slot — those were all functions someone
    else wrote. Now you can write your own.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2480 COMPLETE — Elevators Nominal ★\n"
            "New tool unlocked: def (function definitions)\n"
        )
