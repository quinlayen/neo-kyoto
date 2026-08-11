import time


class VirtualFilesystem:
    def __init__(self):
        self.root = self._make_dir("rwxr-xr-x")
        self.home = "/home/contractor"
        self.add_dir("/home/contractor")
        self.cwd = self.home

    def _make_dir(self, permissions="rwxr-xr-x"):
        return {
            "type": "dir",
            "permissions": permissions,
            "children": {},
            "mtime": time.time(),
        }

    def _make_file(self, content="", permissions="rw-r--r--"):
        return {
            "type": "file",
            "permissions": permissions,
            "content": content,
            "mtime": time.time(),
        }

    def resolve_path(self, path):
        if path.startswith("~"):
            path = self.home + path[1:]
        if not path.startswith("/"):
            if self.cwd == "/":
                path = "/" + path
            else:
                path = self.cwd + "/" + path

        parts = path.split("/")
        resolved = []
        for part in parts:
            if part == "" or part == ".":
                continue
            elif part == "..":
                if resolved:
                    resolved.pop()
            else:
                resolved.append(part)
        return "/" + "/".join(resolved)

    def _traverse(self, path):
        path = self.resolve_path(path)
        if path == "/":
            return self.root
        parts = path.strip("/").split("/")
        node = self.root
        for part in parts:
            if node["type"] != "dir":
                return None
            if part not in node["children"]:
                return None
            node = node["children"][part]
        return node

    def _traverse_parent(self, path):
        path = self.resolve_path(path)
        if path == "/":
            return None, None
        parts = path.strip("/").split("/")
        name = parts[-1]
        parent_path = "/" + "/".join(parts[:-1]) if len(parts) > 1 else "/"
        parent = self._traverse(parent_path)
        if parent is None or parent["type"] != "dir":
            return None, None
        return parent, name

    def add_dir(self, path, permissions="rwxr-xr-x"):
        path = self.resolve_path(path)
        parts = path.strip("/").split("/")
        node = self.root
        for part in parts:
            if part not in node["children"]:
                node["children"][part] = self._make_dir(permissions)
            node = node["children"][part]

    def add_file(self, path, content="", permissions="rw-r--r--"):
        path = self.resolve_path(path)
        parent, name = self._traverse_parent(path)
        if parent is None:
            parts = path.strip("/").split("/")
            self.add_dir("/" + "/".join(parts[:-1]))
            parent, name = self._traverse_parent(path)
        parent["children"][name] = self._make_file(content, permissions)

    def get_node(self, path):
        return self._traverse(path)

    def exists(self, path):
        return self._traverse(path) is not None

    def is_dir(self, path):
        node = self._traverse(path)
        return node is not None and node["type"] == "dir"

    def is_file(self, path):
        node = self._traverse(path)
        return node is not None and node["type"] == "file"

    def list_dir(self, path=None):
        if path is None:
            path = self.cwd
        node = self._traverse(path)
        if node is None:
            return None
        if node["type"] != "dir":
            return None
        return node["children"]

    def read_file(self, path):
        node = self._traverse(path)
        if node is None:
            return None
        if node["type"] != "file":
            return None
        return node["content"]

    def change_dir(self, path):
        resolved = self.resolve_path(path)
        node = self._traverse(resolved)
        if node is None:
            return f"cd: {path}: No such file or directory"
        if node["type"] != "dir":
            return f"cd: {path}: Not a directory"
        if not self._has_permission(node, "x"):
            return f"cd: {path}: Permission denied"
        self.cwd = resolved
        return None

    def make_dir(self, path):
        resolved = self.resolve_path(path)
        if self._traverse(resolved) is not None:
            return f"mkdir: cannot create directory '{path}': File exists"
        parent, name = self._traverse_parent(resolved)
        if parent is None:
            return f"mkdir: cannot create directory '{path}': No such file or directory"
        if not self._has_permission(parent, "w"):
            return f"mkdir: cannot create directory '{path}': Permission denied"
        parent["children"][name] = self._make_dir()
        return None

    def create_file(self, path):
        resolved = self.resolve_path(path)
        if self._traverse(resolved) is not None:
            node = self._traverse(resolved)
            node["mtime"] = time.time()
            return None
        parent, name = self._traverse_parent(resolved)
        if parent is None:
            return f"touch: cannot touch '{path}': No such file or directory"
        if not self._has_permission(parent, "w"):
            return f"touch: cannot touch '{path}': Permission denied"
        parent["children"][name] = self._make_file()
        return None

    def remove(self, path, recursive=False):
        resolved = self.resolve_path(path)
        if resolved == "/":
            return "rm: cannot remove '/': Permission denied"
        node = self._traverse(resolved)
        if node is None:
            return f"rm: cannot remove '{path}': No such file or directory"
        if node["type"] == "dir" and not recursive:
            return f"rm: cannot remove '{path}': Is a directory"
        parent, name = self._traverse_parent(resolved)
        if not self._has_permission(parent, "w"):
            return f"rm: cannot remove '{path}': Permission denied"
        del parent["children"][name]
        return None

    def set_permissions(self, path, mode):
        node = self._traverse(path)
        if node is None:
            return f"chmod: cannot access '{path}': No such file or directory"
        node["permissions"] = self._numeric_to_symbolic(mode)
        return None

    def _has_permission(self, node, perm_type):
        perms = node["permissions"]
        if perm_type == "r":
            return perms[0] == "r"
        elif perm_type == "w":
            return perms[1] == "w"
        elif perm_type == "x":
            return perms[2] == "x"
        return True

    def _numeric_to_symbolic(self, mode):
        if isinstance(mode, str):
            mode = int(mode, 8)
        result = ""
        for digit in [(mode >> 6) & 7, (mode >> 3) & 7, mode & 7]:
            result += "r" if digit & 4 else "-"
            result += "w" if digit & 2 else "-"
            result += "x" if digit & 1 else "-"
        return result

    def get_short_cwd(self):
        if self.cwd == self.home:
            return "~"
        if self.cwd.startswith(self.home + "/"):
            return "~" + self.cwd[len(self.home):]
        return self.cwd
