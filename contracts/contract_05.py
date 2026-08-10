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

    Your biggest job yet. An automated assembly cell in
    the Industrial Zone has gone offline. The cell needs
    to complete 10 full production cycles to fill a
    backlogged order.

    Each production cycle has four stages that must run
    in exact order:

        1. harvest   — gather raw materials
        2. process   — refine the materials
        3. package   — prepare for shipping
        4. ship      — send the finished product

    Calling a stage out of order will fail. The pipeline
    is strict about sequence.

    ─── YOUR FULL TOOLKIT ───

    You now have everything you have unlocked so far:
    loops, variables, conditionals, lists, for loops,
    range(), and len().

    Think about which tools are right for this job.
    The pipeline is repetitive and predictable — you
    know exactly what needs to happen and how many
    times.

    ─── YOUR COMMANDS ───

        harvest()         — stage 1: gather materials
        process()         — stage 2: refine materials
        package()         — stage 3: prepare shipment
        ship()            — stage 4: send product
        check_pipeline()  — view cycle progress

    ─── YOUR GOAL ───

    Complete 10 full production cycles.

    Each cycle is the same four steps in the same order.
    Figure out how to automate the full run.

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

    You built a complete automated pipeline. Your program
    coordinated multiple steps in the right sequence,
    repeated them the exact number of times needed, and
    finished the job without any manual intervention.

    That is real automation. That is what keeps a city
    like Neo-Kyoto running.

    ─── THE LIMITATION ───

    Your pipeline code works, but look at it. Every
    cycle is the same four steps written out. What if
    you needed those same steps in a different script?
    You would have to copy them all over again.

    What if you could name a group of steps — give them
    a label — and then call that label like a command?

    ─── NEW TOOLS UNLOCKED ───

    You can now define your own commands using def.

        def run_cycle():
            harvest()
            process()
            package()
            ship()

    This creates a new command called run_cycle. The
    indented code underneath is what happens when you
    call it. Then you can write:

        for i in range(10):
            run_cycle()

    You started with one command: rebalance(). Now you
    can create your own. That is the power of functions.

    ─── END OF CURRENT CONTRACTS ───

    You have completed all available contracts.

    You started by pressing a single button over and over.
    Now you write programs that store data, make decisions,
    iterate over collections, and define reusable commands.

    Neo-Kyoto's systems are in better hands with you
    on the job. More contracts will come.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2481 COMPLETE — Assembly Cell Online ★\n"
            "New tools unlocked: def (function definitions)\n"
            "All current contracts complete.\n"
        )
