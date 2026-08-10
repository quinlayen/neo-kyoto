class AssemblyLine:
    STAGES = ["harvest", "process", "package", "ship"]

    def __init__(self):
        self.current_stage = 0
        self.cycles_completed = 0
        self.target_cycles = 10

    def _expect_stage(self, stage_name, stage_index):
        if self.current_stage != stage_index:
            expected = self.STAGES[self.current_stage]
            print(f"    Pipeline error: must {expected} before {stage_name}.")
            return False
        return True

    def _advance(self, stage_name, stage_index):
        if not self._expect_stage(stage_name, stage_index):
            return

        if stage_index == len(self.STAGES) - 1:
            self.cycles_completed += 1
            self.current_stage = 0
            print(f"    {stage_name} complete → cycle {self.cycles_completed}/{self.target_cycles} done")
        else:
            self.current_stage = stage_index + 1
            print(f"    {stage_name} complete → ready for {self.STAGES[self.current_stage]}")

    def harvest(self):
        self._advance("harvest", 0)

    def process(self):
        self._advance("process", 1)

    def package(self):
        self._advance("package", 2)

    def ship(self):
        self._advance("ship", 3)

    def check_pipeline(self):
        next_step = self.STAGES[self.current_stage]
        remaining = self.target_cycles - self.cycles_completed
        print(f"    Cycles completed: {self.cycles_completed}/{self.target_cycles}")
        print(f"    Remaining:        {remaining}")
        print(f"    Next step:        {next_step}")

    def is_goal_met(self):
        return self.cycles_completed >= self.target_cycles

    def get_status_text(self):
        indicator = "[OK]" if self.is_goal_met() else "[!!]"
        next_step = self.STAGES[self.current_stage]

        return f"""  INDUSTRIAL ZONE — ASSEMBLY CELL 9
  Status:     {indicator} {self.cycles_completed}/{self.target_cycles} cycles completed
  Next step:  {next_step}
"""
