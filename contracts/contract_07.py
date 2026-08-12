from contracts.base_terminal import BaseTerminalContract
from systems.virtual_fs import VirtualFilesystem


class Contract07(BaseTerminalContract):
    CONTRACT_ID = "contract_07"
    TITLE = "Server Migration"
    LOCATION = "Server Farm"

    def __init__(self):
        self.objectives = {
            "db_config_moved": False,
            "hidden_key_found": False,
            "key_copied": False,
            "legacy_cleaned": False,
        }
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
                     "SERVER MIGRATION NOTES\n"
                     "──────────────────────\n"
                     "Old server structure is a mess. Files\n"
                     "need to be moved to the new standard:\n"
                     "\n"
                     "1. Move db config from /legacy/configs/\n"
                     "   to /srv/database/\n"
                     "\n"
                     "2. There's a hidden encryption key\n"
                     "   somewhere in /legacy/ — find it\n"
                     "   and copy it to /srv/security/\n"
                     "\n"
                     "3. The hidden key file is locked down.\n"
                     "   You'll need to fix its permissions\n"
                     "   before you can copy it.\n"
                     "\n"
                     "4. Clean up /legacy/ when done\n"
                     "   (remove it entirely)\n")

        fs.add_file("/home/contractor/Desktop/file_commands.txt",
                     "FILE OPERATIONS REFERENCE\n"
                     "────────────────────────\n"
                     "cp <src> <dst>    copy a file\n"
                     "mv <src> <dst>    move or rename\n"
                     "rm <file>         delete a file\n"
                     "rm -rf <dir>      delete a directory\n"
                     "                  and everything in it\n"
                     "\n"
                     "HIDDEN FILES\n"
                     "────────────────────────\n"
                     "ls -a             show hidden files\n"
                     "                  (names starting with .)\n"
                     "ls -la            hidden files + details\n"
                     "\n"
                     "PERMISSIONS\n"
                     "────────────────────────\n"
                     "chmod 644 <file>  read/write for owner\n"
                     "chmod 755 <dir>   full access for owner\n")

        fs.add_file("/legacy/configs/database.conf",
                     "# Database Configuration\n"
                     "host: 10.0.1.50\n"
                     "port: 5432\n"
                     "name: neo_kyoto_prod\n"
                     "pool_size: 20\n")

        fs.add_file("/legacy/configs/old_network.conf",
                     "# Deprecated network config\n"
                     "gateway: 10.0.0.1\n")

        fs.add_file("/legacy/logs/migration.log",
                     "2189-08-09 Migration started\n"
                     "2189-08-09 Moved web services to /srv\n"
                     "2189-08-09 Database config NOT migrated\n"
                     "2189-08-09 Security keys NOT migrated\n")

        fs.add_file("/legacy/.encryption_key",
                     "─── NEO-KYOTO ENCRYPTION KEY ───\n"
                     "Key:    NK-ENC-8192-ALPHA\n"
                     "Algo:   AES-256-GCM\n"
                     "Issued: 2189-07-15\n"
                     "────────────────────────────────\n",
                     "---------")

        fs.add_dir("/srv/database")
        fs.add_dir("/srv/security")
        fs.add_dir("/srv/web")

        fs.add_file("/srv/web/index.html",
                     "<html><body>Neo-Kyoto Portal</body></html>\n")

        return fs

    def on_command(self, command_line):
        output = self.terminal.execute(command_line)

        if self.fs.exists("/srv/database/database.conf"):
            self.objectives["db_config_moved"] = True

        found_key = False
        for path in ["/legacy/.encryption_key"]:
            node = self.fs.get_node(path)
            if node and self.fs._has_permission(node, "r"):
                found_key = True
        if found_key or self.fs.exists("/srv/security/.encryption_key") or self.fs.exists("/srv/security/encryption_key"):
            self.objectives["hidden_key_found"] = True

        if self.fs.exists("/srv/security/.encryption_key") or self.fs.exists("/srv/security/encryption_key"):
            self.objectives["key_copied"] = True

        if not self.fs.exists("/legacy"):
            self.objectives["legacy_cleaned"] = True

        self.update_completion()
        return output

    def reset_system(self):
        self.objectives = {
            "db_config_moved": False,
            "hidden_key_found": False,
            "key_copied": False,
            "legacy_cleaned": False,
        }
        super().reset_system()

    def is_goal_met(self):
        return all(self.objectives.values())

    def get_status_text(self):
        def check(key):
            return "[OK]" if self.objectives[key] else "[  ]"
        indicator = "[OK]" if self.is_goal_met() else "[!!]"

        return f"""  SERVER FARM — MIGRATION
  Overall:          {indicator}
  Database config:  {check("db_config_moved")} moved to /srv/database/
  Encryption key:   {check("key_copied")} copied to /srv/security/
  Legacy cleanup:   {check("legacy_cleaned")} /legacy/ removed
"""

    def get_briefing(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
    ║   Contract #2483 – Server Migration         ║
    ╚══════════════════════════════════════════════╝

    ─── INCOMING TRANSMISSION ───

    Contractor,

    The server farm is migrating to a new
    directory structure. Files were left behind
    in /legacy/ and need to be moved to /srv/.

    ─── NEW COMMANDS ───

        cp <src> <dst>   — copy a file
        mv <src> <dst>   — move or rename a file

    ─── YOUR GOAL ───

    Complete all objectives shown in status.
    Check ~/notes.txt for details.

    Type  exit  to return to the contract board.
    Type  reset  to restore the filesystem.
    """

    def get_completion_message(self):
        return """
    ╔══════════════════════════════════════════════╗
    ║   ★  CONTRACT #2483 COMPLETE  ★             ║
    ║   Server Farm — MIGRATION COMPLETE          ║
    ╚══════════════════════════════════════════════╝

    Files migrated, key secured, legacy cleaned up.
    The server farm is running on the new structure.

    ─── WHAT YOU JUST DID ───

    You moved and copied files between directories,
    discovered hidden files that plain ls wouldn't
    show, fixed permissions on locked files, and
    cleaned up an old directory tree. These are
    everyday operations for anyone managing servers.

    ─── TWO WORLDS ───

    You now have real terminal skills — navigating
    directories, reading files, managing permissions,
    moving data around. And you still have your
    Python scripting from before.

    So far these have been separate. But what if
    you needed both at once? Investigate a problem
    through the terminal, then write a script to
    fix it automatically?

    That is your next contract.
"""

    def get_completed_banner(self):
        return (
            "★ CONTRACT #2483 COMPLETE — Server Migration Done ★\n"
            "Combined contracts ahead: Python + terminal together.\n"
        )
