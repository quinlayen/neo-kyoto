class ProcessTable:
    def __init__(self, processes):
        self.processes = {}
        for proc in processes:
            self.processes[proc["pid"]] = proc

    def ps(self, show_all=False):
        lines = []
        lines.append(f"{'PID':>5s}  {'USER':<12s}  {'CPU':>5s}  {'MEM':>5s}  {'STATUS':<10s}  {'COMMAND'}")
        for pid in sorted(self.processes):
            proc = self.processes[pid]
            if not show_all and proc.get("system", False):
                continue
            lines.append(
                f"{proc['pid']:>5d}  {proc['user']:<12s}  "
                f"{proc['cpu']:>4.1f}%  {proc['mem']:>4.1f}%  "
                f"{proc['status']:<10s}  {proc['command']}"
            )
        return "\n".join(lines)

    def kill(self, pid):
        if pid not in self.processes:
            return f"kill: ({pid}) - No such process"
        proc = self.processes[pid]
        if proc.get("protected", False):
            return f"kill: ({pid}) - Operation not permitted: {proc['command']} is a critical service"
        del self.processes[pid]
        return f"Process {pid} ({proc['command']}) terminated."

    def is_rogue_cleared(self):
        return not any(p.get("rogue", False) for p in self.processes.values())

    def get_rogue_count(self):
        return sum(1 for p in self.processes.values() if p.get("rogue", False))
