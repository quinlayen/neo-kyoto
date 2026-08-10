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

    The Midtown elevator grid is in trouble. Five
    elevator components have entered various failure
    states and the building's residents are trapped
    on their floors.

    Each component needs different treatment depending
    on what went wrong. Some are STUCK — they need a
    hard reset. Others are UNSTABLE — they need a
    watchdog process to keep them steady. At least one
    component may need more than one step to recover.

    This is exactly the kind of problem that
    conditionals were made for.

    ─── USING CONDITIONALS ───

    You now have if and else. Here is how they apply
    to this job.

    get_state gives back a piece of text — the current
    state of a component, like "STUCK" or "UNSTABLE"
    or "NOMINAL". You can catch that text in a variable
    and then check it with an if statement.

    When you compare text, you put it in quotes on both
    sides of the ==. The computer checks whether the
    two pieces of text are exactly the same:

        if state == "STUCK":

    If the value in your variable matches the text
    "STUCK", the indented code underneath will run.
    If it does not match, the indented code is skipped.

    You can use multiple if statements in a row to
    check for different states. Each if is its own
    independent check — the computer evaluates each
    one separately.

    The general pattern for this contract is:
    get the state, check what it is, take the right
    action. Repeat that pattern for each component.

    ─── YOUR COMMANDS ───

        get_state(id)        — check a component's state
                               (returns text like "STUCK")
        reset_component(id)  — reset a STUCK component
        set_watchdog(id)     — stabilize an UNSTABLE one

    Each command takes a component ID as an argument.
    The IDs are text values and must be in quotes:
    "E-01", "E-02", "E-03", "E-04", "E-05"

    Applying the wrong fix does nothing harmful — the
    system will just tell you it had no effect. But
    only the right fix for the right state will move
    a component to NOMINAL.

    ─── YOUR GOAL ───

    Bring all 5 components to NOMINAL state.

    Check the status display to see what state each
    component is in. Some may need a reset, others a
    watchdog, and at least one may change state after
    your first fix — so you may need to check it again
    and take a second action.

    ─── HOW TO WORK ───

    Your script file is: elevator.py

    1. Type  edit    → open the file in your editor
    2. Write your program
    3. Save the file
    4. Come back here and type  run

    If not all components are NOMINAL after one run,
    check the status, adjust your script, and run again.
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

    That is the core of almost all real software: look
    at data, decide what to do, act. Your programs are
    no longer just repeating the same thing — they are
    thinking.

    ─── THE LIMITATION ───

    Look at your code. You probably wrote the same
    check-and-act block five times — once per component,
    changing only the ID string each time. The logic
    was identical. Only the data was different.

    What if there were 50 components? 500? Writing the
    same block hundreds of times is not practical. You
    need a way to say "here is a collection of items —
    do this same thing for each one."

    ─── NEW TOOL: LISTS ───

    A list is a way to hold multiple values under one
    name. You create a list with square brackets, and
    separate the items with commas:

        ids = ["E-01", "E-02", "E-03"]

    After this line, the name ids holds all three
    values together. A list can hold numbers, text,
    or any mix of values. The items stay in the order
    you wrote them.

    len() tells you how many items are in a list:

        len(ids)

    This gives you 3 — because there are three items.

    ─── NEW TOOL: FOR LOOPS ───

    A for loop repeats a block of code once for each
    item in a collection. It works differently from
    a while loop.

    A while loop repeats "as long as a condition is
    true." A for loop repeats "once for each item."
    You do not need to manage a counter or worry about
    when to stop — the for loop handles that for you.

    The structure looks like this:

        for item in collection:
            <do something with item>

    The variable before "in" (here called "item") is
    automatically set to each value in the collection,
    one at a time. The first time through the loop, it
    holds the first value. The second time, the second
    value. And so on, until every value has been used.

    You can use any name you want for that variable.
    Pick something that describes what each item is.

    ─── range() ───

    Sometimes you do not have a list of items — you
    just want to repeat something a specific number
    of times. range() creates a sequence of numbers
    for you:

        range(10)

    This produces the numbers 0, 1, 2, 3, 4, 5, 6,
    7, 8, 9 — ten numbers total, starting from 0.

    You can use it with a for loop to repeat code a
    fixed number of times. The variable takes on each
    number in the sequence, one at a time.

    You can also give range a starting number:

        range(1, 6)

    This produces 1, 2, 3, 4, 5 — starting at 1 and
    stopping before 6.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2480 COMPLETE — Elevators Nominal ★\n"
            "New tools unlocked: for loops, lists, range(), len()\n"
        )
