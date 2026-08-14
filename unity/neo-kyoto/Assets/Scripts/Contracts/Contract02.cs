using System.Collections.Generic;
using NeoKyoto.Interpreter;
using NeoKyoto.Systems;

namespace NeoKyoto.Contracts
{
    public class Contract02 : Contract
    {
        public override string Id { get { return "contract_02"; } }
        public override string Title { get { return "Drone Route Cleanup"; } }
        public override string Location { get { return "Sector 12"; } }
        public override string ScriptName { get { return "drone_zone.py"; } }

        public DroneRouter Router { get; private set; }

        public Contract02() { ResetSystem(); }

        public override void ResetSystem()
        {
            Router = new DroneRouter();
            Router.Output = Print;
            Router.Changed += RaiseSystemChanged;
            RaiseSystemChanged();
        }

        public override Dictionary<string, CommandFunc> GetCommands()
        {
            var cmds = new Dictionary<string, CommandFunc>();
            cmds["scan_drones"] = delegate (List<object> args) { return Router.ScanDrones(); };
            cmds["reroute_next"] = delegate (List<object> args) { return Router.RerouteNext(); };
            return cmds;
        }

        public override int BaseCredits { get { return 100; } }
        public override int ThreeStarCalls { get { return 10; } }
        public override int TwoStarCalls { get { return 15; } }

        public override bool IsGoalMet() { return Router.IsGoalMet(); }
        public override string GetStatusText() { return Router.GetStatusText(); }

        public override string StarterScript
        {
            get
            {
                return @"# ── Sector 12 Drone Grid ────────────────────────
#
# Your available commands:
#   scan_drones()   — shows all drone statuses
#   reroute_next()  — fixes the next misrouted drone
#
# ────────────────────────────────────────────────
";
            }
        }

        public override string GetBriefing()
        {
            return @"    ╔══════════════════════════════════════════════╗
    ║   ONCALL:// SYSTEMS CONTRACTOR              ║
    ║   Contract #2478 – Drone Route Cleanup      ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    Good work on Block 7. Word travels fast —
    Sector 12 needs your help now.

    The delivery drones in this sector are flying
    wrong routes. The routing table got corrupted
    and all 8 drones need manual correction.
" + PageBreak + @"
    ─── VARIABLES ───

    A variable is a name that holds a value. You
    create one with the = sign:

        speed = 30

    Now the name speed holds the number 30. You
    can use it anywhere in your program after that.

    Some commands give back a value when they run.
    This is called a return value. You can catch
    it in a variable:

        fixed = reroute_next()

    Now fixed holds whatever reroute_next() gave
    back. Use print() to see it:

        print(fixed)

    You do not strictly need variables for this
    contract, but practice using them — they
    become essential next contract.
" + PageBreak + @"
    ─── YOUR COMMANDS ───

        scan_drones()   — shows all drone statuses
        reroute_next()  — fixes the next misrouted
                          drone
" + PageBreak + @"
    ─── YOUR GOAL ───

    Correct all 8 misrouted drones.

    You could call reroute_next() eight times.
    But you have a loop now — think about how to
    let the computer handle the repetition.
" + PageBreak + @"
    ─── HOW TO WORK ───

    Your script file is: drone_zone.py

    1. Write your program in the editor
    2. Press RUN to execute it
    3. Watch the system respond, then adjust
       and run again
    ";
        }

        public override string GetCompletionMessage()
        {
            return @"    ╔══════════════════════════════════════════════╗
    ║   ◆  CONTRACT #2478 COMPLETE  ◆             ║
    ║   Sector 12 Drone Grid — ALL CORRECTED      ║
    ╚══════════════════════════════════════════════╝

    All drones are back on course. Deliveries are
    flowing again. Sector 12 management sends their
    thanks.

    ─── WHAT YOU JUST DID ───

    You used a loop to automate a repetitive task.
    Instead of writing the same command eight times,
    you wrote it once inside a loop and let the
    computer handle the repetition.

    If you used variables to catch what reroute_next()
    gave back, or printed values to see what was
    happening — good. Those skills will be critical
    from here on.
" + PageBreak + @"
    ─── THE LIMITATION ───

    Your loop fixed all 8 drones, but think about
    what it could NOT do.

    reroute_next() fixes drones in whatever order
    it finds them. But what if some drones were not
    just misrouted — what if some were GROUNDED and
    needed a completely different fix? Your program
    had no way to look at a drone's status and
    choose between two actions.

    scan_drones() showed you priorities and statuses
    on screen. You could read them. But your program
    could not make decisions based on what it saw. It
    did the same thing to every drone, regardless.

    What you need is a way for your program to ask
    a question — ""is this drone misrouted or
    grounded?"" — and take a different action depending
    on the answer.
" + PageBreak + @"
    ─── NEW TOOL: CONDITIONALS ───

    You can now use if and else to make decisions.

    An if statement asks a yes-or-no question. If
    the answer is yes (true), the indented code
    underneath runs. If the answer is no (false),
    the indented code is skipped entirely:

        if <condition>:
            <do this>

    The question you ask is called a condition. You
    write it using comparison operators — symbols
    that compare two values:

        ==   ""is the left equal to the right?""
        !=   ""is the left different from the right?""
        >    ""is the left greater than the right?""
        <    ""is the left less than the right?""

    Important: == and = are different things.

    A single = is assignment — it means ""store this
    value."" It is a statement, not a question:

        status = ""STUCK""     (store ""STUCK"" in status)

    A double == is comparison — it asks a question
    and the answer is either true or false:
" + PageBreak + @"
        status == ""STUCK""    (is status equal to ""STUCK""?)

    For example, you might check a variable:

        if status == ""STUCK"":
            <handle the stuck case>

    The computer looks at what is stored in status
    and asks ""is this equal to STUCK?"" If yes, the
    indented code runs. If no, it is skipped.

    You can add else to handle the other case:

        if status == ""STUCK"":
            <handle stuck>
        else:
            <handle everything else>

    The program takes one path or the other, never
    both. This is called branching — your program
    can now follow different routes depending on
    what it finds.

    You can also use multiple if statements in a
    row, each checking a different condition. Each
    one is its own independent question.";
        }

        public override string GetCompletedBanner()
        {
            return "◆ CONTRACT #2478 COMPLETE — Sector 12 Drones Corrected ◆\n" +
                   "New tool unlocked: if/else conditionals\n";
        }
    }
}
