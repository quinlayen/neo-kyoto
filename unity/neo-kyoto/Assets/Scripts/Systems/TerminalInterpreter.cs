using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NeoKyoto.Systems
{
    /// <summary>
    /// The shell the player types into during terminal contracts. Commands are
    /// deliberately limited to the set each contract has taught.
    /// </summary>
    public class TerminalInterpreter
    {
        private readonly VirtualFilesystem _fs;
        private readonly Dictionary<string, Func<List<string>, string>> _commands;

        public VirtualFilesystem Fs { get { return _fs; } }

        /// <summary>Raised with the raw command line after each execution.</summary>
        public event Action<string> CommandExecuted;

        public TerminalInterpreter(VirtualFilesystem fs)
        {
            _fs = fs;
            _commands = new Dictionary<string, Func<List<string>, string>>
            {
                { "pwd", CmdPwd },
                { "ls", CmdLs },
                { "cd", CmdCd },
                { "cat", CmdCat },
                { "mkdir", CmdMkdir },
                { "touch", CmdTouch },
                { "rm", CmdRm },
                { "grep", CmdGrep },
                { "chmod", CmdChmod },
                { "head", CmdHead },
                { "tail", CmdTail },
                { "echo", CmdEcho },
                { "cp", CmdCp },
                { "mv", CmdMv },
            };
        }

        public string Execute(string commandLine)
        {
            commandLine = (commandLine ?? "").Trim();
            if (commandLine.Length == 0) return "";

            List<string> parts;
            try { parts = SplitArgs(commandLine); }
            catch (Exception e) { return "syntax error: " + e.Message; }
            if (parts.Count == 0) return "";

            string cmd = parts[0];
            var args = parts.GetRange(1, parts.Count - 1);

            Func<List<string>, string> fn;
            if (!_commands.TryGetValue(cmd, out fn)) return cmd + ": command not found";

            string output = fn(args);
            if (CommandExecuted != null) CommandExecuted(commandLine);
            return output;
        }

        /// <summary>Minimal shlex-style split honouring single and double quotes.</summary>
        public static List<string> SplitArgs(string line)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            char quote = '\0';
            bool has = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (quote != '\0')
                {
                    if (c == quote) quote = '\0';
                    else sb.Append(c);
                    continue;
                }
                if (c == '"' || c == '\'') { quote = c; has = true; continue; }
                if (c == ' ' || c == '\t')
                {
                    if (has || sb.Length > 0) { result.Add(sb.ToString()); sb.Length = 0; has = false; }
                    continue;
                }
                sb.Append(c);
            }
            if (quote != '\0') throw new Exception("No closing quotation");
            if (has || sb.Length > 0) result.Add(sb.ToString());
            return result;
        }

        private void ParseFlags(List<string> args, out HashSet<char> flags, out List<string> remaining)
        {
            flags = new HashSet<char>();
            remaining = new List<string>();
            foreach (var arg in args)
            {
                if (arg.Length > 1 && arg[0] == '-' && !char.IsDigit(arg[1]))
                {
                    for (int i = 1; i < arg.Length; i++) flags.Add(arg[i]);
                }
                else remaining.Add(arg);
            }
        }

        private string CmdPwd(List<string> args) { return _fs.Cwd; }

        private string CmdCd(List<string> args)
        {
            string path = args.Count == 0 ? "~" : args[0];
            return _fs.ChangeDir(path) ?? "";
        }

        private string CmdLs(List<string> args)
        {
            HashSet<char> flags; List<string> paths;
            ParseFlags(args, out flags, out paths);
            bool showAll = flags.Contains('a');
            bool longFormat = flags.Contains('l');

            if (paths.Count == 0) paths = new List<string> { _fs.Cwd };

            var outputParts = new List<string>();
            foreach (var path in paths)
            {
                var node = _fs.GetNode(path);
                if (node == null)
                {
                    outputParts.Add("ls: cannot access '" + path + "': No such file or directory");
                    continue;
                }
                if (!node.IsDir)
                {
                    string leaf = path.Split('/')[path.Split('/').Length - 1];
                    outputParts.Add(longFormat ? FormatLongEntry(leaf, node) : leaf);
                    continue;
                }

                var children = _fs.ListDir(path);
                if (children == null)
                {
                    outputParts.Add("ls: cannot open directory '" + path + "': Permission denied");
                    continue;
                }

                var names = new List<string>(children.Keys);
                names.Sort(StringComparer.Ordinal);
                if (!showAll) names = names.FindAll(n => !n.StartsWith("."));

                if (longFormat)
                {
                    var lines = new List<string>();
                    foreach (var name in names) lines.Add(FormatLongEntry(name, children[name]));
                    outputParts.Add(string.Join("\n", lines.ToArray()));
                }
                else
                {
                    outputParts.Add(string.Join("  ", names.ToArray()));
                }
            }
            return string.Join("\n", outputParts.ToArray());
        }

        private string FormatLongEntry(string name, FsNode node)
        {
            string typeChar = node.IsDir ? "d" : "-";
            int size = node.IsDir
                ? (node.Children != null ? node.Children.Count : 0)
                : (node.Content != null ? node.Content.Length : 0);
            string t = node.MTime.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture);
            return typeChar + node.Permissions + "  contractor contractor  " +
                   size.ToString().PadLeft(5) + "  " + t + "  " + name;
        }

        private string CmdCat(List<string> args)
        {
            if (args.Count == 0) return "cat: missing file operand";
            var outputParts = new List<string>();
            foreach (var path in args)
            {
                var node = _fs.GetNode(path);
                if (node == null) { outputParts.Add("cat: " + path + ": No such file or directory"); continue; }
                if (node.IsDir) { outputParts.Add("cat: " + path + ": Is a directory"); continue; }
                if (!_fs.HasPermission(node, 'r')) { outputParts.Add("cat: " + path + ": Permission denied"); continue; }
                outputParts.Add(node.Content);
            }
            return string.Join("\n", outputParts.ToArray());
        }

        private string CmdMkdir(List<string> args)
        {
            HashSet<char> flags; List<string> paths;
            ParseFlags(args, out flags, out paths);
            if (paths.Count == 0) return "mkdir: missing operand";
            var outputParts = new List<string>();
            foreach (var path in paths)
            {
                if (flags.Contains('p')) _fs.AddDir(path);
                else
                {
                    string err = _fs.MakeDirectory(path);
                    if (err != null) outputParts.Add(err);
                }
            }
            return string.Join("\n", outputParts.ToArray());
        }

        private string CmdTouch(List<string> args)
        {
            if (args.Count == 0) return "touch: missing file operand";
            var outputParts = new List<string>();
            foreach (var path in args)
            {
                string err = _fs.CreateFile(path);
                if (err != null) outputParts.Add(err);
            }
            return string.Join("\n", outputParts.ToArray());
        }

        private string CmdRm(List<string> args)
        {
            HashSet<char> flags; List<string> paths;
            ParseFlags(args, out flags, out paths);
            if (paths.Count == 0) return "rm: missing operand";
            bool recursive = flags.Contains('r');
            var outputParts = new List<string>();
            foreach (var path in paths)
            {
                string err = _fs.Remove(path, recursive);
                if (err != null) outputParts.Add(err);
            }
            return string.Join("\n", outputParts.ToArray());
        }

        private string CmdGrep(List<string> args)
        {
            if (args.Count < 2) return "grep: usage: grep PATTERN FILE";
            string pattern = args[0];
            var files = args.GetRange(1, args.Count - 1);
            bool showFilename = files.Count > 1;
            var outputParts = new List<string>();

            foreach (var path in files)
            {
                var node = _fs.GetNode(path);
                if (node == null) { outputParts.Add("grep: " + path + ": No such file or directory"); continue; }
                if (node.IsDir) { outputParts.Add("grep: " + path + ": Is a directory"); continue; }
                if (!_fs.HasPermission(node, 'r')) { outputParts.Add("grep: " + path + ": Permission denied"); continue; }

                foreach (var line in SplitLines(node.Content))
                {
                    if (line.Contains(pattern))
                        outputParts.Add(showFilename ? path + ":" + line : line);
                }
            }
            return string.Join("\n", outputParts.ToArray());
        }

        private string CmdChmod(List<string> args)
        {
            if (args.Count < 2) return "chmod: missing operand";
            string mode = args[0];
            bool allDigits = mode.Length == 3;
            foreach (char c in mode) if (!char.IsDigit(c)) allDigits = false;
            if (!allDigits) return "chmod: invalid mode: '" + mode + "'";

            var outputParts = new List<string>();
            for (int i = 1; i < args.Count; i++)
            {
                string err = _fs.SetPermissions(args[i], mode);
                if (err != null) outputParts.Add(err);
            }
            return string.Join("\n", outputParts.ToArray());
        }

        private string CmdHead(List<string> args) { return HeadTail(args, true, "head"); }
        private string CmdTail(List<string> args) { return HeadTail(args, false, "tail"); }

        private string HeadTail(List<string> args, bool fromStart, string name)
        {
            HashSet<char> flags; List<string> paths;
            ParseFlags(args, out flags, out paths);

            int n = 10;
            for (int i = 0; i < args.Count - 1; i++)
            {
                if (args[i] == "-n")
                {
                    int parsed;
                    if (int.TryParse(args[i + 1], out parsed))
                    {
                        n = parsed;
                        paths.Remove(args[i + 1]);
                    }
                    break;
                }
            }

            if (paths.Count == 0) return name + ": missing file operand";

            var outputParts = new List<string>();
            foreach (var path in paths)
            {
                var node = _fs.GetNode(path);
                if (node == null) { outputParts.Add(name + ": " + path + ": No such file or directory"); continue; }
                if (node.IsDir) { outputParts.Add(name + ": " + path + ": Is a directory"); continue; }
                if (!_fs.HasPermission(node, 'r')) { outputParts.Add(name + ": " + path + ": Permission denied"); continue; }

                var lines = SplitLines(node.Content);
                int count = Math.Min(n, lines.Count);
                var slice = fromStart
                    ? lines.GetRange(0, count)
                    : lines.GetRange(lines.Count - count, count);
                outputParts.Add(string.Join("\n", slice.ToArray()));
            }
            return string.Join("\n", outputParts.ToArray());
        }

        private string CmdCp(List<string> args)
        {
            if (args.Count < 2) return "cp: missing file operand";
            return _fs.CopyFile(args[0], args[1]) ?? "";
        }

        private string CmdMv(List<string> args)
        {
            if (args.Count < 2) return "mv: missing file operand";
            return _fs.Move(args[0], args[1]) ?? "";
        }

        private string CmdEcho(List<string> args) { return string.Join(" ", args.ToArray()); }

        private static List<string> SplitLines(string content)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(content)) return result;
            string normalised = content.Replace("\r\n", "\n").Replace("\r", "\n");
            var parts = normalised.Split('\n');
            int end = parts.Length;
            // A trailing newline does not create an extra empty line, matching splitlines().
            if (end > 0 && parts[end - 1].Length == 0) end--;
            for (int i = 0; i < end; i++) result.Add(parts[i]);
            return result;
        }

        public string GetPrompt()
        {
            return "contractor@neo-kyoto:" + _fs.GetShortCwd() + "$ ";
        }
    }
}
