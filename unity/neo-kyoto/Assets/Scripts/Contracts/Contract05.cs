using System.Collections.Generic;
using NeoKyoto.Systems;

namespace NeoKyoto.Contracts
{
    /// <summary>
    /// First terminal contract. The goal is reading one specific file, so the
    /// player has to navigate rather than run a script.
    /// </summary>
    public class Contract05 : TerminalContract
    {
        public const string TargetFile = "/opt/neo-kyoto/services/power-grid/error.log";

        public override string Id { get { return "contract_05"; } }
        public override string Title { get { return "System Recovery"; } }
        public override string Location { get { return "Data Center"; } }

        public bool TargetFound { get; private set; }

        public override VirtualFilesystem BuildFilesystem()
        {
            var fs = new VirtualFilesystem();

            fs.AddDir("/home/contractor/Desktop");
            fs.AddDir("/home/contractor/Documents");
            fs.AddDir("/home/contractor/Downloads");
            fs.AddDir("/home/contractor/Music");
            fs.AddDir("/home/contractor/Pictures");
            fs.AddDir("/home/contractor/Public");
            fs.AddDir("/home/contractor/Templates");
            fs.AddDir("/home/contractor/Videos");

            fs.AddFile("/home/contractor/.bashrc",
                "# contractor shell config\n" +
                "export PS1='contractor@neo-kyoto:\\w$ '\n" +
                "alias ll='ls -la'\n");

            fs.AddFile("/home/contractor/.bash_history",
                "ssh datacenter-01\n" +
                "cat /var/log/system.log\n" +
                "cd /opt/neo-kyoto/services\n" +
                "ls\n");

            fs.AddFile("/home/contractor/notes.txt",
                "CONTRACTOR NOTES\n" +
                "────────────────\n" +
                "Power grid service crashed at 03:47.\n" +
                "Diagnostic logs should be under\n" +
                "/opt/neo-kyoto/services/ somewhere.\n" +
                "Check the service directories.\n");

            fs.AddFile("/home/contractor/Documents/contract_history.txt",
                "Completed contracts:\n" +
                "  #2477 — Block 7 Power Node\n" +
                "  #2478 — Sector 12 Drone Routing\n" +
                "  #2479 — Sector 14 Drone Dispatch\n" +
                "  #2480 — Transit Hub Signals\n");

            fs.AddFile("/home/contractor/Desktop/terminal_cheatsheet.txt",
                "TERMINAL QUICK REFERENCE\n" +
                "────────────────────────\n" +
                "pwd          where am I?\n" +
                "ls           what's here?\n" +
                "cd <dir>     go into directory\n" +
                "cd ..        go up one level\n" +
                "cd ~         go home\n" +
                "cat <file>   read a file\n");

            fs.AddFile("/var/log/system.log",
                "2189-08-10 03:41:12 [INFO]  System health check passed\n" +
                "2189-08-10 03:42:05 [INFO]  Transit service: nominal\n" +
                "2189-08-10 03:43:18 [INFO]  Water recycler: nominal\n" +
                "2189-08-10 03:44:30 [INFO]  Power grid: load at 94%\n" +
                "2189-08-10 03:45:01 [WARN]  Power grid: load at 97%\n" +
                "2189-08-10 03:46:15 [WARN]  Power grid: load at 99%\n" +
                "2189-08-10 03:47:02 [CRIT]  Power grid: SERVICE CRASHED\n" +
                "2189-08-10 03:47:02 [CRIT]  See service logs for details\n" +
                "2189-08-10 03:47:05 [INFO]  Transit service: nominal\n" +
                "2189-08-10 03:48:00 [INFO]  Water recycler: nominal\n");

            fs.AddFile("/var/log/auth.log",
                "2189-08-10 03:40:00 [INFO]  contractor login accepted\n" +
                "2189-08-10 03:41:00 [INFO]  session opened\n");

            fs.AddDir("/var/log/old");
            fs.AddFile("/var/log/old/system.log.1",
                "2189-08-09 12:00:00 [INFO]  System health check passed\n" +
                "2189-08-09 18:00:00 [INFO]  All services nominal\n");

            fs.AddFile("/etc/services.conf",
                "# Neo-Kyoto Service Registry\n" +
                "# ─────────────────────────\n" +
                "power-grid    /opt/neo-kyoto/services/power-grid\n" +
                "transit       /opt/neo-kyoto/services/transit\n" +
                "water         /opt/neo-kyoto/services/water\n");

            fs.AddFile(TargetFile,
                "═══════════════════════════════════════\n" +
                "  POWER GRID — CRASH REPORT\n" +
                "═══════════════════════════════════════\n" +
                "\n" +
                "  Timestamp:  2189-08-10 03:47:02\n" +
                "  Error:      OVERLOAD_CASCADE\n" +
                "  Code:       NK-4021\n" +
                "  Sector:     Block 7, Sector 12, Sector 14\n" +
                "\n" +
                "  Load exceeded 99% threshold.\n" +
                "  Cascade failure across three sectors.\n" +
                "  Manual intervention required.\n" +
                "\n" +
                "  ◆ DIAGNOSTIC COMPLETE ◆\n" +
                "═══════════════════════════════════════\n");

            fs.AddFile("/opt/neo-kyoto/services/power-grid/config.yaml",
                "service: power-grid\n" +
                "max_load: 0.95\n" +
                "auto_restart: false\n" +
                "sectors: [block-7, sector-12, sector-14]\n");

            fs.AddFile("/opt/neo-kyoto/services/transit/status.log",
                "2189-08-10 03:48:00 [INFO]  All routes nominal\n" +
                "2189-08-10 03:48:00 [INFO]  8 drones active\n");

            fs.AddFile("/opt/neo-kyoto/services/water/status.log",
                "2189-08-10 03:48:00 [INFO]  Recycler output normal\n" +
                "2189-08-10 03:48:00 [INFO]  Pressure stable\n");

            return fs;
        }

        public override string OnCommand(string commandLine)
        {
            string output = Terminal.Execute(commandLine);

            // Completion is reading the crash report, so watch for a successful cat.
            var parts = TerminalInterpreter.SplitArgs((commandLine ?? "").Trim());
            if (parts.Count > 0 && parts[0] == "cat")
            {
                for (int i = 1; i < parts.Count; i++)
                {
                    if (Fs.ResolvePath(parts[i]) == TargetFile) TargetFound = true;
                }
            }

            UpdateCompletion();
            RaiseSystemChanged();
            return output;
        }

        public override void ResetSystem()
        {
            TargetFound = false;
            base.ResetSystem();
        }

        public override bool IsGoalMet() { return TargetFound; }

        public override string GetStatusText()
        {
            string status = TargetFound ? "FOUND" : "NOT FOUND";
            string indicator = TargetFound ? "[OK]" : "[!!]";
            return "  DATA CENTER — POWER GRID DIAGNOSTICS\n" +
                   "  Crash report:  " + indicator + " " + status + "\n";
        }

        public override string GetBriefing()
        {
            return @"    ╔══════════════════════════════════════════════╗
    ║   NEO-KYOTO SYSTEMS CONTRACTOR              ║
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
" + PageBreak + @"
    ─── TERMINAL COMMANDS ───

        pwd             — print your current
                          directory
        ls              — list files here
        cd <dir>        — move into a directory
        cd ..           — move up one directory
        cd ~            — go to your home directory
        cat <file>      — read a file's contents
" + PageBreak + @"
    ─── TIPS ───

    Start in your home directory. There may be
    notes or clues about where to look.

    Use ls to see what is in a directory. Use cd
    to move into it. Use cat to read files.

    The file tree has several directories. Not all
    of them are relevant — explore and figure out
    which path leads to the crash report.
" + PageBreak + @"
    ─── YOUR GOAL ───

    Find and read the power grid crash report.

    Use CONTRACT BOARD to leave this job.
    Type  reset  to restore the filesystem.
    ";
        }

        public override string GetCompletionMessage()
        {
            return @"    ╔══════════════════════════════════════════════╗
    ║   ◆  CONTRACT #2481 COMPLETE  ◆             ║
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
" + PageBreak + @"
    ─── THE LIMITATION ───

    You found one file by exploring manually. But
    what if you did not know which directory it
    was in? What if there were hundreds of log
    files and you needed to search them all for
    a specific word or error code?

    Reading every file by hand does not scale.
    You need a way to search through files.
" + PageBreak + @"
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
                run a file";
        }

        public override string GetCompletedBanner()
        {
            return "◆ CONTRACT #2481 COMPLETE — Crash Report Found ◆\n" +
                   "New tools unlocked: grep, ls -la, mkdir, touch, rm, chmod\n";
        }
    }
}
