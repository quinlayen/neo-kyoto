from contracts.base_terminal import BaseTerminalContract
from systems.virtual_fs import VirtualFilesystem


class Contract05(BaseTerminalContract):
    CONTRACT_ID = "contract_05"
    TITLE = "System Recovery"
    LOCATION = "Data Center"

    def __init__(self):
        self.target_found = False
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
                     "export PS1='contractor@neo-kyoto:\\w$ '\n"
                     "alias ll='ls -la'\n")

        fs.add_file("/home/contractor/.bash_history",
                     "ssh datacenter-01\n"
                     "cat /var/log/system.log\n"
                     "cd /opt/neo-kyoto/services\n"
                     "ls\n")

        fs.add_file("/home/contractor/notes.txt",
                     "CONTRACTOR NOTES\n"
                     "────────────────\n"
                     "Power grid service crashed at 03:47.\n"
                     "Diagnostic logs should be under\n"
                     "/opt/neo-kyoto/services/ somewhere.\n"
                     "Check the service directories.\n")

        fs.add_file("/home/contractor/Documents/contract_history.txt",
                     "Completed contracts:\n"
                     "  #2477 — Block 7 Power Node\n"
                     "  #2478 — Sector 12 Drone Routing\n"
                     "  #2479 — Sector 14 Drone Dispatch\n"
                     "  #2480 — Transit Hub Signals\n")

        fs.add_file("/home/contractor/Desktop/terminal_cheatsheet.txt",
                     "TERMINAL QUICK REFERENCE\n"
                     "────────────────────────\n"
                     "pwd          where am I?\n"
                     "ls           what's here?\n"
                     "cd <dir>     go into directory\n"
                     "cd ..        go up one level\n"
                     "cd ~         go home\n"
                     "cat <file>   read a file\n")

        fs.add_file("/var/log/system.log",
                     "2189-08-10 03:41:12 [INFO]  System health check passed\n"
                     "2189-08-10 03:42:05 [INFO]  Transit service: nominal\n"
                     "2189-08-10 03:43:18 [INFO]  Water recycler: nominal\n"
                     "2189-08-10 03:44:30 [INFO]  Power grid: load at 94%\n"
                     "2189-08-10 03:45:01 [WARN]  Power grid: load at 97%\n"
                     "2189-08-10 03:46:15 [WARN]  Power grid: load at 99%\n"
                     "2189-08-10 03:47:02 [CRIT]  Power grid: SERVICE CRASHED\n"
                     "2189-08-10 03:47:02 [CRIT]  See service logs for details\n"
                     "2189-08-10 03:47:05 [INFO]  Transit service: nominal\n"
                     "2189-08-10 03:48:00 [INFO]  Water recycler: nominal\n")

        fs.add_file("/var/log/auth.log",
                     "2189-08-10 03:40:00 [INFO]  contractor login accepted\n"
                     "2189-08-10 03:41:00 [INFO]  session opened\n")

        fs.add_dir("/var/log/old")
        fs.add_file("/var/log/old/system.log.1",
                     "2189-08-09 12:00:00 [INFO]  System health check passed\n"
                     "2189-08-09 18:00:00 [INFO]  All services nominal\n")

        fs.add_file("/etc/services.conf",
                     "# Neo-Kyoto Service Registry\n"
                     "# ─────────────────────────\n"
                     "power-grid    /opt/neo-kyoto/services/power-grid\n"
                     "transit       /opt/neo-kyoto/services/transit\n"
                     "water         /opt/neo-kyoto/services/water\n")

        fs.add_file("/opt/neo-kyoto/services/power-grid/error.log",
                     "═══════════════════════════════════════\n"
                     "  POWER GRID — CRASH REPORT\n"
                     "═══════════════════════════════════════\n"
                     "\n"
                     "  Timestamp:  2189-08-10 03:47:02\n"
                     "  Error:      OVERLOAD_CASCADE\n"
                     "  Code:       NK-4021\n"
                     "  Sector:     Block 7, Sector 12, Sector 14\n"
                     "\n"
                     "  Load exceeded 99% threshold.\n"
                     "  Cascade failure across three sectors.\n"
                     "  Manual intervention required.\n"
                     "\n"
                     "  ★ DIAGNOSTIC COMPLETE ★\n"
                     "═══════════════════════════════════════\n")

        fs.add_file("/opt/neo-kyoto/services/power-grid/config.yaml",
                     "service: power-grid\n"
                     "max_load: 0.95\n"
                     "auto_restart: false\n"
                     "sectors: [block-7, sector-12, sector-14]\n")

        fs.add_file("/opt/neo-kyoto/services/transit/status.log",
                     "2189-08-10 03:48:00 [INFO]  All routes nominal\n"
                     "2189-08-10 03:48:00 [INFO]  8 drones active\n")

        fs.add_file("/opt/neo-kyoto/services/water/status.log",
                     "2189-08-10 03:48:00 [INFO]  Recycler output normal\n"
                     "2189-08-10 03:48:00 [INFO]  Pressure stable\n")

        return fs

    def on_command(self, command_line):
        output = self.terminal.execute(command_line)

        parts = command_line.strip().split()
        if parts and parts[0] == "cat":
            for arg in parts[1:]:
                resolved = self.fs.resolve_path(arg)
                if resolved == "/opt/neo-kyoto/services/power-grid/error.log":
                    self.target_found = True
                if resolved == "/home/contractor/.bash_history":
                    self.bonus_found.add("bash_history")

        self.update_completion()
        return output

    def reset_system(self):
        self.target_found = False
        super().reset_system()

    def is_goal_met(self):
        return self.target_found

    def get_status_text(self):
        status = "FOUND" if self.target_found else "NOT FOUND"
        indicator = "[OK]" if self.target_found else "[!!]"
        return f"""  DATA CENTER — POWER GRID DIAGNOSTICS
  Crash report:  {indicator} {status}
"""

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ONCALL:// SYSTEMS CONTRACTOR              ║
    ║   Contract #2481 – System Recovery          ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    Something different this time. The power grid
    service crashed at 03:47 and we need to know
    why before we can fix it.

    The crash report is somewhere in the system's
    file tree, but we do not know exactly where.
    You need to jack into the data center terminal
    and find it.

    This is not a scripting job. You will be typing
    commands directly into a terminal — navigating
    directories, listing files, and reading what
    you find.

    ─── TERMINAL COMMANDS ───

        pwd             — print your current
                          directory
        ls              — list files here
        cd <dir>        — move into a directory
        cd ..           — move up one directory
        cd ~            — go to your home directory
        cat <file>      — read a file's contents

    ─── YOUR GOAL ───

    Find and read the power grid crash report.
    Start from your home directory — there may
    be clues about where to look.

    Type  exit  to return to the contract board.
    Type  reset  to restore the filesystem.
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2481 COMPLETE  ★             ║
    ║   Data Center — CRASH REPORT FOUND          ║
    ╚══════════════════════════════════════════════╝

    You found the crash report. Error NK-4021 —
    an overload cascade across three sectors. Now
    the repair team knows what to fix.

    ─── WHAT YOU JUST DID ───

    You navigated a file system using a terminal.
    You moved between directories, listed their
    contents, and read files to find what you
    needed. These are the same tools that real
    system administrators use every day.

    ─── THE LIMITATION ───

    You found one file by exploring manually. But
    what if you did not know which directory it
    was in? What if there were hundreds of log
    files and you needed to search them all for
    a specific word or error code?

    Reading every file by hand does not scale.
    You need a way to search through files.

    ─── NEW TOOLS ───

    ls -a     — show hidden files (names that
                start with a dot)
    ls -l     — show detailed info: permissions,
                size, and date
    ls -la    — both at once
    grep      — search inside files for a word
                or pattern
    head      — show the first few lines of a file
    tail      — show the last few lines
    mkdir     — create a new directory
    touch     — create an empty file
    rm        — delete a file
    rm -rf    — delete a directory and everything
                inside it
    chmod     — change who can read, write, or
                run a file
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2481 COMPLETE — Crash Report Found ★\n"
            "New tools unlocked: grep, ls -la, mkdir, touch, rm, chmod\n"
        )
