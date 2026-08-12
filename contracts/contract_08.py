from contracts.base_combined import BaseCombinedContract
from systems.virtual_fs import VirtualFilesystem
from systems.power_grid import PowerGrid


class Contract08(BaseCombinedContract):
    CONTRACT_ID = "contract_08"
    TITLE = "Grid Restoration"
    LOCATION = "Central Grid"
    SCRIPT_FILE = "player_scripts/grid.py"
    MAX_CALLS = 30

    def __init__(self):
        self.grid = None
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
                     "GRID RESTORATION — URGENT\n"
                     "─────────────────────────\n"
                     "Major power failure across the city.\n"
                     "Multiple sectors are offline or degraded.\n"
                     "\n"
                     "The sector manifest is on the grid\n"
                     "control server at /opt/grid/manifest.txt\n"
                     "\n"
                     "Too many sectors to fix by hand. You\n"
                     "will need to write a script that loops\n"
                     "through the broken sectors and repairs\n"
                     "each one automatically.\n"
                     "\n"
                     "Use  edit  to open your script file.\n"
                     "Use  run   to execute it.\n")

        fs.add_file("/opt/grid/manifest.txt",
                     "═══════════════════════════════════════\n"
                     "  CENTRAL GRID — SECTOR MANIFEST\n"
                     "═══════════════════════════════════════\n"
                     "\n"
                     "  S-01    OFFLINE\n"
                     "  S-02    DEGRADED\n"
                     "  S-03    OFFLINE\n"
                     "  S-04    ONLINE\n"
                     "  S-05    OFFLINE\n"
                     "  S-06    DEGRADED\n"
                     "  S-07    OFFLINE\n"
                     "  S-08    ONLINE\n"
                     "  S-09    DEGRADED\n"
                     "  S-10    OFFLINE\n"
                     "\n"
                     "  8 sectors need repair.\n"
                     "  Sector IDs: S-01 through S-10\n"
                     "═══════════════════════════════════════\n")

        fs.add_file("/opt/grid/repair_protocol.txt",
                     "REPAIR PROTOCOL\n"
                     "───────────────\n"
                     "For each broken sector, call:\n"
                     "    repair(sector_id)\n"
                     "\n"
                     "Example:\n"
                     "    repair(\"S-01\")\n"
                     "\n"
                     "To check a sector's status:\n"
                     "    get_status(sector_id)\n"
                     "\n"
                     "To see the full grid:\n"
                     "    scan_grid()\n"
                     "\n"
                     "There are 10 sectors. Writing repair()\n"
                     "for each one by hand would work, but\n"
                     "there is a better way.\n"
                     "\n"
                     "A for loop lets you repeat code for\n"
                     "each item in a list:\n"
                     "\n"
                     "    for item in [\"a\", \"b\", \"c\"]:\n"
                     "        <do something with item>\n"
                     "\n"
                     "The variable before 'in' takes on each\n"
                     "value in turn. First loop: item is \"a\".\n"
                     "Second: \"b\". Third: \"c\".\n")

        fs.add_file("/var/log/grid.log",
                     "2189-08-11 02:30:00 [CRIT]  Cascade failure detected\n"
                     "2189-08-11 02:30:01 [CRIT]  S-01: OFFLINE\n"
                     "2189-08-11 02:30:01 [WARN]  S-02: DEGRADED\n"
                     "2189-08-11 02:30:02 [CRIT]  S-03: OFFLINE\n"
                     "2189-08-11 02:30:03 [CRIT]  S-05: OFFLINE\n"
                     "2189-08-11 02:30:03 [WARN]  S-06: DEGRADED\n"
                     "2189-08-11 02:30:04 [CRIT]  S-07: OFFLINE\n"
                     "2189-08-11 02:30:04 [WARN]  S-09: DEGRADED\n"
                     "2189-08-11 02:30:05 [CRIT]  S-10: OFFLINE\n"
                     "2189-08-11 02:31:00 [INFO]  Contractor dispatched\n")

        return fs

    def reset_system(self):
        self.grid = PowerGrid()
        super().reset_system()

    def get_commands(self):
        return {
            "scan_grid": self.grid.scan_grid,
            "get_status": self.grid.get_status,
            "repair": self.grid.repair,
        }

    def is_goal_met(self):
        return self.grid.is_goal_met()

    def get_status_text(self):
        return self.grid.get_status_text()

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2485 – Grid Restoration         ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    This is the big one. A cascade failure has
    knocked out most of the city's power grid.
    Multiple sectors are offline or degraded.

    This is a two-part job:

    FIRST — investigate. Use the terminal to
    check the grid manifest and logs. Find out
    which sectors are down and what needs fixing.

    THEN — automate. There are too many sectors
    to repair by hand. Write a Python script that
    loops through the broken sectors and repairs
    each one.

    You have both terminal commands AND Python
    scripting available in this contract.

    ─── TERMINAL COMMANDS ───

    All your terminal commands work here:
    ls, cd, cat, grep, and everything else.

    ─── SCRIPT COMMANDS ───

        edit          — open your script for editing
        run           — execute your script

    Your script can use:
        scan_grid()           — show all sectors
        get_status(sector_id) — check one sector
        repair(sector_id)     — repair a sector

    ─── YOUR GOAL ───

    Bring all 10 sectors online.

    Check the files in /opt/grid/ for the repair
    protocol and sector manifest.

    Type  exit  to return to the contract board.
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2485 COMPLETE  ★             ║
    ║   Central Grid — ALL SECTORS ONLINE         ║
    ╚══════════════════════════════════════════════╝

    The grid is back. All 10 sectors restored.
    Neo-Kyoto has power again.

    ─── WHAT YOU JUST DID ───

    You combined two skill sets. You used the
    terminal to investigate — navigating files,
    reading logs, finding the sector manifest.
    Then you wrote a Python script with a for
    loop to automate the repairs.

    Neither skill alone could have done this job
    efficiently. Investigation told you WHAT was
    broken. Automation fixed it all at once.

    This is how real engineers work. Diagnose
    with tools, then automate the fix.

    ─── FOR LOOPS ───

    The for loop you just used is one of the
    most powerful tools in programming:

        for sector in ["S-01", "S-02", "S-03"]:
            repair(sector)

    It takes a list of values and runs the body
    once for each one. The variable (sector)
    automatically becomes each value in turn.

    range() creates a sequence of numbers:

        range(10)     → 0 through 9
        range(1, 11)  → 1 through 10

    len() tells you how many items are in a list:

        len(["a", "b", "c"])  → 3

    These tools let your programs work with
    collections of data — not just one item
    at a time.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2485 COMPLETE — Grid Restored ★\n"
            "New tools unlocked: for loops, lists, range(), len()\n"
        )
