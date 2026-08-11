import shlex
import time


class TerminalInterpreter:
    def __init__(self, filesystem):
        self.fs = filesystem
        self.commands = {
            "pwd": self.cmd_pwd,
            "ls": self.cmd_ls,
            "cd": self.cmd_cd,
            "cat": self.cmd_cat,
            "mkdir": self.cmd_mkdir,
            "touch": self.cmd_touch,
            "rm": self.cmd_rm,
            "grep": self.cmd_grep,
            "chmod": self.cmd_chmod,
            "head": self.cmd_head,
            "tail": self.cmd_tail,
            "echo": self.cmd_echo,
            "clear": self.cmd_clear,
        }

    def execute(self, command_line):
        command_line = command_line.strip()
        if not command_line:
            return ""
        try:
            parts = shlex.split(command_line)
        except ValueError as e:
            return f"syntax error: {e}"

        cmd = parts[0]
        args = parts[1:]

        if cmd not in self.commands:
            return f"{cmd}: command not found"

        return self.commands[cmd](args)

    def _parse_flags(self, args):
        flags = set()
        remaining = []
        for arg in args:
            if arg.startswith("-") and len(arg) > 1 and not arg[1].isdigit():
                for ch in arg[1:]:
                    flags.add(ch)
            else:
                remaining.append(arg)
        return flags, remaining

    def cmd_pwd(self, args):
        return self.fs.cwd

    def cmd_cd(self, args):
        if not args:
            path = "~"
        else:
            path = args[0]
        err = self.fs.change_dir(path)
        return err or ""

    def cmd_ls(self, args):
        flags, paths = self._parse_flags(args)
        show_all = "a" in flags
        long_format = "l" in flags

        if not paths:
            paths = [self.fs.cwd]

        output_parts = []
        for path in paths:
            node = self.fs.get_node(path)
            if node is None:
                output_parts.append(f"ls: cannot access '{path}': No such file or directory")
                continue
            if node["type"] == "file":
                if long_format:
                    name = path.split("/")[-1]
                    output_parts.append(self._format_long_entry(name, node))
                else:
                    output_parts.append(path.split("/")[-1])
                continue

            children = self.fs.list_dir(path)
            if children is None:
                output_parts.append(f"ls: cannot open directory '{path}': Permission denied")
                continue

            names = sorted(children.keys())
            if not show_all:
                names = [n for n in names if not n.startswith(".")]

            if long_format:
                lines = []
                for name in names:
                    child = children[name]
                    lines.append(self._format_long_entry(name, child))
                output_parts.append("\n".join(lines))
            else:
                output_parts.append("  ".join(names))

        return "\n".join(output_parts)

    def _format_long_entry(self, name, node):
        perms = node["permissions"]
        type_char = "d" if node["type"] == "dir" else "-"
        if node["type"] == "file":
            size = len(node["content"])
        else:
            size = len(node.get("children", {}))
        t = time.strftime("%b %d %H:%M", time.localtime(node["mtime"]))
        return f"{type_char}{perms}  contractor contractor  {size:>5d}  {t}  {name}"

    def cmd_cat(self, args):
        if not args:
            return "cat: missing file operand"
        output_parts = []
        for path in args:
            node = self.fs.get_node(path)
            if node is None:
                output_parts.append(f"cat: {path}: No such file or directory")
                continue
            if node["type"] == "dir":
                output_parts.append(f"cat: {path}: Is a directory")
                continue
            if not self.fs._has_permission(node, "r"):
                output_parts.append(f"cat: {path}: Permission denied")
                continue
            output_parts.append(node["content"])
        return "\n".join(output_parts)

    def cmd_mkdir(self, args):
        flags, paths = self._parse_flags(args)
        if not paths:
            return "mkdir: missing operand"
        output_parts = []
        for path in paths:
            if "p" in flags:
                self.fs.add_dir(path)
            else:
                err = self.fs.make_dir(path)
                if err:
                    output_parts.append(err)
        return "\n".join(output_parts)

    def cmd_touch(self, args):
        if not args:
            return "touch: missing file operand"
        output_parts = []
        for path in args:
            err = self.fs.create_file(path)
            if err:
                output_parts.append(err)
        return "\n".join(output_parts)

    def cmd_rm(self, args):
        flags, paths = self._parse_flags(args)
        if not paths:
            return "rm: missing operand"
        recursive = "r" in flags
        output_parts = []
        for path in paths:
            err = self.fs.remove(path, recursive=recursive)
            if err:
                output_parts.append(err)
        return "\n".join(output_parts)

    def cmd_grep(self, args):
        if len(args) < 2:
            return "grep: usage: grep PATTERN FILE"
        pattern = args[0]
        files = args[1:]
        output_parts = []
        show_filename = len(files) > 1
        for path in files:
            node = self.fs.get_node(path)
            if node is None:
                output_parts.append(f"grep: {path}: No such file or directory")
                continue
            if node["type"] == "dir":
                output_parts.append(f"grep: {path}: Is a directory")
                continue
            if not self.fs._has_permission(node, "r"):
                output_parts.append(f"grep: {path}: Permission denied")
                continue
            for line in node["content"].splitlines():
                if pattern in line:
                    if show_filename:
                        output_parts.append(f"{path}:{line}")
                    else:
                        output_parts.append(line)
        return "\n".join(output_parts)

    def cmd_chmod(self, args):
        if len(args) < 2:
            return "chmod: missing operand"
        mode = args[0]
        paths = args[1:]
        if not mode.isdigit() or len(mode) != 3:
            return f"chmod: invalid mode: '{mode}'"
        output_parts = []
        for path in paths:
            err = self.fs.set_permissions(path, mode)
            if err:
                output_parts.append(err)
        return "\n".join(output_parts)

    def cmd_head(self, args):
        flags, paths = self._parse_flags(args)
        if not paths:
            return "head: missing file operand"
        n = 10
        for i, arg in enumerate(args):
            if arg == "-n" and i + 1 < len(args) and args[i + 1].isdigit():
                n = int(args[i + 1])
                paths = [p for p in paths if p != args[i + 1]]
                break
        output_parts = []
        for path in paths:
            node = self.fs.get_node(path)
            if node is None:
                output_parts.append(f"head: {path}: No such file or directory")
                continue
            if node["type"] == "dir":
                output_parts.append(f"head: {path}: Is a directory")
                continue
            if not self.fs._has_permission(node, "r"):
                output_parts.append(f"head: {path}: Permission denied")
                continue
            lines = node["content"].splitlines()
            output_parts.append("\n".join(lines[:n]))
        return "\n".join(output_parts)

    def cmd_tail(self, args):
        flags, paths = self._parse_flags(args)
        if not paths:
            return "tail: missing file operand"
        n = 10
        for i, arg in enumerate(args):
            if arg == "-n" and i + 1 < len(args) and args[i + 1].isdigit():
                n = int(args[i + 1])
                paths = [p for p in paths if p != args[i + 1]]
                break
        output_parts = []
        for path in paths:
            node = self.fs.get_node(path)
            if node is None:
                output_parts.append(f"tail: {path}: No such file or directory")
                continue
            if node["type"] == "dir":
                output_parts.append(f"tail: {path}: Is a directory")
                continue
            if not self.fs._has_permission(node, "r"):
                output_parts.append(f"tail: {path}: Permission denied")
                continue
            lines = node["content"].splitlines()
            output_parts.append("\n".join(lines[-n:]))
        return "\n".join(output_parts)

    def cmd_echo(self, args):
        return " ".join(args)

    def cmd_clear(self, args):
        return "\033[2J\033[H"

    def get_prompt(self):
        short_cwd = self.fs.get_short_cwd()
        return f"contractor@neo-kyoto:{short_cwd}$ "
