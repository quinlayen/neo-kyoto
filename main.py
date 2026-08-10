import os
import time
from contracts.contract_01 import Contract01
from interpreter import RestrictedInterpreter

def clear():
    os.system('cls' if os.name == 'nt' else 'clear')

def main():
    contract = Contract01()
    interpreter = RestrictedInterpreter(contract.node)
    script_path = "player_scripts/block7.py"

    print("=== Neo-Kyoto Systems Contractor ===")
    print("Prototype – Contract 1: Keep the Lights On\n")
    print(contract.get_briefing())
    input("\nPress Enter to continue...")

    while True:
        clear()
        print("=== Neo-Kyoto Systems Contractor ===")
        print(contract.node.get_status_text())

        if contract.completed:
            print(contract.get_completed_banner())

        print("Commands:  edit | run | status | quit")
        cmd = input("> ").strip().lower()

        if cmd == "quit":
            break

        elif cmd == "status":
            continue

        elif cmd == "edit":
            print(f"\nEdit the file: {script_path}")
            print("Save it, then come back and type 'run'.")
            input("Press Enter when ready...")

        elif cmd == "run":
            if not os.path.exists(script_path):
                print("No script found.")
                time.sleep(1)
                continue

            with open(script_path, "r") as f:
                code = f.read()

            print("\n--- Running script ---")
            result = interpreter.execute(code)
            print(result)
            print("---------------------\n")
            print(contract.node.get_status_text())

            # Goal met → full completion message exactly once, then pause
            # so clear() on the next loop cannot wipe it unread.
            if contract.consume_completion_announcement():
                interpreter.unlock_loops()
                print(contract.get_completion_message(), flush=True)

            input("\nPress Enter to continue...")

        else:
            print("Unknown command.")
            time.sleep(0.8)

if __name__ == "__main__":
    main()
