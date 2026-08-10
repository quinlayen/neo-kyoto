from contracts.base import BaseContract
from systems.assembly_line import AssemblyLine


class Contract05(BaseContract):
    CONTRACT_ID = "contract_05"
    TITLE = "Assembly Automation"
    LOCATION = "Industrial Zone"
    SCRIPT_FILE = "player_scripts/assembly.py"

    def __init__(self):
        super().__init__()
        self.line = AssemblyLine()

    def get_commands(self):
        return {
            "harvest": self.line.harvest,
            "process": self.line.process,
            "package": self.line.package,
            "ship": self.line.ship,
            "check_pipeline": self.line.check_pipeline,
        }

    def is_goal_met(self):
        return self.line.is_goal_met()

    def get_status_text(self):
        return self.line.get_status_text()

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2481 – Assembly Automation      ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    Your biggest job yet. An automated assembly cell
    in the Industrial Zone has gone offline. The cell
    needs to complete 10 full production cycles to
    fill a backlogged order.

    Each production cycle has four stages that must
    run in exact order:

        1. harvest   — gather raw materials
        2. process   — refine the materials
        3. package   — prepare for shipping
        4. ship      — send the finished product

    The pipeline is strict about sequence. If you try
    to process before harvesting, or ship before
    packaging, the system will reject the command and
    tell you what step it expects next.

    ─── THINKING ABOUT THE PROBLEM ───

    Before you start coding, think about the structure
    of this job.

    What repeats? The whole cycle of four steps repeats
    10 times. The steps within each cycle are always
    the same and always in the same order.

    What tool handles repeating something a known
    number of times? You unlocked that in the last
    contract.

    What goes inside the loop? One complete cycle —
    all four stages, called in order.

    Start simple: try writing just one cycle first.
    Once that works, wrap it in the right kind of
    repetition.

    Use check_pipeline() at any point to see how many
    cycles have been completed.

    ─── YOUR COMMANDS ───

        harvest()         — stage 1: gather materials
        process()         — stage 2: refine materials
        package()         — stage 3: prepare shipment
        ship()            — stage 4: send product
        check_pipeline()  — view cycle progress

    ─── YOUR GOAL ───

    Complete 10 full production cycles.

    ─── HOW TO WORK ───

    Your script file is: assembly.py

    1. Type  edit    → open the file in your editor
    2. Write your program
    3. Save the file
    4. Come back here and type  run
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2481 COMPLETE  ★             ║
    ║   Assembly Cell 9 — ORDER FULFILLED         ║
    ╚══════════════════════════════════════════════╝

    10 production cycles completed. The backlog is
    cleared. Industrial Zone management is impressed —
    this was your most complex automation yet.

    ─── WHAT YOU JUST DID ───

    You built a complete automated pipeline. Your
    program coordinated multiple steps in the right
    sequence, repeated them the exact number of times
    needed, and finished the job without any manual
    intervention.

    That is real automation. That is what keeps a
    city like Neo-Kyoto running.

    ─── THE LIMITATION ───

    Your pipeline code works, but look at it. The
    four steps of a production cycle are written out
    inside the loop. What if you needed those same
    four steps in a different script? You would have
    to copy all of them again.

    What if the cycle had 20 steps instead of 4?
    Your loop body would be enormous. And if you
    needed to change one step, you would have to find
    and fix it everywhere you copied it.

    What if you could take a group of steps, give
    that group a name, and then call that name like
    a command — just like calling rebalance() or
    harvest()? Write the steps once, use them
    wherever you need them.

    ─── NEW TOOL: FUNCTION DEFINITIONS ───

    You can now create your own commands using def.

    The keyword def is followed by a name you choose,
    then (), then a colon. The indented lines underneath
    are the instructions that run when you call it:

        def <your_name>():
            <step 1>
            <step 2>
            <step 3>

    Writing a def does not run the code inside it.
    It just teaches the computer a new command. The
    code only runs when you call the name later with
    parentheses, just like any other command:

        <your_name>()

    You can name it anything that makes sense. Good
    names describe what the group of steps does. Once
    defined, you can call it as many times as you want,
    from anywhere in your program — including inside
    loops.

    This is called a function. You have been using
    functions since your very first contract — rebalance,
    scan_drones, check_slot were all functions that
    someone else wrote. Now you can write your own.

    ─── END OF CURRENT CONTRACTS ───

    You have completed all available contracts.

    You started by pressing a single button over and
    over. Now you write programs that store data, make
    decisions, iterate over collections, and define
    your own reusable commands.

    Neo-Kyoto's systems are in better hands with you
    on the job. More contracts will come.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2481 COMPLETE — Assembly Cell Online ★\n"
            "New tools unlocked: def (function definitions)\n"
            "All current contracts complete.\n"
        )
