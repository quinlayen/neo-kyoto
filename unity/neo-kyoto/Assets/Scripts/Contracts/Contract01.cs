using System.Collections.Generic;
using NeoKyoto.Interpreter;
using NeoKyoto.Systems;

namespace NeoKyoto.Contracts
{
    public class Contract01 : Contract
    {
        public override string Id { get { return "contract_01"; } }
        public override string Title { get { return "Keep the Lights On"; } }
        public override string Location { get { return "Block 7"; } }
        public override string ScriptName { get { return "block7.py"; } }

        public PowerNode Node { get; private set; }

        public Contract01() { ResetSystem(); }

        public override void ResetSystem()
        {
            Node = new PowerNode();
            Node.Output = Print;
            Node.Changed += RaiseSystemChanged;
            RaiseSystemChanged();
        }

        public override Dictionary<string, CommandFunc> GetCommands()
        {
            var cmds = new Dictionary<string, CommandFunc>();
            cmds["rebalance"] = delegate (List<object> args) { return Node.Rebalance(); };
            return cmds;
        }

        public override bool IsGoalMet() { return Node.IsGoalMet(); }
        public override string GetStatusText() { return Node.GetStatusText(); }

        public override string StarterScript
        {
            get
            {
                return @"# ── Block 7 Power Node ──────────────────────────
#
# Write your program below.
#
# Your available command:  rebalance()
#
# ────────────────────────────────────────────────
";
            }
        }

        public override string GetBriefing()
        {
            return @"
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2477 – Keep the Lights On       ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    Welcome to Neo-Kyoto. The year is 2189.

    This city runs on thousands of automated
    systems — power grids, cargo drones, water
    recyclers, transit networks. When those systems
    break, people like you get the call.

    You are an engineer. You write small programs
    that tell machines what to do. Right now,
    Block 7's power node is flickering and the
    residents are losing power. You need to
    stabilize it by writing a short program.

    ─── WHAT IS A PROGRAM? ───

    A program is a list of instructions the
    computer follows from top to bottom, one
    line at a time. A command looks like this:

        rebalance()

    The name says which command. The () tells
    it ""do it now."" Each command goes on its
    own line.

    ─── YOUR COMMAND ───

        rebalance()  — rebalances the power node

    One rebalance is not enough. The node needs
    many before it reaches a STABLE state.

    You also have print() — it displays text on
    screen. Useful for seeing what is happening.

    ─── YOUR GOAL ───

    Bring the power node from FLICKERING to
    STABLE. Your script file is: block7.py

    Write your program in the editor, then press
    RUN to execute it.
    ";
        }

        public override string GetCompletionMessage()
        {
            return @"
    ╔══════════════════════════════════════════════╗
    ║   ◆  CONTRACT #2477 COMPLETE  ◆             ║
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

    A loop tells the computer: ""repeat these
    instructions."" Instead of writing a command
    many times, you write it once and tell the
    computer to keep doing it.

    You just unlocked a new keyword:  while

    Here is how a while loop works, step by step:

    The computer reads the while line and checks
    the condition — a yes-or-no question. If the
    answer is yes (true), it runs the indented
    block underneath. Then it goes back to the
    while line and checks the condition again. If
    still true, it runs the block again. This
    cycle keeps repeating — check, run, check,
    run — until the condition becomes false. When
    it does, the loop stops and the program moves
    to the next line after the loop.

    The structure looks like this:

        while <condition>:
            <instructions to repeat>

    The indented lines underneath the while line
    are called the ""loop body."" These are the
    instructions that get repeated each cycle.

    Indentation means adding a tab or 4 spaces at
    the start of a line. This is how the computer
    knows which lines are inside the loop and
    which are not. Every line you want repeated
    must be indented. Lines that are not indented
    are outside the loop.

    The colon : at the end of the while line is
    required — it marks where the loop body begins.

    ─── WHAT IS THE CONDITION? ───

    The condition is anything that evaluates to
    true or false. The simplest condition is the
    word True (with a capital T), which is always
    true. A loop with True as its condition will
    repeat forever — this is called an infinite
    loop.

    An infinite loop sounds dangerous, but the
    sandbox will safely stop your program after
    enough cycles. So you can experiment without
    worrying about it running out of control.

    Later, you will learn to write conditions that
    can change — so the loop runs a specific number
    of times and then stops on its own. For now,
    True is all you need.

    ─── TRY IT ───

    Edit your script and see if you can replace
    your repeated lines with a loop. Think about
    what goes on the while line and what goes
    indented underneath.

    If you get it right, two lines of code can do
    what many lines did before. You can also add
    print() inside your loop to watch each step
    as it happens.
";
        }

        public override string GetCompletedBanner()
        {
            return "◆ CONTRACT #2477 COMPLETE — Block 7 Stable ◆\n" +
                   "New tool unlocked: while True loops\n" +
                   "Try rewriting your script to use a loop instead of repeated lines.\n";
        }
    }
}
