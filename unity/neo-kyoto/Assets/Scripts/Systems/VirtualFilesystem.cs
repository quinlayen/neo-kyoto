using System;
using System.Collections.Generic;

namespace NeoKyoto.Systems
{
    public class FsNode
    {
        public bool IsDir;
        public string Permissions;
        public string Content;
        public DateTime MTime;
        public Dictionary<string, FsNode> Children;

        public FsNode Clone()
        {
            var copy = new FsNode
            {
                IsDir = IsDir,
                Permissions = Permissions,
                Content = Content,
                MTime = MTime
            };
            if (Children != null)
            {
                copy.Children = new Dictionary<string, FsNode>();
                foreach (var kv in Children) copy.Children[kv.Key] = kv.Value.Clone();
            }
            return copy;
        }
    }

    /// <summary>
    /// In-memory filesystem backing the terminal contracts. Paths are POSIX-style
    /// and permissions are enforced, so chmod and hidden files work as obstacles.
    /// </summary>
    public class VirtualFilesystem
    {
        public FsNode Root;
        public string Home = "/home/contractor";
        public string Cwd;

        public VirtualFilesystem()
        {
            Root = MakeDir("rwxr-xr-x");
            AddDir(Home);
            Cwd = Home;
        }

        private FsNode MakeDir(string permissions)
        {
            return new FsNode
            {
                IsDir = true,
                Permissions = permissions,
                Children = new Dictionary<string, FsNode>(),
                MTime = DateTime.Now
            };
        }

        private FsNode MakeFile(string content, string permissions)
        {
            return new FsNode
            {
                IsDir = false,
                Permissions = permissions,
                Content = content,
                MTime = DateTime.Now
            };
        }

        public string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) path = ".";
            if (path.StartsWith("~")) path = Home + path.Substring(1);
            if (!path.StartsWith("/")) path = (Cwd == "/" ? "/" : Cwd + "/") + path;

            var resolved = new List<string>();
            foreach (var part in path.Split('/'))
            {
                if (part.Length == 0 || part == ".") continue;
                if (part == "..") { if (resolved.Count > 0) resolved.RemoveAt(resolved.Count - 1); }
                else resolved.Add(part);
            }
            return "/" + string.Join("/", resolved.ToArray());
        }

        private FsNode Traverse(string path)
        {
            path = ResolvePath(path);
            if (path == "/") return Root;
            var node = Root;
            foreach (var part in path.Trim('/').Split('/'))
            {
                if (!node.IsDir) return null;
                FsNode child;
                if (!node.Children.TryGetValue(part, out child)) return null;
                node = child;
            }
            return node;
        }

        private void TraverseParent(string path, out FsNode parent, out string name)
        {
            parent = null;
            name = null;
            path = ResolvePath(path);
            if (path == "/") return;

            var parts = path.Trim('/').Split('/');
            name = parts[parts.Length - 1];
            string parentPath = parts.Length > 1
                ? "/" + string.Join("/", parts, 0, parts.Length - 1)
                : "/";
            var p = Traverse(parentPath);
            if (p == null || !p.IsDir) { parent = null; name = null; return; }
            parent = p;
        }

        public void AddDir(string path, string permissions = "rwxr-xr-x")
        {
            path = ResolvePath(path);
            var node = Root;
            foreach (var part in path.Trim('/').Split('/'))
            {
                if (part.Length == 0) continue;
                if (!node.Children.ContainsKey(part)) node.Children[part] = MakeDir(permissions);
                node = node.Children[part];
            }
        }

        public void AddFile(string path, string content = "", string permissions = "rw-r--r--")
        {
            path = ResolvePath(path);
            FsNode parent; string name;
            TraverseParent(path, out parent, out name);
            if (parent == null)
            {
                var parts = path.Trim('/').Split('/');
                AddDir("/" + string.Join("/", parts, 0, parts.Length - 1));
                TraverseParent(path, out parent, out name);
            }
            if (parent == null) return;
            parent.Children[name] = MakeFile(content, permissions);
        }

        public FsNode GetNode(string path) { return Traverse(path); }
        public bool Exists(string path) { return Traverse(path) != null; }

        public bool IsDir(string path)
        {
            var n = Traverse(path);
            return n != null && n.IsDir;
        }

        public bool IsFile(string path)
        {
            var n = Traverse(path);
            return n != null && !n.IsDir;
        }

        public Dictionary<string, FsNode> ListDir(string path = null)
        {
            var n = Traverse(path ?? Cwd);
            if (n == null || !n.IsDir) return null;
            if (!HasPermission(n, 'r')) return null;
            return n.Children;
        }

        public string ReadFile(string path)
        {
            var n = Traverse(path);
            if (n == null || n.IsDir) return null;
            return n.Content;
        }

        public string ChangeDir(string path)
        {
            string resolved = ResolvePath(path);
            var node = Traverse(resolved);
            if (node == null) return "cd: " + path + ": No such file or directory";
            if (!node.IsDir) return "cd: " + path + ": Not a directory";
            if (!HasPermission(node, 'x')) return "cd: " + path + ": Permission denied";
            Cwd = resolved;
            return null;
        }

        public string MakeDirectory(string path)
        {
            string resolved = ResolvePath(path);
            if (Traverse(resolved) != null)
                return "mkdir: cannot create directory '" + path + "': File exists";
            FsNode parent; string name;
            TraverseParent(resolved, out parent, out name);
            if (parent == null)
                return "mkdir: cannot create directory '" + path + "': No such file or directory";
            if (!HasPermission(parent, 'w'))
                return "mkdir: cannot create directory '" + path + "': Permission denied";
            parent.Children[name] = MakeDir("rwxr-xr-x");
            return null;
        }

        public string CreateFile(string path)
        {
            string resolved = ResolvePath(path);
            var existing = Traverse(resolved);
            if (existing != null) { existing.MTime = DateTime.Now; return null; }
            FsNode parent; string name;
            TraverseParent(resolved, out parent, out name);
            if (parent == null) return "touch: cannot touch '" + path + "': No such file or directory";
            if (!HasPermission(parent, 'w')) return "touch: cannot touch '" + path + "': Permission denied";
            parent.Children[name] = MakeFile("", "rw-r--r--");
            return null;
        }

        public string Remove(string path, bool recursive)
        {
            string resolved = ResolvePath(path);
            if (resolved == "/") return "rm: cannot remove '/': Permission denied";
            var node = Traverse(resolved);
            if (node == null) return "rm: cannot remove '" + path + "': No such file or directory";
            if (node.IsDir && !recursive) return "rm: cannot remove '" + path + "': Is a directory";
            FsNode parent; string name;
            TraverseParent(resolved, out parent, out name);
            if (parent == null) return "rm: cannot remove '" + path + "': No such file or directory";
            if (!HasPermission(parent, 'w')) return "rm: cannot remove '" + path + "': Permission denied";
            parent.Children.Remove(name);
            return null;
        }

        public string CopyFile(string src, string dst)
        {
            string srcResolved = ResolvePath(src);
            var srcNode = Traverse(srcResolved);
            if (srcNode == null) return "cp: cannot stat '" + src + "': No such file or directory";
            if (srcNode.IsDir) return "cp: -r not specified; omitting directory '" + src + "'";
            if (!HasPermission(srcNode, 'r')) return "cp: cannot open '" + src + "': Permission denied";

            string dstResolved = ResolvePath(dst);
            var dstNode = Traverse(dstResolved);
            if (dstNode != null && dstNode.IsDir)
            {
                var parts = srcResolved.Split('/');
                dstResolved = dstResolved.TrimEnd('/') + "/" + parts[parts.Length - 1];
            }

            FsNode parent; string name;
            TraverseParent(dstResolved, out parent, out name);
            if (parent == null) return "cp: cannot create '" + dst + "': No such file or directory";
            if (!HasPermission(parent, 'w')) return "cp: cannot create '" + dst + "': Permission denied";

            var copy = srcNode.Clone();
            copy.MTime = DateTime.Now;
            parent.Children[name] = copy;
            return null;
        }

        public string Move(string src, string dst)
        {
            string srcResolved = ResolvePath(src);
            var srcNode = Traverse(srcResolved);
            if (srcNode == null) return "mv: cannot stat '" + src + "': No such file or directory";

            FsNode srcParent; string srcName;
            TraverseParent(srcResolved, out srcParent, out srcName);
            if (srcParent == null) return "mv: cannot move '" + src + "': No such file or directory";
            if (!HasPermission(srcParent, 'w')) return "mv: cannot move '" + src + "': Permission denied";

            string dstResolved = ResolvePath(dst);
            var dstNode = Traverse(dstResolved);
            if (dstNode != null && dstNode.IsDir)
                dstResolved = dstResolved.TrimEnd('/') + "/" + srcName;

            FsNode dstParent; string dstName;
            TraverseParent(dstResolved, out dstParent, out dstName);
            if (dstParent == null) return "mv: cannot move '" + src + "' to '" + dst + "': No such file or directory";
            if (!HasPermission(dstParent, 'w')) return "mv: cannot move to '" + dst + "': Permission denied";

            dstParent.Children[dstName] = srcNode;
            srcParent.Children.Remove(srcName);
            return null;
        }

        public string SetPermissions(string path, string mode)
        {
            var node = Traverse(path);
            if (node == null) return "chmod: cannot access '" + path + "': No such file or directory";
            node.Permissions = NumericToSymbolic(mode);
            return null;
        }

        public bool HasPermission(FsNode node, char permType)
        {
            string p = node.Permissions;
            if (string.IsNullOrEmpty(p) || p.Length < 3) return true;
            if (permType == 'r') return p[0] == 'r';
            if (permType == 'w') return p[1] == 'w';
            if (permType == 'x') return p[2] == 'x';
            return true;
        }

        private string NumericToSymbolic(string mode)
        {
            int value = Convert.ToInt32(mode, 8);
            int[] digits = { (value >> 6) & 7, (value >> 3) & 7, value & 7 };
            string result = "";
            foreach (int d in digits)
            {
                result += (d & 4) != 0 ? "r" : "-";
                result += (d & 2) != 0 ? "w" : "-";
                result += (d & 1) != 0 ? "x" : "-";
            }
            return result;
        }

        public string GetShortCwd()
        {
            if (Cwd == Home) return "~";
            if (Cwd.StartsWith(Home + "/")) return "~" + Cwd.Substring(Home.Length);
            return Cwd;
        }
    }
}
