import glob
import os
import sys
import time
from game_state import GameState
from interpreter import RestrictedInterpreter
from contracts.contract_01 import Contract01


def clear():
    os.system('cls' if os.name == 'nt' else 'clear')


def reset_player_scripts():
    for path in glob.glob("player_scripts/*.py"):
        with open(path, "r") as f:
            lines = f.readlines()
        header = []
        for line in lines:
            header.append(line)
            if line.startswith("# ────"):
                break
        header.append("\n")
        with open(path, "w") as f:
            f.writelines(header)


CONTRACT_DEFS = [
    {"id": "contract_01", "class": Contract01, "title": "Keep the Lights On", "location": "Block 7", "unlock_index": 0},
]


def _load_contracts():
    try:
        from contracts.contract_02 import Contract02
        CONTRACT_DEFS.append({"id": "contract_02", "class": Contract02, "title": "Drone Route Cleanup", "location": "Sector 12", "unlock_index": 1})  # conditionals
    except ImportError:
        pass
    try:
        from contracts.contract_03 import Contract03
        CONTRACT_DEFS.append({"id": "contract_03", "class": Contract03, "title": "Drone Dispatch", "location": "Sector 14", "unlock_index": -1})  # controlled while (no new gate)
    except ImportError:
        pass
    try:
        from contracts.contract_04 import Contract04
        CONTRACT_DEFS.append({"id": "contract_04", "class": Contract04, "title": "Signal Interference", "location": "Transit Hub", "unlock_index": -1})  # end of Python Phase 1
    except ImportError:
        pass
    try:
        from contracts.contract_05 import Contract05
        CONTRACT_DEFS.append({"id": "contract_05", "class": Contract05, "title": "System Recovery", "location": "Data Center", "unlock_index": -1, "type": "terminal"})
    except ImportError:
        pass
    try:
        from contracts.contract_06 import Contract06
        CONTRACT_DEFS.append({"id": "contract_06", "class": Contract06, "title": "Log Analysis", "location": "Network Ops", "unlock_index": -1, "type": "terminal"})
    except ImportError:
        pass
    try:
        from contracts.contract_07 import Contract07
        CONTRACT_DEFS.append({"id": "contract_07", "class": Contract07, "title": "Server Migration", "location": "Server Farm", "unlock_index": -1, "type": "terminal"})
    except ImportError:
        pass
    try:
        from contracts.contract_08 import Contract08
        CONTRACT_DEFS.append({"id": "contract_08", "class": Contract08, "title": "Grid Restoration", "location": "Central Grid", "unlock_index": 2, "type": "combined"})
    except ImportError:
        pass
    try:
        from contracts.contract_09 import Contract09
        CONTRACT_DEFS.append({"id": "contract_09", "class": Contract09, "title": "Process Lockdown", "location": "Comms Tower", "unlock_index": -1, "type": "terminal"})
    except ImportError:
        pass
    try:
        from contracts.contract_10 import Contract10
        CONTRACT_DEFS.append({"id": "contract_10", "class": Contract10, "title": "Water Treatment", "location": "Underground Plant", "unlock_index": 3, "type": "combined"})
    except ImportError:
        pass
    try:
        from contracts.contract_11 import Contract11
        CONTRACT_DEFS.append({"id": "contract_11", "class": Contract11, "title": "Sector Sweep", "location": "Industrial Zone", "unlock_index": -1, "type": "combined"})
    except ImportError:
        pass


def show_title_screen():
    clear()
    print("╔══════════════════════════════════════════════╗")
    print("║        NEO-KYOTO SYSTEMS CONTRACTOR         ║")
    print("║                                              ║")
    print("║    \"The city doesn't sleep. Neither do its   ║")
    print("║     systems. When they break, you get        ║")
    print("║     the call.\"                               ║")
    print("╚══════════════════════════════════════════════╝")
    print()
    input("  Press Enter to connect to the contractor terminal...")


def show_contract_board(game_state):
    while True:
        clear()
        print("╔══════════════════════════════════════════════╗")
        print("║   NEO-KYOTO — CONTRACTOR TERMINAL           ║")
        print("╚══════════════════════════════════════════════╝")
        print()
        print("  ─── AVAILABLE CONTRACTS ───")
        print()

        for i, cdef in enumerate(CONTRACT_DEFS):
            num = i + 1
            label = f"{cdef['title']} — {cdef['location']}"

            if game_state.is_contract_completed(cdef["id"]):
                status = "[DONE] ★"
            elif i == 0 or game_state.is_contract_completed(CONTRACT_DEFS[i - 1]["id"]):
                status = "[AVAILABLE]"
            else:
                status = "[LOCKED]"

            print(f"  [{num}]  {label:<40s} {status}")

        print()
        print("  ──────────────────────────────────────────────")
        print("  Type a number to accept a contract, or 'quit'.")
        print("  ──────────────────────────────────────────────")
        choice = input("  > ").strip().lower()

        if choice == "quit":
            return None

        if not choice.isdigit():
            print(f"\n  Unknown command: '{choice}'")
            time.sleep(1)
            continue

        idx = int(choice) - 1
        if idx < 0 or idx >= len(CONTRACT_DEFS):
            print("\n  Invalid contract number.")
            time.sleep(1)
            continue

        cdef = CONTRACT_DEFS[idx]

        if idx > 0 and not game_state.is_contract_completed(CONTRACT_DEFS[idx - 1]["id"]):
            print("\n  That contract is locked.")
            print("  Complete the previous contract first.")
            time.sleep(1.5)
            continue

        return cdef


def run_contract(cdef, game_state):
    contract = cdef["class"]()
    interpreter = RestrictedInterpreter(game_state, max_calls=contract.MAX_CALLS)
    script_path = contract.SCRIPT_FILE

    clear()
    print(contract.get_briefing())
    input("\n  Press Enter to begin...")

    while True:
        clear()
        print("══════════════════════════════════════════════")
        print(f"  NEO-KYOTO — {contract.TITLE} — {contract.LOCATION}")
        print("══════════════════════════════════════════════")
        print()
        print(contract.get_status_text())

        if contract.completed:
            print(contract.get_completed_banner())

        print("──────────────────────────────────────────────")
        print("  edit    — open your script for editing")
        print("  run     — execute your script")
        print("  status  — check system status")
        print("  brief   — re-read the contract briefing")
        if contract.completed:
            print("  debrief — review completion debrief")
        print("  back    — return to contract board")
        print("──────────────────────────────────────────────")
        cmd = input("  > ").strip().lower()

        if cmd == "back":
            break

        elif cmd == "status":
            continue

        elif cmd == "brief":
            clear()
            print(contract.get_briefing())
            input("\n  Press Enter to continue...")

        elif cmd == "debrief" and contract.completed:
            clear()
            print(contract.get_completion_message())
            input("\n  Press Enter to continue...")

        elif cmd == "edit":
            print(f"\n  Your script file is: {script_path}")
            print("  Open it in any text editor, write your code,")
            print("  and save the file.")
            input("\n  Press Enter when ready...")

        elif cmd == "run":
            if not os.path.exists(script_path):
                print(f"\n  No script file found at {script_path}")
                print("  Make sure the file exists and try again.")
                time.sleep(1.5)
                continue

            with open(script_path, "r") as f:
                code = f.read()

            contract.reset_system()
            interpreter.set_commands(
                active_commands=contract.get_commands(),
                retired_commands=game_state.retired_commands,
            )

            print()
            print("  ┌─ Running script ─────────────────────┐")
            result = interpreter.execute(code)
            print(f"    {result}")
            print("  └─────────────────────────────────────────┘")
            print()
            print(contract.get_status_text())

            if contract.consume_completion_announcement():
                game_state.mark_completed(cdef["id"], cdef["unlock_index"])
                game_state.retire_commands(contract.get_commands())
                print(contract.get_completion_message(), flush=True)

            input("\n  Press Enter to continue...")

        else:
            print(f"\n  Unknown command: '{cmd}'")
            print("  Available commands: edit, run, status, brief, back")
            time.sleep(1.2)


def run_terminal_contract(cdef, game_state):
    contract = cdef["class"]()

    clear()
    print(contract.get_briefing())
    input("\n  Press Enter to jack in...")

    while True:
        clear()
        print("══════════════════════════════════════════════")
        print(f"  NEO-KYOTO — {contract.TITLE} — {contract.LOCATION}")
        print("══════════════════════════════════════════════")
        print()
        print(contract.get_status_text())

        if contract.completed:
            print(contract.get_completed_banner())

        print("──────────────────────────────────────────────")
        print("  Type terminal commands below.")
        print("  Special:  brief | status | debrief | reset | exit")
        print("──────────────────────────────────────────────")

        while True:
            try:
                cmd = input(contract.get_prompt()).strip()
            except EOFError:
                return

            if not cmd:
                continue

            if cmd == "exit":
                return

            if cmd == "brief":
                clear()
                print(contract.get_briefing())
                input("\n  Press Enter to continue...")
                break

            if cmd == "status":
                break

            if cmd == "debrief" and contract.completed:
                clear()
                print(contract.get_completion_message())
                input("\n  Press Enter to continue...")
                break

            if cmd == "reset":
                contract.reset_system()
                print("  System reset. Filesystem restored to initial state.")
                break

            output = contract.on_command(cmd)
            if output:
                print(output)

            if contract.consume_completion_announcement():
                game_state.mark_completed(cdef["id"], cdef["unlock_index"])
                print()
                print(contract.get_completion_message(), flush=True)
                input("\n  Press Enter to continue...")
                break


def run_combined_contract(cdef, game_state):
    contract = cdef["class"]()
    interpreter = RestrictedInterpreter(game_state, max_calls=contract.MAX_CALLS)
    script_path = contract.SCRIPT_FILE

    clear()
    print(contract.get_briefing())
    input("\n  Press Enter to jack in...")

    while True:
        clear()
        print("══════════════════════════════════════════════")
        print(f"  NEO-KYOTO — {contract.TITLE} — {contract.LOCATION}")
        print("══════════════════════════════════════════════")
        print()
        print(contract.get_status_text())

        if contract.completed:
            print(contract.get_completed_banner())

        print("──────────────────────────────────────────────")
        print("  Terminal commands work here.")
        print("  edit — open script  |  run — execute script")
        print("  brief | status | debrief | reset | exit")
        print("──────────────────────────────────────────────")

        while True:
            try:
                cmd = input(contract.get_prompt()).strip()
            except EOFError:
                return

            if not cmd:
                continue

            if cmd == "exit":
                return

            if cmd == "brief":
                clear()
                print(contract.get_briefing())
                input("\n  Press Enter to continue...")
                break

            if cmd == "status":
                break

            if cmd == "debrief" and contract.completed:
                clear()
                print(contract.get_completion_message())
                input("\n  Press Enter to continue...")
                break

            if cmd == "reset":
                contract.reset_system()
                interpreter = RestrictedInterpreter(game_state, max_calls=contract.MAX_CALLS)
                print("  System reset.")
                break

            if cmd == "edit":
                print(f"\n  Your script file is: {script_path}")
                print("  Open it in any text editor, write your code,")
                print("  and save the file.")
                input("\n  Press Enter when ready...")
                continue

            if cmd == "run":
                if not os.path.exists(script_path):
                    print(f"\n  No script file found at {script_path}")
                    time.sleep(1)
                    continue

                with open(script_path, "r") as f:
                    code = f.read()

                contract.reset_game_system()
                interpreter.set_commands(
                    active_commands=contract.get_commands(),
                    retired_commands=game_state.retired_commands,
                )

                print()
                print("  ┌─ Running script ─────────────────────┐")
                result = interpreter.execute(code)
                print(f"    {result}")
                print("  └─────────────────────────────────────────┘")
                print()
                print(contract.get_status_text())

                if contract.is_goal_met():
                    contract.completed = True

                if contract.consume_completion_announcement():
                    game_state.mark_completed(cdef["id"], cdef["unlock_index"])
                    print(contract.get_completion_message(), flush=True)

                input("\n  Press Enter to continue...")
                break

            output = contract.on_command(cmd)
            if output:
                print(output)


def _dispatch_contract(cdef, game_state):
    if cdef.get("type") == "combined":
        run_combined_contract(cdef, game_state)
    elif cdef.get("type") == "terminal":
        run_terminal_contract(cdef, game_state)
    else:
        run_contract(cdef, game_state)


def main():
    _load_contracts()
    reset_player_scripts()
    game_state = GameState()
    dev_mode = "--dev" in sys.argv

    if dev_mode:
        game_state.unlock_all(CONTRACT_DEFS)
        print("  [DEV] All contracts and features unlocked.\n")

    if not dev_mode and not game_state.is_contract_completed("contract_01"):
        show_title_screen()
        run_contract(CONTRACT_DEFS[0], game_state)

    while True:
        cdef = show_contract_board(game_state)
        if cdef is None:
            print("\n  Disconnecting from contractor terminal...")
            time.sleep(0.6)
            print("  Session ended.\n")
            break
        _dispatch_contract(cdef, game_state)


if __name__ == "__main__":
    main()
