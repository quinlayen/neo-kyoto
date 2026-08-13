from contracts.base_combined import BaseCombinedContract
from systems.virtual_fs import VirtualFilesystem
from systems.power_grid import PowerGrid


class Contract08(BaseCombinedContract):
    CONTRACT_ID = "contract_08"
    TITLE = "Grid Restoration"
    LOCATION = "Central Grid"
    SCRIPT_FILE = "player_scripts/grid.py"
    MAX_CALLS = 40

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
                     "Major power failure. The grid control\n"
                     "docs are somewhere under /opt/grid/ but\n"
                     "the directory got locked down after the\n"
                     "cascade failure.\n"
                     "\n"
                     "You will need to find the repair protocol\n"
                     "and the command reference. Not everything\n"
                     "is visible at first glance.\n")

        fs.add_dir("/opt/grid", "r-x------")
        fs.add_file("/opt/grid/.repair_protocol.txt",
                     "REPAIR PROTOCOL\n"
                     "───────────────\n"
                     "\n"
                     "get_broken_sectors() returns a list of\n"
                     "sector IDs that need repair. The list\n"
                     "changes each time the grid resets, so\n"
                     "you cannot write out each repair by hand.\n"
                     "\n"
                     "You need a way to say: for each sector\n"
                     "in this list, call repair on it.\n"
                     "\n"
                     "That is what a for loop does.\n"
                     "\n"
                     "─── FOR LOOPS ───\n"
                     "\n"
                     "A for loop repeats code once for each\n"
                     "item in a list:\n"
                     "\n"
                     "    for item in my_list:\n"
                     "        <do something with item>\n"
                     "\n"
                     "The variable before 'in' (here called\n"
                     "item) takes on each value in the list,\n"
                     "one at a time.\n"
                     "\n"
                     "You can combine this with a command that\n"
                     "returns a list:\n"
                     "\n"
                     "    broken = get_broken_sectors()\n"
                     "    for sector in broken:\n"
                     "        repair(sector)\n"
                     "\n"
                     "First line: get the list of broken IDs.\n"
                     "Second line: for each one in that list...\n"
                     "Third line: repair it.\n"
                     "\n"
                     "The indented line runs once per sector.\n"
                     "When the list is exhausted, the loop\n"
                     "stops and your program continues.\n")

        fs.add_file("/opt/grid/.commands.txt",
                     "GRID CONTROL COMMANDS\n"
                     "─────────────────────\n"
                     "scan_grid()            show all sectors\n"
                     "get_broken_sectors()   returns a list of\n"
                     "                       broken sector IDs\n"
                     "get_status(sector_id)  check one sector\n"
                     "repair(sector_id)      repair a sector\n",
                     "---------")

        fs.add_file("/var/log/grid.log",
                     "2189-08-11 02:30:00 [CRIT]  Cascade failure detected\n"
                     "2189-08-11 02:30:01 [CRIT]  Multiple sectors offline\n"
                     "2189-08-11 02:30:02 [WARN]  Sector failures randomized\n"
                     "2189-08-11 02:30:03 [WARN]  Manual hardcoding will not work\n"
                     "2189-08-11 02:31:00 [INFO]  Contractor dispatched\n")

        return fs

    def reset_game_system(self):
        self.grid = PowerGrid()

    def reset_system(self):
        self.reset_game_system()
        super().reset_system()

    def get_commands(self):
        return {
            "scan_grid": self.grid.scan_grid,
            "get_broken_sectors": self.grid.get_broken_sectors,
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

    Cascade failure. Most of the city's power
    grid is down — 15 sectors, and the failures
    change every time the grid resets. You cannot
    fix this by hand.

    The repair protocol and command reference
    are on the grid control server under /opt/grid.
    The directory got locked down after the crash.
    You will need your terminal skills to find
    and access the documentation.

    Once you understand the repair process, write
    a script to automate it.

    Terminal commands and scripting (edit/run) are
    both available.

    ─── YOUR GOAL ───

    Bring all 15 sectors online.

    Type  exit  to return to the contract board.
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2485 COMPLETE  ★             ║
    ║   Central Grid — ALL SECTORS ONLINE         ║
    ╚══════════════════════════════════════════════╝

    The grid is back. All 15 sectors restored.
    Neo-Kyoto has power again.

    ─── WHAT YOU JUST DID ───

    You combined two skill sets. You used the
    terminal to investigate — reading the repair
    protocol, understanding the system. Then you
    wrote a Python script with a for loop to
    automate the repairs.

    The key insight: get_broken_sectors() gave
    you a list, and the for loop processed every
    item in it. You did not need to know which
    sectors were broken in advance — your script
    handled whatever the system returned.

    ─── FOR LOOPS ───

    The for loop is one of the most powerful
    tools in programming:

        for item in collection:
            <do something with item>

    It works with any list or sequence. You will
    use it constantly from here on.

    range() creates a sequence of numbers:

        range(10)     → 0 through 9
        range(1, 16)  → 1 through 15

    len() tells you how many items are in a list:

        len(my_list)  → the count

    ─── WHAT COMES NEXT ───

    You just combined terminal investigation with
    Python automation for the first time. That
    pattern — investigate, then automate — is the
    core of systems engineering.

    More combined contracts are coming.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2485 COMPLETE — Grid Restored ★\n"
            "New tools unlocked: for loops, lists, range(), len()\n"
        )
