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

    MAX_CALLS = 50

    def get_commands(self):
        return {
            "harvest": self.line.harvest,
            "process": self.line.process,
            "package": self.line.package,
            "ship": self.line.ship,
            "check_pipeline": self.line.check_pipeline,
        }

    def reset_system(self):
        self.line = AssemblyLine()

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
    cleared. Industrial Zone management is impressed
    — this was your most complex automation yet.

    ─── WHAT YOU JUST DID ───

    You built a complete automated pipeline. Your
    program defined a reusable function, called it
    inside a loop, and coordinated multiple steps
    in sequence without any manual intervention.

    That is real automation. That is what keeps a
    city like Neo-Kyoto running.

    ─── HOW FAR YOU HAVE COME ───

    Think back to your first contract. You wrote
    one command on each line, over and over.

    Now you write programs that store data in
    variables, make decisions with conditionals,
    iterate over collections with for loops, and
    organize logic into reusable functions.

    These are the fundamentals of programming.
    Every language, every system, every tool you
    will ever use builds on these ideas.

    ─── WHAT COMES NEXT ───

    You have proven yourself as a programmer. But
    Neo-Kyoto runs on more than scripts.

    The city's infrastructure depends on terminals,
    databases, and version control systems. There
    are contractors who navigate file systems from
    a command line, query city data with SQL, and
    track system changes with Git.

    Those tools are coming. And when they arrive,
    the programming skills you have built here will
    be the foundation for everything else.

    More contracts will come.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2481 COMPLETE — Assembly Cell Online ★\n"
            "All current contracts complete.\n"
        )
