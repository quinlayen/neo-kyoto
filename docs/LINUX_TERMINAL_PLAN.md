# Linux Terminal Contracts — Architecture + C5/C6

## Context

After 4 Python contracts, the player transitions to Linux terminal. Instead of writing scripts and running them, the player types commands at an interactive terminal prompt. The game provides a simulated filesystem that each contract populates with directories, files, and logs for the player to navigate and investigate.

## Architecture

### 1. `systems/virtual_fs.py` — VirtualFilesystem

An in-memory directory tree. Each contract creates its own layout.

**Core data structure:** Nested dict. Each node has type (dir/file), permissions, and either children (dir) or content (file).

**Key methods:**
- `add_file(path, content, permissions)` — plant a file in the tree
- `add_dir(path, permissions)` — create a directory
- `resolve_path(path)` — handle `~`, `..`, `.`, absolute/relative paths
- `get_node(path)` — returns the node dict at a path
- `list_dir(path)` — returns directory contents
- `read_file(path)` — returns file content string
- `remove(path, recursive)` — delete file/dir
- `make_dir(path)` — create new directory
- `set_permissions(path, mode)` — chmod equivalent
- `cwd` property, `home` = `/home/contractor`

Files have: name, content (string), permissions (e.g. "rw-r--r--"), owner, size (len of content), timestamp.
Directories have: name, children dict, permissions.
Hidden files: names starting with `.`

### 2. `systems/terminal.py` — TerminalInterpreter

Parses command strings and executes against a VirtualFilesystem.

**Commands to implement:**

| Command | Flags | Description |
|---------|-------|-------------|
| `pwd` | — | Print working directory |
| `ls` | `-l`, `-a`, `-la` | List directory contents |
| `cd` | — | Change directory (supports `~`, `..`, `.`) |
| `cat` | — | Display file contents |
| `mkdir` | `-p` | Create directory |
| `touch` | — | Create empty file |
| `rm` | `-r`, `-rf` | Remove file/directory |
| `grep` | — | Search file contents for pattern |
| `chmod` | — | Change file permissions (numeric: 755, 644) |
| `head` | `-n N` | Show first N lines of file |
| `tail` | `-n N` | Show last N lines of file |
| `echo` | — | Print text (useful later for redirection) |

**Parsing:** Use `shlex.split()` for proper quoting, then separate flags from arguments.

**Error handling:** Return helpful messages matching real terminal style:
- `ls: cannot access 'foo': No such file or directory`
- `cat: report.log: Permission denied`
- `rm: cannot remove 'logs/': Is a directory`

**Permission checking:** Before reads/writes/executes, check if permissions allow it. This enables teaching `chmod`.

### 3. `contracts/base_terminal.py` — BaseTerminalContract

Extends or parallels BaseContract for terminal-type contracts.

**Key differences from BaseContract:**
- `get_filesystem()` — returns a configured VirtualFilesystem (instead of get_commands)
- `get_available_commands()` — returns list of terminal commands the player can use
- No `SCRIPT_FILE`, no `MAX_CALLS`
- `reset_system()` rebuilds the filesystem

**Shared with BaseContract:** briefing, completion_message, completed_banner, is_goal_met, get_status_text, consume_completion_announcement

### 4. `main.py` — Terminal contract runner

Add `run_terminal_contract(cdef, game_state)` — different interaction loop:

```
1. Show briefing
2. Loop:
   - Show prompt: contractor@neo-kyoto:~$ 
   - Read player input
   - Execute command via TerminalInterpreter
   - Print output
   - Check is_goal_met() after each command
   - If goal met, show completion
   - Special inputs: "brief", "status", "exit" (back to board)
3. Prompt shows current directory (like real terminal)
```

**Contract type detection:** Add `"type": "terminal"` to CONTRACT_DEFS. The main loop checks type and calls the appropriate runner.

## Contract Designs

### C5: "System Recovery"
**Location:** Server Room / Data Center
**Teaches:** pwd, ls, cd, cat, ~ (navigation + reading files)

**Filesystem layout:**
```
/home/contractor/
    notes.txt          — "Check the power grid service logs"
/var/log/
    system.log         — general log, large, mentions power grid
    auth.log           — irrelevant
    old/
        system.log.1   — old logs
/etc/
    services.conf      — lists service paths
/opt/neo-kyoto/
    services/
        power-grid/
            error.log  — TARGET: contains the error code
            config.yaml
        transit/
            status.log
        water/
            status.log
```

**Goal:** The player must `cat` the error.log file in the power-grid directory. The contract tracks which files the player has read and checks if they found the target file.

**Briefing (hybrid):** "A power grid service crashed. The diagnostic logs are somewhere in the system's file tree. Navigate the directories, find the right log, and read it."

**Completion:** Teaches ls -la (hidden files, permissions), mkdir, touch, rm. "You navigated the file tree by hand. But you only needed a few files. What if you could search through ALL of them at once?"

### C6: "Log Analysis"
**Location:** Network Operations Center
**Teaches:** ls -la, grep, hidden files, mkdir, touch, rm, chmod, head/tail

**Filesystem layout:**
```
/home/contractor/
/var/log/
    access.log         — 200+ lines, several with "BREACH" 
    connections.log    — 100+ lines, some with "UNAUTHORIZED"
    .backup/           — hidden directory
        original.conf  — needed but permission-denied (r--------)
/etc/
    firewall.conf      — needs editing (readable but relevant)
/tmp/
    report/            — player creates report here
```

**Goal:** Multi-step:
1. Use grep to find "BREACH" entries in access.log
2. Find the hidden .backup directory (ls -a)
3. Fix permissions on original.conf (chmod)
4. Read original.conf to get the security key
5. Create a report file (touch /tmp/report/findings.txt or similar)

**Briefing:** "A security breach was detected. The logs are massive — you need to search them, not read them line by line. Some diagnostic files may be hidden or locked down."

**Completion:** Teases Python + terminal combined. "You investigated by hand. But what if you could write a script that does this investigation automatically — navigating directories, searching files, processing what it finds?" Sets up for loops + lists returning in Python Phase 2.

## Build Order

1. `systems/virtual_fs.py` — filesystem first, test independently
2. `systems/terminal.py` — terminal interpreter, test with filesystem
3. `contracts/base_terminal.py` — terminal contract base
4. `main.py` — terminal contract runner + type detection
5. `contracts/contract_05.py` — first terminal contract
6. Test C5 end-to-end
7. `contracts/contract_06.py` — second terminal contract
8. Test C6 end-to-end

## Future Considerations

- Pipes (`|`) and redirection (`>`, `>>`) for later contracts
- `wc` for counting (combine with grep)
- `find` for searching directory trees
- Combined Python + terminal contracts where the player uses terminal to investigate, then writes a Python script to fix
- The visual game will render the terminal as a "jacked in" interface overlaying the god-view
