import os
import time
from game_state import GameState
from interpreter import RestrictedInterpreter
from contracts.contract_01 import Contract01


def clear():
    os.system('cls' if os.name == 'nt' else 'clear')


CONTRACT_DEFS = [
    {"id": "contract_01", "class": Contract01, "title": "Keep the Lights On", "location": "Block 7", "unlock_index": 0},
]


def _load_contracts():
    try:
        from contracts.contract_02 import Contract02
        CONTRACT_DEFS.append({"id": "contract_02", "class": Contract02, "title": "Drone Route Cleanup", "location": "Sector 12", "unlock_index": 1})
    except ImportError:
        pass
    try:
        from contracts.contract_03 import Contract03
        CONTRACT_DEFS.append({"id": "contract_03", "class": Contract03, "title": "Inventory Drift", "location": "Harbor District", "unlock_index": 2})
    except ImportError:
        pass
    try:
        from contracts.contract_04 import Contract04
        CONTRACT_DEFS.append({"id": "contract_04", "class": Contract04, "title": "Elevator Recovery", "location": "Midtown", "unlock_index": 3})
    except ImportError:
        pass
    try:
        from contracts.contract_05 import Contract05
        CONTRACT_DEFS.append({"id": "contract_05", "class": Contract05, "title": "Assembly Automation", "location": "Industrial Zone", "unlock_index": 4})
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
    interpreter = RestrictedInterpreter(game_state)
    interpreter.set_commands(
        active_commands=contract.get_commands(),
        retired_commands=game_state.retired_commands,
    )
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


def main():
    _load_contracts()
    show_title_screen()
    game_state = GameState()

    while True:
        cdef = show_contract_board(game_state)
        if cdef is None:
            print("\n  Disconnecting from contractor terminal...")
            time.sleep(0.6)
            print("  Session ended.\n")
            break
        run_contract(cdef, game_state)


if __name__ == "__main__":
    main()
