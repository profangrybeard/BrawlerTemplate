# Session 5: Game Feel — Hitstop, Hitstun & Knockback + Action Gating

**Date:** Tuesday, Mar 3
**Duration:** 2.5 hours
**Prerequisites:** Both fighters playable with 3+ attacks each. Match flow and UI working. Students have played several matches and have opinions about what feels good and what doesn't.

**Session Goal:** Students understand the three pillars of impact feel (hitstop, hitstun, knockback), can tune them per attack to create weight and escalation, implement juice elements, and understand the full picture of when and why a player cannot act.

---

## Lecture 1 (20 min): Control and the Lack of It

### Opening Hook

"Think about the last time you got hit in a fighting game and yelled at the screen. You couldn't move. You couldn't block. You couldn't do anything. That helpless feeling? That's designed. And it's what makes fighting games work."

### The Master Gate: CanAct

Reference `FighterBase.CanAct`:

```
CanAct = !IsInHitstun && !IsDead && !IsRespawning
```

When CanAct is false, the player's input is ignored. Movement stops. Attacks don't start. The character is on screen but the player is locked out.

This is **action gating** — the game deciding when input matters and when it doesn't.

### Three Layers of Control Removal

Last session introduced one layer. Now show all three:

**1. Attack commitment (self-inflicted)**
- You pressed attack. You're in startup/active/recovery. You chose this.
- Short commitment (jab) = small bet. Long commitment (power smash) = big bet.
- `AttackController.IsAttacking` governs this.
- "You did this to yourself."

**2. Hitstun (opponent-inflicted)**
- You got hit. You can't act. The opponent did this to you.
- Duration comes from the AttackData that hit you.
- `KnockbackHandler.IsInHitstun` governs this.
- This is the mechanic that makes combos possible and getting hit meaningful.
- "They did this to you."

**3. Game state (system-level)**
- Countdown: nobody can act. Round end: nobody can act. Match end: nobody can act.
- `GameManager.CurrentState` governs this. Movement and attacks check the state.
- "Nobody chose this. It's the rules."

"Your game is always asking: 'Can this player act right now?' The answer is a stack of checks. All three layers have to say yes."

### Hitstop: The Freeze Frame

When a hit connects, the game pauses briefly. Both the attacker and the defender freeze.

"Why freeze? Your brain needs a moment to register the impact. Without hitstop, hits feel like passing through fog. With it, they feel like collisions."

`HitstopManager` handles this — it pauses `Time.timeScale` for a fraction of a second.

Typical values:
- Light jab: 0.02-0.04s (barely perceptible, but you feel it)
- Medium attack: 0.04-0.06s (clear pause)
- Heavy smash: 0.06-0.10s (dramatic stop, sells the weight)
- KO-finishing hit: 0.08-0.12s (cinematic moment)

"The weight of a hit isn't just damage numbers — it's how long the world stops."

Reference "The Art of Screenshake" by Jan Willem Nijman — this talk covers hitstop and why it matters.

### Hitstun: The Lockout

After hitstop ends, time resumes — but the fighter who got hit is in hitstun. They can see what's happening, but they can't act. `KnockbackHandler` manages this state.

"Hitstun is what makes getting hit matter. Without it, you take damage but immediately fight back. There's no consequence to being hit. With hitstun, the attacker gets an advantage — maybe enough for a follow-up hit."

This connects directly to combos:
- If hitstun from attack A lasts longer than attack B's startup, A combos into B
- The attacker gets a guaranteed follow-up — the opponent literally can't escape
- Too much hitstun = feels unfair, oppressive. Too little = getting hit is meaningless.

"Hitstun is the most important number you'll tune. It decides whether your game has combos, and how long they are."

### Knockback: The Physics of Getting Launched

When hitstun starts, the victim also gets launched. `KnockbackHandler` applies a velocity based on the attack's base knockback and angle.

**The formula:**

```
finalKnockback = baseKnockback * (2 - healthPercent)
```

Walk through concrete examples with a Forward Punch (base knockback 8):

| Health | healthPercent | Multiplier | Final Knockback |
|--------|--------------|------------|-----------------|
| 100% (full) | 1.0 | 2 - 1.0 = **1.0x** | 8 (nudge) |
| 75% | 0.75 | 2 - 0.75 = **1.25x** | 10 (push) |
| 50% | 0.50 | 2 - 0.5 = **1.5x** | 12 (solid launch) |
| 25% | 0.25 | 2 - 0.25 = **1.75x** | 14 (dangerous) |
| 0% (dead) | 0.0 | 2 - 0.0 = **2.0x** | 16 (blast zone) |

"The same attack gets twice as dangerous at zero health. This creates the escalation arc: early in the match, hits probe and position. Late in the match, every hit could be the last."

**Knockback angle** decides direction:
- `(1, 0.3)` — mostly horizontal, slight upward = standard knockback
- `(0.2, 1)` — mostly vertical = launcher (sends toward top blast zone)
- `(1, -0.5)` — downward diagonal = spike (sends toward bottom blast zone)
- The X component flips based on facing direction — always sends opponent away

"Which of your attacks is the kill move? It needs high base knockback so the scaling sends them to a blast zone at low health. And it should be risky — slow startup or long recovery — because a safe kill move means you never need to use anything else."

---

## Work Block 1 (45 min): Tune Combat Feel

### Student Task

Open each AttackData and tune three values:

**1. Hitstop per attack**
- Light attacks: 0.02-0.04s
- Medium attacks: 0.04-0.06s
- Heavy/kill attacks: 0.06-0.10s
- The difference matters more than the absolute values — heavy should feel noticeably different from light

**2. Hitstun per attack**
- Think about combos: does the fast attack's hitstun allow a follow-up?
- Think about fairness: does the heavy attack's hitstun let you combo after a slow move? (Probably shouldn't — the damage alone should be reward enough.)
- Test with a partner: hit them, then try to hit them again. Can you? Should you be able to?

**3. Knockback per attack**
- Identify the kill move: give it significantly higher base knockback than other attacks
- Test at different health levels: at full health, does the kill move KO? (Probably shouldn't.) At low health? (It should.)
- Play a full match. Does it escalate? Do the last few hits feel more tense?

### The Key Exercise

"Play a full match start to finish. Don't focus on winning — focus on feel."
- Do light hits feel light?
- Do heavy hits feel heavy?
- Does the match get more tense as health drops?
- When you get hit, can you tell you're in hitstun?
- When the kill move connects at low health, is there a dramatic moment?

### Circulate and Watch For

| What You'll See | What to Say |
|-----------------|-------------|
| All attacks have the same hitstop | "Your jab and your power smash freeze for the same amount of time. The power smash should freeze longer — that's how you sell weight." |
| Hitstun too long on light attacks | "Your jab locks them out for half a second. That's oppressive — light attacks should have short hitstun. Save the long hitstun for heavies." |
| Knockback too high on everything | "Every attack sends them to the blast zone at half health. If everything is a kill move, nothing is. Lower the base knockback on most attacks, keep it high on one." |
| Knockback too low — matches go forever | "Your kill move has the same knockback as your jab. At low health, it needs to actually kill. Raise the base knockback on your strongest attack." |
| Nobody testing at different health levels | "Hit them a few times first, THEN test the kill move. The formula scales with damage taken — it's supposed to feel different at low health vs full health." |
| Students tuning in Inspector without playing | "Close the Inspector. Play a match. Feel it. Then adjust. The Inspector is where you make changes, play mode is where you evaluate them." |

### Check-In (around 30-40 min)

Ask each team: **"Play me your game. Hit me with your jab, then hit me with your kill move. I want to feel the difference."**

If the jab and the kill move feel the same — they need more contrast. If the kill move at low health doesn't threaten a KO — base knockback needs to go up.

---

## Break (15 min)

---

## Lecture 2 (20 min): Juice — Feedback Beyond Mechanics

### The Assignment Requirement

"The assignment requires 3 juice elements. Hitstop counts as one — you just tuned it. You need two more."

### Why Juice Matters

Juice is feedback that matches the action. It doesn't change gameplay — it communicates gameplay.

"Without juice, a hit is: health number goes down. With juice, a hit is: the screen freezes, the fighter flashes red, sparks fly, the camera shakes, and the health bar jolts. Same damage. Completely different experience."

"Juice magnitude should match action weight. Light jab = tiny flash. Power smash = big shake + sparks + loud crack. If everything is equally juicy, nothing stands out."

### The Options (with implementation approaches)

Walk through each briefly — students pick their 2+:

**1. Damage Flash (easiest starting point)**
- When `OnTakeDamage` fires, tint the `SpriteRenderer` color for a few frames
- `ExampleFighter` already does this — it's a coroutine that sets color to red, waits a couple frames, sets it back
- Students can reference `ExampleFighter.DamageFlashCoroutine` as a pattern
- This is the lowest-effort, highest-impact juice element

**2. Screen Shake**
- On heavy hits or KOs, offset the camera position by a small random amount each frame, decaying over time
- A coroutine on a camera shake component: generate random offset, apply to camera position, reduce magnitude each frame, stop when magnitude is near zero
- Subscribe to `GameEvents.OnFighterDamaged` and check the knockback force — only shake on heavy hits
- Or subscribe to `GameEvents.OnFighterKO` for KO-specific shake

**3. Hit Particles**
- Spawn a ParticleSystem at the hit point when damage is dealt
- Short burst (not looping), maybe 10-20 particles, quick fade
- Can be as simple as a star-burst or spark prefab
- Instantiate at the point of contact or use object pooling if ambitious

**4. Sound Effects**
- `AudioSource.PlayClipAtPoint(clip, position)` for one-shot sounds
- AttackData already has fields for Attack Sound (whoosh on swing) and Hit Sound (impact on connection)
- Assign AudioClips in the AttackData Inspector
- Different weight attacks should have different sounds — light tap vs heavy crack

**5. Knockback Trails**
- Add a `TrailRenderer` component to the fighter
- Enable it only during knockback (when `IsInHitstun` is true)
- Creates a visual streak showing the launch path
- Cheap and effective for selling the force of a hit

**6. UI Reactions**
- Health bar shakes or flashes when damage is taken
- Screen border flash on heavy hits (a full-screen UI Image that flashes and fades)
- Add to `HealthBarUI` — on the damage event, trigger a brief animation

### Recommended Priority

"If you're not sure where to start:"
1. **Damage flash** — easiest, already demonstrated in ExampleFighter
2. **Screen shake** — medium effort, big payoff, works for heavy hits and KOs
3. **Sound effects** — medium effort, requires finding or making audio clips, but AttackData already has the fields
4. **Particles** — more setup but very visual
5. **Trails / UI reactions** — bonus if you have time

### Implementation Pattern

All juice follows the same event-driven pattern from Session 3:

```
Something happens (hit, KO, damage)
  -> Event fires
  -> Juice system subscribes and reacts
  -> Visual/audio feedback plays
  -> Feedback ends after brief duration
```

"You already know this pattern. Subscribe to an event. Do something in the handler. This is just more subscribers."

---

## Work Block 2 (remaining ~50 min): Implement Juice + Cross-Team Playtesting

### Student Task (first 30 min)

Implement at least 2 juice elements. Recommended starting order:
1. Damage flash (if not already inherited from ExampleFighter)
2. Screen shake OR sound effects
3. Whatever appeals to the team

### Cross-Team Playtesting (last 20 min)

Pair up teams. Each team plays the other team's game. Rules:

**For the watching team:**
- Don't explain anything. Watch silently.
- Note: Do they react to hits? Do they understand when they're in hitstun? Do they try the kill move at low health?

**For the playing team:**
- Just play. Try to win.
- After playing, answer: "What felt good? What felt off? When you got hit, could you tell you were stunned?"

**Debrief questions for each team after playtesting:**
- Did players react differently to light vs heavy hits?
- Did the match feel more tense as health dropped?
- Was there a moment where the juice made something feel satisfying?
- Was there a moment where something felt flat or missing?

### What to Watch For

| What You'll See | What to Say |
|-----------------|-------------|
| Juice on everything equally | "Your jab shakes the screen as much as your power smash. Scale the intensity. Light = subtle, heavy = dramatic." |
| Juice that fires but is invisible | "Your damage flash might be too short, or the color change too subtle. Make it obvious during testing, then dial back." |
| Screen shake that's nauseating | "Shake magnitude is too high or duration too long. Start small — even 2-3 pixels of offset for a few frames reads as impact." |
| Team skipping playtesting to keep coding | "Stop coding. Watch someone else play your game. You learn more from watching a stranger play for 2 minutes than tuning in the Inspector for 20." |

### Commit Reminder

"Commit before you leave. Your attack tuning and juice implementations are major progress. Don't risk losing them."

---

## Homework for Session 6

- [ ] All juice elements working (3 minimum including hitstop)
- [ ] Both fighters fully playable with tuned attacks — fights feel good
- [ ] Art and animation work in progress (Guide 06) — at minimum, idle and run animations
- [ ] Play 3+ full matches as a team. Write down one thing that feels great and one thing that feels off.
- [ ] Think about: "When can your player NOT act, and why?" Prepare to explain all three layers.
- [ ] Think about: "What's one game rule you changed from the template defaults and why?" (for the final playtest)

---

## Session Checkpoint

By end of this session, every team should be able to answer:

- **"What is hitstop and why does it exist?"** — A brief freeze on hit that gives the player's brain time to register the impact. Without it, hits feel weightless.
- **"What is hitstun and what does it enable?"** — A period where the hit fighter can't act. It creates consequences for getting hit and enables combos. Duration determines combo potential.
- **"How does knockback scale with damage?"** — `base * (2 - healthPercent)`. Same attack, double knockback at zero health vs full health. This creates the escalation arc.
- **"When can a player NOT act, and why?"** — Three layers: attack commitment (self-inflicted, chose to attack), hitstun (opponent-inflicted, got hit), game state (system-level, countdown/round end). All must allow action for input to matter.
- **"What's your kill move?"** — One specific attack with high base knockback designed to KO at low health, with enough risk (slow startup or long recovery) to make it a meaningful choice.

---

## Instructor Notes

- This session is where the game starts to feel like a game. The transformation from "functional combat" to "satisfying combat" is dramatic and immediate. Students usually have an "aha" moment when they first tune hitstop correctly.
- The CanAct / action gating lecture connects Sessions 4 and 5 into a unified concept. In Session 4 they learned "attacks lock you in." Now they learn "getting hit locks you out." Same mechanism (gating), different source (self vs opponent vs system).
- Cross-team playtesting is critical and should not be skipped for more work time. Watching a stranger play your game reveals things you can't see yourself. Enforce the "watch silently" rule — the urge to explain is strong but defeats the purpose.
- If a team's game feels flat despite correct tuning, the problem is usually contrast — all attacks feel the same weight. Push them to make their lightest attack much lighter and their heaviest much heavier. Exaggerate first, refine later.
- Some students will want to add complex juice (particle systems with custom shaders, multi-stage screen effects). Redirect them: "Get something simple working first. A 3-line damage flash that ships beats a particle system that's still broken on playtest day."
- After this session, students have all four core concepts in hand (events, frame data, feel, state/gating). Sessions 6 and 7 are integration and polish. The teaching is done — the rest is making.
