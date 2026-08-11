class TransitSignals:
    SIGNAL_DEFS = {
        1: "SCRAMBLED",
        2: "STUCK",
        3: "STUCK",
        4: "SCRAMBLED",
        5: "STUCK",
        6: "SCRAMBLED",
    }

    def __init__(self):
        self.signals = {}
        for n, state in self.SIGNAL_DEFS.items():
            self.signals[n] = state

    def _validate(self, signal_number):
        if signal_number not in self.signals:
            print(f"    Error: no signal {signal_number}. Valid signals are 1-6.")
            return False
        return True

    def check_signal(self, signal_number):
        if not self._validate(signal_number):
            return "UNKNOWN"
        state = self.signals[signal_number]
        print(f"    Signal {signal_number}: {state}")
        return state

    def reset_signal(self, signal_number):
        if not self._validate(signal_number):
            return
        state = self.signals[signal_number]
        if state == "STUCK":
            self.signals[signal_number] = "FIXED"
            print(f"    Signal {signal_number}: reset applied → FIXED")
        elif state == "FIXED":
            print(f"    Signal {signal_number}: already fixed.")
        else:
            print(f"    Signal {signal_number}: not stuck — reset won't help.")
            print(f"    Try calibrate_signal() for SCRAMBLED signals.")

    def calibrate_signal(self, signal_number):
        if not self._validate(signal_number):
            return
        state = self.signals[signal_number]
        if state == "SCRAMBLED":
            self.signals[signal_number] = "FIXED"
            print(f"    Signal {signal_number}: calibrated → FIXED")
        elif state == "FIXED":
            print(f"    Signal {signal_number}: already fixed.")
        else:
            print(f"    Signal {signal_number}: not scrambled — calibration won't help.")
            print(f"    Try reset_signal() for STUCK signals.")

    def is_goal_met(self):
        return all(s == "FIXED" for s in self.signals.values())

    def get_status_text(self):
        fixed = sum(1 for s in self.signals.values() if s == "FIXED")
        total = len(self.signals)
        indicator = "[OK]" if fixed == total else "[!!]"
        return f"""  TRANSIT HUB — SIGNAL CONTROL
  Status:     {indicator} {fixed}/{total} signals operational
"""
