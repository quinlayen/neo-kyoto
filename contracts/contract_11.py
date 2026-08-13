from contracts.base_combined import BaseCombinedContract
from systems.virtual_fs import VirtualFilesystem
from systems.production_floor import ProductionFloor


class Contract11(BaseCombinedContract):
    CONTRACT_ID = "contract_11"
    TITLE = "Sector Sweep"
    LOCATION = "Industrial Zone"
    SCRIPT_FILE = "player_scripts/factory.py"
    MAX_CALLS = 50
    BASE_CREDITS = 300
    STAR_THRESHOLDS = (33, 40)

    def __init__(self):
        self.floor = None
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
                     "SECTOR SWEEP — PRODUCTION FLOOR\n"
                     "───────────────────────────────\n"
                     "The factory district's production lines\n"
                     "keep stalling. 12 lines total, most of\n"
                     "them broken.\n"
                     "\n"
                     "Two types of failure: JAMMED and\n"
                     "OVERHEATED. Each needs a different fix\n"
                     "before the line can restart.\n"
                     "\n"
                     "The repair manual is under /opt/factory/\n"
                     "— look for the diagnostic procedure.\n"
                     "\n"
                     "You have def now. Use it.\n")

        fs.add_dir("/opt/factory", "r-x------")
        fs.add_file("/opt/factory/.diagnostic_manual.txt",
                     "PRODUCTION LINE REPAIR MANUAL\n"
                     "─────────────────────────────\n"
                     "\n"
                     "Each broken line must be diagnosed and\n"
                     "repaired based on its failure mode:\n"
                     "\n"
                     "  1. diagnose(line_id) → returns the\n"
                     "     failure mode: \"JAMMED\" or\n"
                     "     \"OVERHEATED\"\n"
                     "\n"
                     "  2. If JAMMED:     clear_jam(line_id)\n"
                     "     If OVERHEATED: cool_down(line_id)\n"
                     "\n"
                     "  3. restart_line(line_id)\n"
                     "\n"
                     "Using the wrong fix (clear_jam on an\n"
                     "overheated line, or cool_down on a\n"
                     "jammed one) will be rejected.\n"
                     "\n"
                     "─── SUGGESTION ───\n"
                     "\n"
                     "This is the kind of procedure you will\n"
                     "use again and again. Consider writing\n"
                     "a function that handles the diagnose →\n"
                     "fix → restart sequence for any line.\n"
                     "\n"
                     "Then call it in a loop.\n")

        fs.add_file("/opt/factory/.commands.txt",
                     "PRODUCTION FLOOR COMMANDS\n"
                     "────────────────────────\n"
                     "scan_floor()              show all lines\n"
                     "get_broken_lines()        returns a list of\n"
                     "                          broken line IDs\n"
                     "diagnose(line_id)         returns the\n"
                     "                          failure mode\n"
                     "clear_jam(line_id)        fix a JAMMED line\n"
                     "cool_down(line_id)        fix OVERHEATED\n"
                     "restart_line(line_id)     restart after fix\n",
                     "---------")

        fs.add_file("/var/log/factory.log",
                     "2189-08-12 06:00:00 [CRIT]  Production floor shutdown\n"
                     "2189-08-12 06:00:01 [CRIT]  Multiple line failures\n"
                     "2189-08-12 06:00:02 [INFO]  Failure modes: JAMMED, OVERHEATED\n"
                     "2189-08-12 06:00:03 [INFO]  Failures are randomized — cannot hardcode\n"
                     "2189-08-12 06:01:00 [INFO]  Contractor dispatched\n")

        return fs

    def on_command(self, command_line):
        output = self.terminal.execute(command_line)

        parts = command_line.strip().split()
        if parts and parts[0] == "cat":
            for arg in parts[1:]:
                resolved = self.fs.resolve_path(arg)
                if resolved == "/opt/factory/.diagnostic_manual.txt":
                    self.bonus_found.add("diagnostic_manual")

        self.update_completion()
        return output

    def reset_game_system(self):
        self.floor = ProductionFloor()

    def reset_system(self):
        self.reset_game_system()
        super().reset_system()

    def get_commands(self):
        return {
            "scan_floor": self.floor.scan_floor,
            "get_broken_lines": self.floor.get_broken_lines,
            "diagnose": self.floor.diagnose,
            "clear_jam": self.floor.clear_jam,
            "cool_down": self.floor.cool_down,
            "restart_line": self.floor.restart_line,
        }

    def is_goal_met(self):
        return self.floor.is_goal_met()

    def get_status_text(self):
        return self.floor.get_status_text()

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2488 – Sector Sweep             ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    The factory district's production floor is
    down. 12 lines, most of them broken. Two
    failure modes — JAMMED and OVERHEATED — each
    needs a different fix.

    The failures are randomized. You cannot
    hardcode this. Your script must diagnose
    each line and choose the right repair.

    The diagnostic manual is under /opt/factory/.
    You know the drill — find it, read it, then
    write a script.

    You have a new tool: def. You can write your
    own functions now. This is the kind of job
    where that matters.

    Terminal commands and scripting (edit/run)
    are both available.

    ─── YOUR GOAL ───

    Bring all 12 production lines back online.

    Type  exit  to return to the contract board.
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2488 COMPLETE  ★             ║
    ║   Industrial Zone — ALL LINES OPERATIONAL   ║
    ╚══════════════════════════════════════════════╝

    All 12 production lines are running. The
    factory district is back at full capacity.

    ─── WHAT YOU JUST DID ───

    You wrote your own function — a reusable
    command that you designed yourself. It
    diagnosed each line, chose the right fix
    based on the failure mode, and restarted it.

    Then you called that function in a loop for
    every broken line. The function handled the
    complexity; the loop handled the repetition.

    ─── THE SHIFT ───

    Until now, every command you used was one
    we built for you: rebalance(), repair(),
    drain(), flush(). You called our tools.

    This time, you built the tool. You decided
    what it does, what it takes as input, and
    how it handles different cases. Then you
    used it just like any other command.

    That is what functions are. Not a syntax
    trick — a way of thinking. See a pattern,
    name it, reuse it.

    From here on, contracts will give you
    lower-level building blocks instead of
    ready-made solutions. You write the logic.
    You build the tools. That is engineering.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2488 COMPLETE — Production Floor Online ★\n"
            "Functions mastered: you build the tools now.\n"
        )
