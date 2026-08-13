---
name: feedback-briefing-style
description: "Briefing length was trimmed, then deliberately reverted — early contracts carry full teaching. Do not trim them again without asking"
metadata:
  type: feedback
---

**Current position (2026-08-13): early briefings are long on purpose. Do NOT trim them.**

The text was cut down in commit 2d4740d ("tighten all briefings"), then explicitly
restored on 2026-08-13 to the pre-trim versions from `2d4740d^`. C1 regained the
full explanation of what `()` means and how `print()` is used to watch a program
run; C2 regained the VARIABLES section on `=` and return values. Briefing text
roughly doubled on the early contracts.

**Why the reversal:** the trimmed versions left new players without enough setup —
both world context and concept explanation. The player needs the most scaffolding
in the opening jobs. Later contracts can taper.

**What changed my earlier advice:** I first read the shortness as the standing rule
and cut further; that was wrong. When Peter said briefings felt "way too concise",
verification showed the text already matched the pre-trim original byte-for-byte —
the real problem was **pagination making full text feel thin**, not missing words.
Both levers matter and they are separate: how much text there is, and how it is
chunked on screen.

**How to apply:**
- Do not shorten briefings or debriefs without asking. This has now flipped twice.
- Pages break at `─── SECTION ───` headers, splitting only if a section overflows
  the panel. If a screen looks sparse, pack pages fuller rather than cutting words.
- Still true: error messages should guide, filesystem files should reward
  exploration, and debriefs teach the *next* tool by first making the player feel
  the current tool's limitation.

Related: [[reference-tfwr-progression]] [[project-unity-demo-status]]
