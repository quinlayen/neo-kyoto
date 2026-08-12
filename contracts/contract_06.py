from contracts.base_terminal import BaseTerminalContract
from systems.virtual_fs import VirtualFilesystem


class Contract06(BaseTerminalContract):
    CONTRACT_ID = "contract_06"
    TITLE = "Log Analysis"
    LOCATION = "Network Ops"

    def __init__(self):
        self.breaches_found = False
        self.backup_found = False
        self.backup_read = False
        self.report_created = False
        super().__init__()

    def _generate_access_log(self):
        lines = []
        for i in range(1, 201):
            ts = f"2189-08-10 {3 + i // 60:02d}:{i % 60:02d}:00"
            if i in (23, 67, 112, 158, 189):
                lines.append(f"{ts} [BREACH]  Unauthorized access from 10.0.{i}.{i % 255} — port {8000 + i}")
            elif i % 7 == 0:
                lines.append(f"{ts} [WARN]    Failed login attempt from 10.0.{i}.1")
            else:
                lines.append(f"{ts} [OK]      Request processed — user_{i % 50:03d}")
        return "\n".join(lines) + "\n"

    def _generate_connections_log(self):
        lines = []
        for i in range(1, 101):
            ts = f"2189-08-10 {3 + i // 60:02d}:{i % 60:02d}:00"
            if i in (15, 45, 78):
                lines.append(f"{ts} [UNAUTHORIZED]  Connection from unknown source 192.168.{i}.{i}")
            else:
                lines.append(f"{ts} [CONNECTED]     Node {i % 20:02d} — status OK")
        return "\n".join(lines) + "\n"

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

        fs.add_file("/home/contractor/Documents/contract_history.txt",
                     "Completed contracts:\n"
                     "  #2477 — Block 7 Power Node\n"
                     "  #2478 — Sector 12 Drone Routing\n"
                     "  #2479 — Sector 14 Drone Dispatch\n"
                     "  #2480 — Transit Hub Signals\n"
                     "  #2481 — Data Center Recovery\n")

        fs.add_file("/home/contractor/Desktop/grep_cheatsheet.txt",
                     "GREP QUICK REFERENCE\n"
                     "────────────────────\n"
                     "grep <word> <file>    search for word\n"
                     "grep ERROR log.txt    find ERROR lines\n"
                     "\n"
                     "PERMISSIONS\n"
                     "────────────────────\n"
                     "ls -l                 see permissions\n"
                     "chmod 644 <file>      owner read/write,\n"
                     "                      others read only\n"
                     "chmod 755 <dir>       full access for\n"
                     "                      owner, read for others\n")

        fs.add_file("/home/contractor/notes.txt",
                     "CONTRACTOR NOTES\n"
                     "────────────────\n"
                     "Security breach detected in the network.\n"
                     "Logs are in /var/log/ — access.log has\n"
                     "the breach records. Too many lines to\n"
                     "read manually. Use grep to search.\n"
                     "\n"
                     "There may be a backup config somewhere\n"
                     "with the original security key. Check\n"
                     "for hidden files.\n"
                     "\n"
                     "When done, create a findings report at\n"
                     "/tmp/report/findings.txt\n")

        fs.add_file("/var/log/access.log", self._generate_access_log())
        fs.add_file("/var/log/connections.log", self._generate_connections_log())

        fs.add_dir("/var/log/.backup")
        fs.add_file("/var/log/.backup/original.conf",
                     "# Network Security Configuration\n"
                     "# ──────────────────────────────\n"
                     "# ORIGINAL — before breach\n"
                     "\n"
                     "security_key:   NK-SEC-7744\n"
                     "firewall:       ENABLED\n"
                     "intrusion_det:  ACTIVE\n"
                     "last_audit:     2189-08-01\n",
                     "---------")

        fs.add_file("/etc/firewall.conf",
                     "# Firewall Configuration\n"
                     "# ─────────────────────\n"
                     "status: DISABLED\n"
                     "reason: manually overridden during breach\n"
                     "restore_key: <see original config backup>\n")

        fs.add_dir("/tmp")

        return fs

    def on_command(self, command_line):
        output = self.terminal.execute(command_line)

        parts = command_line.strip().split()
        if not parts:
            return output

        cmd = parts[0]

        if cmd == "grep" and len(parts) >= 3:
            pattern = parts[1]
            if pattern == "BREACH":
                for arg in parts[2:]:
                    resolved = self.fs.resolve_path(arg)
                    if resolved == "/var/log/access.log":
                        self.breaches_found = True

        if cmd == "ls":
            for arg in parts[1:]:
                if arg.startswith("-") and "a" in arg:
                    resolved = self.fs.resolve_path(parts[-1]) if len(parts) > 2 else self.fs.cwd
                    if resolved == "/var/log" or resolved.startswith("/var/log"):
                        if self.fs.exists("/var/log/.backup"):
                            self.backup_found = True

        if cmd == "cat":
            for arg in parts[1:]:
                resolved = self.fs.resolve_path(arg)
                if resolved == "/var/log/.backup/original.conf":
                    node = self.fs.get_node(resolved)
                    if node and self.fs._has_permission(node, "r"):
                        self.backup_read = True

        if cmd in ("touch", "mkdir"):
            if self.fs.exists("/tmp/report/findings.txt"):
                self.report_created = True

        self.update_completion()
        return output

    def reset_system(self):
        self.breaches_found = False
        self.backup_found = False
        self.backup_read = False
        self.report_created = False
        super().reset_system()

    def is_goal_met(self):
        return (self.breaches_found and self.backup_read and self.report_created)

    def get_status_text(self):
        def check(val):
            return "[OK]" if val else "[  ]"
        all_done = self.is_goal_met()
        indicator = "[OK]" if all_done else "[!!]"

        return f"""  NETWORK OPS — BREACH INVESTIGATION
  Overall:        {indicator}
  Breach search:  {check(self.breaches_found)} grep for BREACH entries
  Backup config:  {check(self.backup_read)} find and read original.conf
  Report filed:   {check(self.report_created)} create /tmp/report/findings.txt
"""

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2482 – Log Analysis             ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    A security breach hit the network. The access
    logs are massive — too many lines to read by
    hand. Some files may be hidden or locked down.

    ─── NEW COMMANDS ───

        grep <word> <file>  — search for a word
        ls -a               — show hidden files
        ls -la              — hidden + details
        chmod <mode> <file> — change permissions
        mkdir <dir>         — create a directory
        touch <file>        — create a file
        rm <file>           — delete a file
        rm -rf <dir>        — delete a directory

    ─── YOUR GOAL ───

    Complete the objectives shown in status.
    Check your notes at ~ to get started.

    Type  exit  to return to the contract board.
    Type  reset  to restore the filesystem.
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2482 COMPLETE  ★             ║
    ║   Network Ops — BREACH INVESTIGATED         ║
    ╚══════════════════════════════════════════════╝

    Breach entries identified. Original config
    recovered. Report filed. The security team
    has what they need.

    ─── WHAT YOU JUST DID ───

    You used grep to search hundreds of log lines
    in seconds. You found hidden files that a plain
    ls would miss. You changed file permissions to
    access locked data. And you created files and
    directories to organize your findings.

    These are the core tools of a system
    administrator. With pwd, ls, cd, cat, grep,
    chmod, mkdir, touch, and rm, you can navigate
    and manage any Linux system.

    ─── LOOKING AHEAD ───

    You have two skill sets now: Python scripting
    and terminal navigation. Each is powerful on
    its own. But imagine combining them.

    What if you could write a Python script that
    searches through files the way grep does? Or
    a script that processes a list of directories
    automatically?

    The next contracts will bring these worlds
    together.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2482 COMPLETE — Breach Investigated ★\n"
            "Terminal skills mastered. Combined contracts ahead.\n"
        )
