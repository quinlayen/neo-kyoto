from contracts.base import BaseContract
from systems.power_node import PowerNode


class Contract01(BaseContract):
    CONTRACT_ID = "contract_01"
    TITLE = "Keep the Lights On"
    LOCATION = "Block 7"
    SCRIPT_FILE = "player_scripts/block7.py"

    def __init__(self):
        super().__init__()
        self.node = PowerNode()

    def get_commands(self):
        return {"rebalance": self.node.rebalance}

    def is_goal_met(self):
        return self.node.is_goal_met()

    def get_status_text(self):
        return self.node.get_status_text()

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2477 – Keep the Lights On       ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    Welcome to Neo-Kyoto. The year is 2189.

    This city runs on thousands of automated systems — power
    grids, cargo drones, water recyclers, transit networks.
    When those systems break, people like you get the call.

    You are not a hacker. You are not a soldier.
    You are an engineer. You write small programs that tell
    machines what to do, and right now a machine needs help.

    ─── YOUR FIRST JOB ───

    Block 7's power node is flickering. The residents are
    losing power every few minutes. The node's auto-repair
    failed, and the district needs someone to stabilize it
    manually — by writing a short program.

    That is what you will do today: write your first program.

    ─── WHAT IS A PROGRAM? ───

    A program is a set of instructions, written out line by
    line. The computer reads your instructions from top to
    bottom and follows them in order — exactly like a recipe.

    Each instruction goes on its own line.
    The computer does line 1 first, then line 2, then line 3,
    and so on until it reaches the end.

    That is all a program is: a list of steps.

    ─── COMMANDS ───

    An instruction tells the computer to do one specific thing.
    In programming, we write instructions as commands that
    look like this:

        rebalance()

    Let's break that down:

        rebalance   ← the name of the command
        ()          ← tells the computer "do it now"

    The parentheses are important. Without them, the computer
    just sees a name but does not actually do anything.

    Think of it like a button: "rebalance" is the label,
    and "()" is you pressing it.

    ─── YOUR ONLY TOOL (FOR NOW) ───

    Right now, you have access to one command:

        rebalance()

    Each time this runs, it tells the power node to
    recalculate and redistribute its load. One call helps
    a little, but the node is too unstable for a single
    rebalance to fix it.

    You will need to call this command several times in a row.

    ─── YOUR GOAL ───

    Bring the power node from FLICKERING to STABLE.

    Remember: a program is a list of steps, and each step
    goes on its own line. One rebalance is not enough —
    figure out how many the node needs.

    ─── HOW TO WORK ───

    Your program lives in a file called block7.py.

    1. Type  edit    → open the file in your editor
    2. Write your program
    3. Save the file
    4. Come back here and type  run

    You can run your program as many times as you want.
    If it does not work the first time, edit and try again.

    ─── READY? ───

    The residents of Block 7 are waiting.
    Type a command to begin.
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2477 COMPLETE  ★             ║
    ║   Block 7 Power Node — STABLE               ║
    ╚══════════════════════════════════════════════╝

    Power restored. The lights in Block 7 are steady again.
    District management has logged your work. Payment processed.

    ─── WHAT YOU JUST DID ───

    You wrote a program. A real one.

    You gave the computer a list of instructions, it read them
    from top to bottom, and it followed every one. That is how
    all software works — from the simplest script to the systems
    that run this city.

    ─── A NEW TOOL: LOOPS ───

    Look at the program you just wrote. You probably typed
    the same line several times. It worked, but imagine if
    the node needed 100 rebalances. Writing 100 identical
    lines would be painful.

    Programmers solve this with something called a loop.
    A loop tells the computer: "keep repeating these steps."

    You just unlocked a new keyword:  while

    The syntax works like this:

        while <condition>:
            <indented instructions go here>

    The colon at the end of the while line is required.
    Any instructions indented underneath it will be repeated
    as long as the condition is true.

    Hint: the value True (capital T) is always true.

    ─── TRY IT ───

    Edit your script and see if you can replace your repeated
    lines with a loop. The sandbox will safely stop your
    program after a few dozen cycles, so do not worry about
    it running forever.

    If you get it right, two lines can do what many did before.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2477 COMPLETE — Block 7 Stable ★\n"
            "New tool unlocked: while True loops\n"
            "Try rewriting your script to use a loop instead of repeated lines.\n"
        )
