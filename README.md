# ONCALL: Systems Contractor

A cyberpunk programming-puzzle game. You are a freelance systems contractor in Neo-Kyoto, 2189.
Infrastructure breaks — power grids, drone networks, transit — and you get the call. You travel to
the district, plug a deck into the failing system, write real Python or terminal commands, and
watch the city respond.

**Platform**: PC native (Unity 6000.5.8f1, URP). WebGL is a best-effort share build, never a
design constraint.

## Layout

| Path | What |
|---|---|
| `unity/neo-kyoto/` | **The game.** Current implementation |
| `docs/` | Design docs — start at [`docs/README.md`](docs/README.md) |
| `docs/HANDOFF.md` | Moving machine or Claude account |
| `CLAUDE.md` | Orientation and gotchas |
| `contracts/`, `main.py` | ⚠ Historical pre-Unity Python prototype |

## Running the game

Open `unity/neo-kyoto` in Unity Hub with Editor **6000.5.8f1**.

A fresh clone needs the purchased asset kits re-imported before their scenes resolve — they are
gitignored for licence and LFS-quota reasons. See `docs/HANDOFF.md`, which also covers the
two-step Cyberpunk Megapolis import that catches everyone.

## Running the old Python prototype

Kept for reference only; the game moved to Unity.

```bash
python -m venv .venv
source .venv/bin/activate   # Windows: .venv\Scripts\activate
python main.py
```
