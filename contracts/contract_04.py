from contracts.base import BaseContract
from systems.transit_signals import TransitSignals


class Contract04(BaseContract):
    CONTRACT_ID = "contract_04"
    TITLE = "Signal Interference"
    LOCATION = "Transit Hub"
    SCRIPT_FILE = "player_scripts/signals.py"

    def __init__(self):
        super().__init__()
        self.signals = TransitSignals()

    MAX_CALLS = 25

    def get_commands(self):
        return {
            "check_signal": self.signals.check_signal,
            "reset_signal": self.signals.reset_signal,
            "calibrate_signal": self.signals.calibrate_signal,
            "submit_report": self.signals.submit_report,
        }

    def reset_system(self):
        self.signals = TransitSignals()

    def is_goal_met(self):
        return self.signals.is_goal_met()

    def get_status_text(self):
        return self.signals.get_status_text()

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2480 – Signal Interference      ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    The Transit Hub's signal controllers are down.
    There are 6 signals, numbered 1 through 6.
    Each one is either STUCK or SCRAMBLED — you
    know how to handle both types from last time.

    Here is what is different: the signals are
    numbered, and each command needs a signal
    number to know which one you mean:

        check_signal(1)
        reset_signal(3)
        calibrate_signal(5)

    The number in the parentheses tells the command
    which signal to work on. This is called an
    argument — a value you pass to a command.

    After fixing all 6 signals, you must call
    submit_report() to log the work. The report
    can only be submitted after all signals are
    fixed — so your loop must end for your program
    to reach it.

    ─── YOUR COMMANDS ───

        check_signal(n)      — shows and returns
                               the signal's state
        reset_signal(n)      — fixes STUCK signals
        calibrate_signal(n)  — fixes SCRAMBLED signals
        submit_report()      — log your completed work

    Signal numbers are 1 through 6.

    ─── YOUR GOAL ───

    Fix all 6 signals and submit the report.

    ─── HOW TO WORK ───

    Your script file is: signals.py

    1. Type  edit    → open the file in your editor
    2. Write your program
    3. Save the file
    4. Come back here and type  run
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2480 COMPLETE  ★             ║
    ║   Transit Hub — ALL SIGNALS FIXED           ║
    ╚══════════════════════════════════════════════╝

    All signals are operational and the report is
    filed. Transit Hub sends payment.

    ─── WHAT YOU JUST DID ───

    You wrote a program that processes numbered
    items using a controlled loop, passes arguments
    to commands, makes decisions for each item, and
    then continues to the next step after the loop
    ends. That is real automation.

    ─── HOW FAR YOU HAVE COME ───

    Think back to your first contract. You wrote
    one command on each line, over and over.

    Now you write programs that store data in
    variables, make decisions with conditionals,
    control loops with conditions, and pass data
    to commands as arguments.

    These are the fundamentals of programming.
    Every language, every system, every tool you
    will ever use builds on these ideas.

    ─── WHAT COMES NEXT ───

    You have been writing scripts to fix systems.
    But some problems cannot be diagnosed from a
    script alone.

    The city's infrastructure runs on terminals —
    systems with directories full of logs, config
    files, and diagnostic data. Sometimes you need
    to connect directly to a system's terminal,
    navigate its files, and find what is wrong by
    hand before you can write the fix.

    That is a different kind of skill. Not writing
    code — but reading systems. Navigating them.
    Searching through data to find what matters.

    New contracts are coming.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2480 COMPLETE — Transit Signals Fixed ★\n"
            "Python fundamentals mastered.\n"
        )
