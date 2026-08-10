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

    This city runs on thousands of automated systems —
    power grids, cargo drones, water recyclers, transit
    networks. When those systems break, people like you
    get the call.

    You are not a hacker. You are not a soldier.
    You are an engineer. You write small programs that
    tell machines what to do, and right now a machine
    needs help.

    ─── YOUR FIRST JOB ───

    Block 7's power node is flickering. The residents
    are losing power every few minutes. The node's
    auto-repair failed, and the district needs someone
    to stabilize it manually — by writing a short program.

    That is what you will do today: write your first
    program.

    ─── WHAT IS A PROGRAM? ───

    Think of a recipe. A recipe is a list of steps,
    written in order. You follow step 1 first, then
    step 2, then step 3, and so on. If you skip a step
    or do them out of order, the result is wrong.

    A program works the same way. It is a list of
    instructions, written out line by line, that the
    computer follows from top to bottom.

    Each instruction goes on its own line. The computer
    does line 1 first, then line 2, then line 3, and
    so on until it reaches the end. Then it stops.

    That is all a program is: a list of steps that a
    computer follows in order. Nothing more.

    ─── WHAT IS A COMMAND? ───

    Each line of your program is a command — a single
    instruction that tells the computer to do one
    specific thing. In programming, a command looks
    like this:

        rebalance()

    There are two parts:

        rebalance   ← the name of the command
        ()          ← tells the computer "do it now"

    The name tells the computer which command you mean.
    The parentheses are what actually makes it run.
    Without the parentheses, the computer just sees a
    name and does nothing with it.

    Think of it like a button on a control panel.
    The label on the button is "rebalance." The
    parentheses are you pressing that button.

    Every command you write in your programs will
    follow this pattern: a name, followed by ().

    ─── YOUR ONLY TOOL (FOR NOW) ───

    Right now, you have access to one command:

        rebalance()

    Each time this command runs, it tells the power node
    to recalculate and redistribute its electrical load.

    One rebalance helps a little, but the node is too
    unstable for a single call to fix it. Each time you
    call rebalance(), the load drops a little and the
    node gets closer to stability. But it takes several
    calls before the node settles into a STABLE state.

    You will need to call this command more than once.

    ─── YOUR GOAL ───

    Bring the power node from FLICKERING to STABLE.

    Remember: a program is a list of steps, and each
    step goes on its own line. One rebalance is not
    enough — figure out how many the node needs.

    After you run your script, check the status display
    to see whether the node has stabilized. If it has
    not, edit your script, add more, and run again.

    ─── HOW TO WORK ───

    Your program lives in a file called block7.py.

    1. Type  edit    → open the file in your editor
    2. Write your program
    3. Save the file
    4. Come back here and type  run

    You can run your program as many times as you want.
    If it does not work the first time, edit and try
    again. Experimenting is how programmers work.

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

    Power restored. The lights in Block 7 are steady
    again. District management has logged your work.
    Payment processed.

    ─── WHAT YOU JUST DID ───

    You wrote a program — a real one. You gave the
    computer a list of instructions, it read them from
    top to bottom, and it followed every one in order.

    That is how all software works, from the simplest
    script to the systems that run this city. A list
    of steps. Nothing magical.

    ─── THE LIMITATION ───

    Look at the program you just wrote. You probably
    typed the same line several times in a row. It
    worked, but think about what would happen if the
    node needed 100 rebalances, or 1,000. Writing the
    same line hundreds of times is not just painful —
    it is error-prone and impractical.

    Whenever you find yourself writing the same thing
    over and over, there is almost always a better way.

    ─── NEW TOOL: LOOPS ───

    A loop tells the computer: "repeat these
    instructions." Instead of writing a command ten
    times, you write it once and tell the computer
    to keep doing it.

    You just unlocked a new keyword:  while

    A while loop has three parts:

    1. The keyword "while" — this tells the computer
       a loop is starting.

    2. A condition — this is a yes-or-no question that
       the computer checks each time before it repeats.
       As long as the answer is yes (true), the loop
       keeps going. When the answer is no (false), the
       loop stops and the program moves on.

    3. A colon : at the end of the while line — this
       marks where the loop body begins.

    Everything indented underneath the while line is
    the "loop body" — the instructions that get
    repeated. Indentation means adding spaces at the
    start of a line (usually 4 spaces or a tab). The spaces are
    how the computer knows which lines are inside the
    loop and which are not.

    The structure looks like this:

        while <condition>:
            <instructions to repeat>

    The condition can be anything that is true or
    false. The simplest condition is the word True
    (with a capital T), which is always true — meaning
    the loop will repeat forever.

    "Forever" sounds dangerous, but the sandbox will
    safely stop your program after enough cycles. So
    you can experiment without worrying about it
    running out of control.

    ─── TRY IT ───

    Edit your script and see if you can replace your
    repeated lines with a loop. Think about what goes
    on the while line and what goes indented underneath.

    If you get it right, two lines of code can do what
    many lines did before.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2477 COMPLETE — Block 7 Stable ★\n"
            "New tool unlocked: while True loops\n"
            "Try rewriting your script to use a loop instead of repeated lines.\n"
        )
