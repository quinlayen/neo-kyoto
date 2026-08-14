from contracts.base_terminal import BaseTerminalContract
from systems.virtual_fs import VirtualFilesystem
from systems.process_table import ProcessTable
from systems.terminal import TerminalInterpreter


class Contract09(BaseTerminalContract):
    CONTRACT_ID = "contract_09"
    TITLE = "Process Lockdown"
    LOCATION = "Comms Tower"

    def __init__(self):
        self.process_table = None
        super().__init__()

    def _build_processes(self):
        return ProcessTable([
            {"pid": 1, "user": "root", "cpu": 0.1, "mem": 0.3,
             "status": "running", "command": "systemd",
             "system": True, "protected": True},
            {"pid": 87, "user": "root", "cpu": 0.2, "mem": 0.8,
             "status": "running", "command": "neo-kyoto-core",
             "system": True, "protected": True},
            {"pid": 143, "user": "root", "cpu": 0.4, "mem": 1.2,
             "status": "running", "command": "network-monitor",
             "system": True, "protected": True},
            {"pid": 201, "user": "root", "cpu": 0.1, "mem": 0.5,
             "status": "running", "command": "sshd",
             "system": True, "protected": True},
            {"pid": 1204, "user": "contractor", "cpu": 0.3, "mem": 0.8,
             "status": "running", "command": "bash"},
            {"pid": 2847, "user": "unknown", "cpu": 47.3, "mem": 34.2,
             "status": "running", "command": "crypto_miner_x86",
             "rogue": True},
            {"pid": 3019, "user": "unknown", "cpu": 12.8, "mem": 8.5,
             "status": "running", "command": "data_exfiltrator",
             "rogue": True},
            {"pid": 3344, "user": "unknown", "cpu": 23.1, "mem": 15.7,
             "status": "running", "command": "port_scanner",
             "rogue": True},
            {"pid": 3501, "user": "www", "cpu": 1.2, "mem": 2.1,
             "status": "running", "command": "nginx",
             "system": True, "protected": True},
            {"pid": 4102, "user": "unknown", "cpu": 31.5, "mem": 22.3,
             "status": "running", "command": "backdoor_listener",
             "rogue": True},
        ])

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
                     "COMMS TOWER — INCIDENT REPORT\n"
                     "─────────────────────────────\n"
                     "The communications tower is running\n"
                     "slow. CPU usage is through the roof.\n"
                     "\n"
                     "Suspected rogue processes — someone\n"
                     "may have planted unauthorized software.\n"
                     "\n"
                     "Use  ps  to see running processes.\n"
                     "Use  ps aux  to see ALL processes,\n"
                     "including system-level ones.\n"
                     "\n"
                     "Look for processes with high CPU or\n"
                     "memory usage from unknown users.\n"
                     "\n"
                     "Use  kill <pid>  to terminate a\n"
                     "process by its ID number.\n"
                     "\n"
                     "WARNING: Do NOT kill system services.\n"
                     "They are protected, but still — be\n"
                     "careful what you terminate.\n")

        fs.add_file("/home/contractor/Desktop/process_cheatsheet.txt",
                     "PROCESS MANAGEMENT REFERENCE\n"
                     "────────────────────────────\n"
                     "ps           show your processes\n"
                     "ps aux       show ALL processes\n"
                     "kill <pid>   terminate a process\n"
                     "\n"
                     "WHAT TO LOOK FOR\n"
                     "────────────────────────────\n"
                     "High CPU%     something is working\n"
                     "              too hard\n"
                     "Unknown user  process wasn't started\n"
                     "              by anyone authorized\n"
                     "Suspicious    crypto miners, port\n"
                     "  names       scanners, backdoors\n")

        fs.add_file("/var/log/security.log",
                     "2189-08-11 01:14:22 [WARN]  Unusual CPU spike detected\n"
                     "2189-08-11 01:14:23 [WARN]  Unknown user processes found\n"
                     "2189-08-11 01:14:25 [ALERT] Possible cryptocurrency miner: PID 2847\n"
                     "2189-08-11 01:14:26 [ALERT] Data exfiltration attempt: PID 3019\n"
                     "2189-08-11 01:14:28 [ALERT] Port scanning activity: PID 3344\n"
                     "2189-08-11 01:14:30 [ALERT] Backdoor listener detected: PID 4102\n"
                     "2189-08-11 01:15:00 [INFO]  Contractor dispatched\n"
                     "2189-08-11 01:15:01 [TRACE] External origin: 203.0.113.42\n"
                     "2189-08-11 01:15:02 [TRACE] Access method: compromised SSH key\n"
                     "2189-08-11 01:15:03 [TRACE] Intrusion window: 00:47 — 01:12\n")

        fs.add_dir("/var/log/.incident")
        fs.add_file("/var/log/.incident/trace.log",
                     "─── INTRUSION TRACE ───\n"
                     "Origin IP:    203.0.113.42\n"
                     "Geolocation:  Neo-Kyoto District 9\n"
                     "Access point: Comms Tower SSH (port 22)\n"
                     "Credential:   stolen contractor key\n"
                     "\n"
                     "The attacker accessed the system using\n"
                     "a valid contractor SSH key. This was not\n"
                     "a brute force attack — someone had\n"
                     "legitimate credentials.\n"
                     "\n"
                     "Cross-reference with personnel database\n"
                     "recommended. [Requires database access.]\n",
                     "---------")

        return fs

    def reset_system(self):
        self.process_table = self._build_processes()
        super().reset_system()
        self.terminal = TerminalInterpreter(self.fs, process_table=self.process_table)

    def on_command(self, command_line):
        output = self.terminal.execute(command_line)

        parts = command_line.strip().split()
        if parts and parts[0] == "cat":
            for arg in parts[1:]:
                resolved = self.fs.resolve_path(arg)
                if resolved == "/var/log/.incident/trace.log":
                    node = self.fs.get_node(resolved)
                    if node and self.fs._has_permission(node, "r"):
                        self.bonus_found.add("trace_file")

        self.update_completion()
        return output

    def is_goal_met(self):
        return self.process_table.is_rogue_cleared()

    def get_status_text(self):
        rogue = self.process_table.get_rogue_count()
        indicator = "[OK]" if rogue == 0 else "[!!]"
        return f"""  COMMS TOWER — THREAT RESPONSE
  Status:          {indicator}
  Rogue processes: {rogue} remaining
"""

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ONCALL:// SYSTEMS CONTRACTOR              ║
    ║   Contract #2486 – Process Lockdown         ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    The communications tower is compromised.
    CPU usage is maxed out and the network is
    crawling. Someone planted rogue processes
    on this system.

    Find them and shut them down.

    ─── NEW COMMANDS ───

        ps            — show running processes
        ps aux        — show ALL processes
                        (including system)
        kill <pid>    — terminate a process

    Plus all your existing terminal commands.
    Check the security log in /var/log/ for
    additional intel.

    ─── YOUR GOAL ───

    Terminate all rogue processes.

    Type  exit  to return to the contract board.
    Type  reset  to restore the system.
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2486 COMPLETE  ★             ║
    ║   Comms Tower — ALL THREATS ELIMINATED       ║
    ╚══════════════════════════════════════════════╝

    All rogue processes terminated. CPU usage
    is back to normal. The communications tower
    is secure.

    ─── WHAT YOU JUST DID ───

    You identified unauthorized processes by
    reading the process table, distinguished
    legitimate services from rogue software, and
    terminated the threats by process ID. This is
    how real incident response works — inspect,
    identify, act.

    ─── SOMETHING TO NOTICE ───

    You found four rogue processes and killed
    them one at a time. That worked because there
    were four. But what if there were forty?

    And did you check the security logs? There
    is a trace file hidden in /var/log/ — someone
    accessed this system using a stolen contractor
    key. That is not a random attack. Someone had
    credentials.

    Keep that in mind.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2486 COMPLETE — Threats Eliminated ★\n"
            "New tools: ps, kill (process management)\n"
        )
