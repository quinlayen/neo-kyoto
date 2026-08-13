from contracts.base_combined import BaseCombinedContract
from systems.virtual_fs import VirtualFilesystem
from systems.water_processor import WaterProcessor


class Contract10(BaseCombinedContract):
    CONTRACT_ID = "contract_10"
    TITLE = "Water Treatment"
    LOCATION = "Underground Plant"
    SCRIPT_FILE = "player_scripts/treatment.py"
    MAX_CALLS = 60
    BASE_CREDITS = 250
    STAR_THRESHOLDS = (32, 42)

    def __init__(self):
        self.water = None
        super().__init__()

    def build_filesystem(self):
        fs = VirtualFilesystem()

        fs.add_dir("/home/contractor/Desktop")
        fs.add_dir("/home/contractor/Documents")
        fs.add_dir("/home/contractor/Downloads")
        fs.add_dir("/home/contractor/Music")
        fs.add_dir("/home/contractor/Pictures")
        fs.add_dir("/home/contractor/Public")
        fs.add_dir("/home/contractor/Templates")
        fs.add_dir("/home/contractor/Videos")

        fs.add_file("/home/contractor/.bashrc",
                     "# contractor shell config\n"
                     "export PS1='contractor@neo-kyoto:\\w$ '\n")

        fs.add_file("/home/contractor/notes.txt",
                     "WATER TREATMENT — URGENT\n"
                     "────────────────────────\n"
                     "Contamination detected in the water\n"
                     "recycling system. Multiple pump stations\n"
                     "are affected.\n"
                     "\n"
                     "The repair procedure should be in the\n"
                     "maintenance docs under /opt/water/ —\n"
                     "but access got locked down after the\n"
                     "contamination alert.\n"
                     "\n"
                     "Find the procedure, then write a script\n"
                     "to automate the repairs.\n")

        fs.add_dir("/opt/water", "r-x------")
        fs.add_file("/opt/water/.repair_procedure.txt",
                     "WATER SYSTEM REPAIR PROCEDURE\n"
                     "─────────────────────────────\n"
                     "\n"
                     "Each contaminated unit must be repaired\n"
                     "using a strict 4-step sequence:\n"
                     "\n"
                     "    1. drain(unit_id)\n"
                     "    2. flush(unit_id)\n"
                     "    3. refill(unit_id)\n"
                     "    4. restart(unit_id)\n"
                     "\n"
                     "Steps must be done IN ORDER. The system\n"
                     "will reject out-of-sequence commands.\n"
                     "\n"
                     "Use get_broken_stations() to see which\n"
                     "pump stations need repair.\n"
                     "\n"
                     "After all stations are fixed, check for\n"
                     "secondary failures in the intake system.\n")

        fs.add_file("/opt/water/.commands.txt",
                     "WATER SYSTEM COMMANDS\n"
                     "─────────────────────\n"
                     "scan_system()             show all units\n"
                     "get_broken_stations()     list of broken\n"
                     "                          pump station IDs\n"
                     "get_broken_valves()       list of broken\n"
                     "                          intake valve IDs\n"
                     "drain(unit_id)            step 1: drain\n"
                     "flush(unit_id)            step 2: flush\n"
                     "refill(unit_id)           step 3: refill\n"
                     "restart(unit_id)          step 4: restart\n",
                     "---------")

        fs.add_file("/var/log/water.log",
                     "2189-08-12 04:10:00 [CRIT]  Contamination detected in recycler\n"
                     "2189-08-12 04:10:01 [CRIT]  Multiple pump stations affected\n"
                     "2189-08-12 04:10:02 [WARN]  Standard repair procedure required\n"
                     "2189-08-12 04:10:03 [WARN]  Each unit: drain → flush → refill → restart\n"
                     "2189-08-12 04:11:00 [INFO]  Contractor dispatched\n")

        return fs

    def on_command(self, command_line):
        output = self.terminal.execute(command_line)

        parts = command_line.strip().split()
        if parts and parts[0] == "cat":
            for arg in parts[1:]:
                resolved = self.fs.resolve_path(arg)
                if resolved == "/opt/water/.commands.txt":
                    node = self.fs.get_node(resolved)
                    if node and self.fs._has_permission(node, "r"):
                        self.bonus_found.add("commands_file")

        self.update_completion()
        return output

    def reset_game_system(self):
        self.water = WaterProcessor()

    def reset_system(self):
        self.reset_game_system()
        super().reset_system()

    def get_commands(self):
        return {
            "scan_system": self.water.scan_system,
            "get_broken_stations": self.water.get_broken_stations,
            "get_broken_valves": self.water.get_broken_valves,
            "drain": self.water.drain,
            "flush": self.water.flush,
            "refill": self.water.refill,
            "restart": self.water.restart,
        }

    def is_goal_met(self):
        return self.water.is_goal_met()

    def get_status_text(self):
        return self.water.get_status_text()

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2487 – Water Treatment          ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    Contamination in the underground water
    recycling plant. Multiple pump stations are
    down. Each one needs the same multi-step
    repair procedure.

    The maintenance docs are somewhere under
    /opt/water/ — the directory got restricted
    after the contamination alert. Find the
    repair procedure and command reference.

    Then write a script to fix everything.

    Terminal commands and scripting (edit/run)
    are both available.

    ─── YOUR GOAL ───

    Bring all water systems back online.

    Type  exit  to return to the contract board.
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2487 COMPLETE  ★             ║
    ║   Underground Plant — WATER RESTORED        ║
    ╚══════════════════════════════════════════════╝

    All pump stations and intake valves are
    operational. Clean water is flowing again.

    ─── WHAT YOU JUST DID ───

    You wrote a script that performed the same
    4-step repair procedure on multiple units.
    For each broken station: drain, flush,
    refill, restart. Then you did it again for
    the intake valves.

    Look at your code. You probably wrote the
    same four lines twice — once for stations,
    once for valves. Maybe more if you had to
    restructure.

    ─── THE LIMITATION ───

    Those four repair steps are the same sequence
    both times. The only difference is which list
    of units you are looping over.

    If there were ten subsystems that all needed
    the same procedure, you would copy that block
    ten times. That is not just tedious — it is
    fragile. Change one step and you have to find
    and fix it everywhere.

    What you need is a way to give that sequence
    a name, write it once, and call it whenever
    you need it.

    ─── NEW TOOL: FUNCTIONS ───

    You can now define your own commands using
    def:

        def repair_unit(unit_id):
            drain(unit_id)
            flush(unit_id)
            refill(unit_id)
            restart(unit_id)

    This creates a new command called repair_unit.
    The name in parentheses (unit_id) is called a
    parameter — it is a placeholder for whatever
    value you pass in when you call it:

        repair_unit("PS-01")
        repair_unit("IV-02")

    Each call runs the same four steps with a
    different unit ID. Write it once, use it
    anywhere.

    You can put a function definition at the top
    of your script, then call it as many times as
    you need below:

        def repair_unit(unit_id):
            drain(unit_id)
            flush(unit_id)
            refill(unit_id)
            restart(unit_id)

        stations = get_broken_stations()
        for s in stations:
            repair_unit(s)

        valves = get_broken_valves()
        for v in valves:
            repair_unit(v)

    The whole script is cleaner, shorter, and if
    the procedure ever changes, you fix it in one
    place.

    This is the most important tool in programming.
    Until now, you have been calling commands we
    built for you. From here on, you build your own.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2487 COMPLETE — Water Restored ★\n"
            "New tool unlocked: def (function definitions)\n"
        )
