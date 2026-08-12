---
name: feedback-print-early
description: "Variables and print() should be available from the very start, not gated behind contract completion"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 95c6bdc1-46af-4303-ba42-a2e49afc13aa
---

Variables and print() should be introduced very early — ideally available from the beginning, not gated behind contract unlocks.

**Why:** Variables are fundamental to all technologies (Python, Linux, SQL, Git). Gating them behind contract 2 means the player can't use them in early contracts where they'd be naturally useful. print() is the most basic debugging tool and beginners need it from the start to understand what their programs are doing.

**How to apply:** Consider making variables and print() available from contract 1, or at minimum teach them explicitly very early. The current gate (variables unlock after contract 2) may be too late. When the multi-technology arc begins (Linux, SQL, Git), variables will be needed across all of them — they should feel like a core tool, not a late unlock.

**Design note:** User likes TFWR's pattern of "mechanics create the need" — the game should break the old approach to motivate learning the new tool, rather than just saying "here's a new feature." Apply this to variables: create a situation early where the player can SEE data but can't DO anything with it, making them want variables.
