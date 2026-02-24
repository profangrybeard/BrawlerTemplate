# Session 4: Combat Frame Data + Attack State Machines

**Date:** Thursday, Feb 26
**Duration:** 2.5 hours
**Prerequisites:** Working match flow from Session 3. UI wired from homework (HealthBarUI minimum). Students are still using ExampleFighter with 2 example attacks.

**Session Goal:** Students understand attacks as data (not code), create 3+ attacks per fighter with intentional frame data, and begin making their two fighters feel distinct. They also understand the attack lifecycle as a state machine — the first concrete example of action gating.

---

## Lecture 1 (20 min): Attacks Are Data, Not Code

### Opening Hook

"If I asked you to make a jab faster, what file do you open? If the answer is a C# script, you're doing it wrong."

### Key Concept: ScriptableObject-Driven Attacks

Every attack in this template is an `AttackData` ScriptableObject. No attack logic lives in your fighter script. This means:

- Designers and artists can tune attacks in the Inspector without touching code
- Creating a new attack = right-click, Create, fill in numbers
- A/B testing = swap one asset for another in the AttackController slot
- Balance changes = adjust numbers, no recompile

"Code defines HOW attacks work. Data defines WHAT each attack does."

### Frame Data: The Language of Fighting Games

Every attack has three phases. This is universal — Street Fighter, Smash Bros, Tekken, your game.

**Startup** — frames before the hitbox appears.
- This is your commitment. You pressed the button, now you wait.
- Fast startup (2-4 frames) = the attack comes out almost instantly
- Slow startup (10-15 frames) = the opponent sees it coming

**Active** — frames the hitbox can connect.
- More active frames = more forgiving, easier to land
- Fewer active frames = precise timing required
- This is the window where the attack can actually hit

**Recovery** — frames after the hitbox disappears, before you can act again.
- This is your vulnerability. If you missed, you're stuck.
- Short recovery (6-10) = safe on whiff, hard to punish
- Long recovery (20-30) = if you miss, opponent gets a free hit

**Total commitment** = startup + active + recovery = how many frames you can't do anything else.

### Frame Timing at 60fps

The game runs at 60 frames per second. Each frame is ~17 milliseconds.

| Frames | Time | Feel |
|--------|------|------|
| 3 | 50ms | Blink-fast, reactive |
| 6 | 100ms | Quick, intentional |
| 10 | 167ms | Readable, committal |
| 15 | 250ms | Slow, telegraph |
| 30 | 500ms | Very slow, high risk |

A fast jab at 3+3+8 = 14 total frames = about a quarter second from button press to acting again.
A power smash at 15+4+28 = 47 total frames = over three quarters of a second locked out. That's an eternity in a fight.

### Risk vs Reward

This is game design through numbers:

- **Fast jab:** 14 frames total. Low damage (5-8). Safe — short recovery means even if you miss, you recover quickly. Low knockback — won't kill, but it's hard to punish.
- **Power smash:** 47 frames total. High damage (18-25). Risky — long startup means opponent can see it coming, long recovery means missing gets you punished. High knockback — this is your kill move.

"The jab is a question: 'are you blocking?' The power smash is an answer: 'I read that you weren't.'"

### Reference the Sakurai Video

Masahiro Sakurai (creator of Smash Bros) has a video specifically about frame data. Recommend students watch it outside of class. It's short and directly relevant.

### The Pipeline

Trace how a button press becomes a hit:

```
Player presses Attack
  -> PlayerInputHandler buffers the input
  -> AttackController reads direction held (neutral/forward/up/down)
  -> AttackController checks if grounded or aerial
  -> Selects the matching AttackData from its slots
  -> Enters Startup state (frames counted)
  -> Hitbox activates (Active state)
  -> If Hitbox overlaps a Hurtbox: damage, knockback, hitstop
  -> Hitbox deactivates (Recovery state)
  -> Back to Idle — player can act again
```

### Transition: This IS a State Machine

"Notice that word — 'state.' The attack lifecycle is a state machine:"

```
Idle -> Startup -> Active -> Recovery -> Idle
```

You can't cancel out of startup. You can't move during recovery. You can't start a new attack until the current one finishes. `AttackController.IsAttacking` is true during all three phases.

This is **action gating** — the game deciding when you can and can't act. In Session 3 you wired GameManager, which gates actions at the match level (can't fight during countdown). Now you're seeing gating at the attack level (can't act during recovery).

"Every time you press attack, you're entering a contract: 'I will do nothing else for N frames.' A jab is a short contract. A power smash is a long one. Choosing which contract to sign is the game."

---

## Work Block 1 (45 min): Create Fighter 1's Attacks

### Student Task

Create AttackData ScriptableObjects for their first fighter. Minimum 3 attacks:
- **Neutral** (no direction held) — usually a jab or quick strike
- **Forward** (left/right held) — usually a stronger, reaching attack
- **One more** — up attack (launcher), down attack (spike), or aerial

Place them in `Fighter1/Configs/Attacks/`. Assign to AttackController slots. Test immediately.

### Guidance

"Use the attack archetypes in Guide 05 as starting points. Don't invent numbers from scratch — start with the template values and adjust based on how it feels."

"Create, test, adjust. Don't design on paper. Design in play mode. If the jab feels too slow, reduce startup frames. If the forward attack never connects, increase hitbox size or active frames. Iterate."

### Circulate and Watch For

| What You'll See | What to Say |
|-----------------|-------------|
| Attacks that never hit | "Check your hitbox offset — is it in front of the fighter or behind them? Offset X should be positive (hitbox is offset in the facing direction)." |
| Attacks that hit from across the stage | "Hitbox size is too large. Pull it in. Look at the red gizmo in Scene view during an attack." |
| All attacks feel identical | "Compare your numbers. If the jab and the power hit both have 6 startup and 12 recovery, they'll feel the same. Make the jab much faster and the heavy much slower." |
| Student designing on paper without testing | "Stop writing. Create one AttackData, assign it, press play, press attack. How does it feel? Adjust from there." |
| Recovery is 0 or very short on everything | "If recovery is short on every attack, there's no risk to attacking. The opponent can never punish a miss. That's not a game — that's button mashing." |
| "How do I see the hitbox?" | "Look in the Scene view, not the Game view. Hitboxes draw gizmos — red when active, gray when inactive. You can also enable Log Attacks on AttackController for console output." |

### Check-In (around 30-40 min)

Ask each team: **"Show me your jab vs your heavy attack. Which is faster? Which hits harder? Which is riskier?"**

If they can answer clearly, they get it. If both attacks feel the same, push them to increase the contrast.

---

## Break (15 min)

---

## Lecture 2 (20 min): Fighter Identity Through Frame Data

### Opening Question

Ask 2-3 teams: "What are your two fighters? How should they feel different?"

Listen for vague answers ("one is fast, one is strong") — that's fine as a starting point. Push for specifics: "Fast how? Startup frames? Movement speed? Recovery?"

### Archetypes: How Data Creates Identity

Show how the same system produces fundamentally different fighters, purely through data:

**Rushdown** — "Gets in your face and stays there"
- Fast startup (2-4 frames), short recovery (6-10)
- Low per-hit damage (5-8), low knockback
- Strength: pressure, combos, overwhelming opponent
- Weakness: can't kill easily — needs many hits, or a risky read
- Every attack is a small commitment

**Heavy Hitter** — "One good read wins the round"
- Slow startup (10-15 frames), long recovery (20-30)
- High per-hit damage (18-25), high knockback
- Strength: punishes mistakes, kills early
- Weakness: whiff anything and you're wide open
- Every attack is a big commitment

**Zoner / Spacing** — "Controls the space between fighters"
- Medium startup, large hitboxes, wide offsets
- Medium damage, diagonal knockback angles
- Strength: keeps opponent at preferred range
- Weakness: opponent gets inside their range and they struggle
- Commitment is moderate but positioning is everything

**All-Rounder** — "Good at everything, best at nothing"
- Moderate numbers across the board
- No glaring weakness, no dominant tool
- Strength: adaptable
- Weakness: specialist opponents outperform them in their niche

"Your fighters don't have to be these archetypes exactly. But they can't be the same archetype. Two all-rounders with the same numbers is a failed assignment."

### Combo Theory (Brief)

"A combo happens when the opponent can't act before your next attack connects."

The rule: **If attack A's hitstun > attack B's startup, A combos into B.**

- Jab hitstun: 0.15s. Forward Punch startup: 7 frames = 0.117s. Jab combos into Forward Punch.
- But Forward Punch hitstun: 0.25s. Power Smash startup: 15 frames = 0.25s. That's frame-perfect — basically doesn't combo.

"Design at least one reliable 2-hit sequence per fighter. It doesn't have to be long. Jab into Forward is a combo. That's enough."

"Don't overthink this. Combo design comes from tuning hitstun and startup. You'll refine it in Session 5."

### Kill Move Design

"The knockback formula: `base * (2 - healthPercent)`. Your kill move needs high base knockback, because at low health it gets multiplied."

"But it should also be risky — slow startup or long recovery. If your kill move is safe AND strong, there's no reason to use anything else."

"Every team should be able to answer: **What is your fighter's kill move?**"

---

## Work Block 2 (remaining ~50 min): Create Fighter 2 + Playtest

### Student Task

1. **Create AttackData for Fighter 2** — must feel different from Fighter 1
2. **Begin Fighter scripts** if not started — `Fighter1.cs` and `Fighter2.cs` extending FighterBase (at minimum: `FighterName` override)
3. **Partner playtest** — two team members each pick a different fighter and play against each other

### Key Exercise

After playtesting, each team answers these questions out loud (to you or each other):
- "Which fighter is faster?"
- "Which fighter hits harder?"
- "Which fighter has the kill move that terrifies you at low health?"
- "What's the risk of using Fighter 1's strongest attack vs Fighter 2's?"

If they can't answer these, the fighters aren't different enough yet.

### What to Watch For

- **Both fighters with identical data:** "Did you copy the AttackData? Your fighters need different numbers. That's the whole point."
- **Nobody playtesting:** "Stop creating data. Play the game. You can't feel frame data in the Inspector."
- **Fights that end too fast:** "Knockback is probably too high. Lower the base knockback so fights last longer at full health."
- **Fights that never end:** "Kill move knockback is too low, or there's no kill move at all. Give one attack significantly higher base knockback."

### Commit Reminder

"Commit your AttackData assets and any new scripts before you leave. These are your fighters' DNA."

---

## Homework for Session 5

- [ ] Both fighters fully playable with 3+ attacks each
- [ ] Fighter scripts created (extending FighterBase with unique FighterName)
- [ ] Attacks feel intentionally different between the two fighters
- [ ] UI is wired and working (from Session 3 homework — if still not done, finish it)
- [ ] Continue art and animation work (Guide 06)
- [ ] Play at least 3 full matches as a team. Note what feels good and what doesn't.

---

## Session Checkpoint

By end of this session, every team should be able to answer:

- **"What are the three phases of an attack?"** — Startup, active, recovery. Startup is commitment before the hit. Active is when it can connect. Recovery is vulnerability after.
- **"What makes a jab different from a power smash?"** — Numbers, not code. Startup, damage, knockback, recovery — all defined in AttackData.
- **"Why can't you move during an attack?"** — The attack lifecycle is a state machine. AttackController.IsAttacking is true during all three phases, which feeds into CanAct.
- **"What's different about your two fighters?"** — They should have a concrete answer about speed, power, risk, or playstyle — backed by actual frame data.

---

## Instructor Notes

- Students who played fighting games before will pick up frame data quickly. Students who haven't need the concrete examples — "3 frames = 50 milliseconds, that's faster than you can blink" helps anchor the abstraction.
- The biggest risk this session is students spending all their time in the Inspector and never testing. Push them into play mode early and often. "Does it feel right?" is the only question that matters.
- If a team is struggling with fighter identity, give them a constraint: "Fighter 1 can only have attacks with 5 or fewer startup frames. Fighter 2 can only have attacks with 10 or more. Now what?"
- The state machine concept in Lecture 1 is intentionally light — you're planting a seed that grows in Session 5. Don't over-explain it. "You press attack, you're locked in for N frames" is enough for now.
- Resist comparing student work to Smash Bros or other polished games. "Does it feel intentional?" is the bar, not "does it feel AAA?"
