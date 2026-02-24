# Session 3: Event-Driven Architecture

**Date:** Tuesday, Feb 24
**Duration:** 2.5 hours
**Prerequisites:** Teams have forked repos, working DevArena scenes, ExampleFighters moving and attacking. GameManager scaffold is unwired. No UI yet.

**Session Goal:** Every team leaves with a working match loop — countdown, fighting, KO, round end, respawn, match winner. They understand the observer pattern and can subscribe/unsubscribe to events.

---

## Lecture 1 (20 min): How Systems Talk to Each Other

### Opening Hook

"Right now your fighters can punch each other off the screen, and nothing happens. No round ends. No score. No winner. The GameManager exists but it's deaf — it has no idea a KO happened. How do we connect them?"

### Three Approaches (Build the Case)

**1. Polling** — GameManager checks every frame: "did anyone die yet?"
- Wasteful. Every system checking every other system every frame doesn't scale.
- 10 systems checking each other = 100 checks per frame. Most are "nope, nothing happened."

**2. Direct reference** — Fighter calls `gameManager.HandleKO()` directly.
- Works, but now Fighter depends on GameManager. What if you want a UI system that also reacts to KOs? Modify Fighter again. Every new listener means the sender changes.
- This is tight coupling. It makes code fragile and hard to extend.

**3. Events (observer pattern)** — Fighter announces "I died." Whoever cares, listens.
- GameManager subscribes: "tell me when someone dies." UI subscribes: "tell me too." A replay system could subscribe later. Fighter never changes.
- The sender doesn't know or care who's listening. That's the point.

### The Pattern in C#

```csharp
// Declare (in GameEvents)
public static Action<FighterKOEventArgs> OnFighterKO;

// Subscribe — "call me when this happens"
GameEvents.OnFighterKO += HandleKO;

// Fire — "this just happened, tell everyone"
GameEvents.OnFighterKO?.Invoke(args);

// Unsubscribe — "stop calling me"
GameEvents.OnFighterKO -= HandleKO;
```

- `Action<T>` is a delegate — a slot for functions that take `T` as input.
- `+=` adds a listener. `-=` removes one. `?.Invoke()` fires to all listeners.
- The `?.` is null-safe: if nobody is listening, it doesn't crash.

### Reference: GameEvents

`GameEvents` is a static class with nothing but Action fields. That's the entire event bus. No inheritance, no interfaces, no magic. Just a place where events live so any system can find them.

### Trace the Flow

Walk through what happens when a fighter hits a blast zone:

```
Fighter enters blast zone trigger
  -> BlastZone.OnTriggerEnter2D() detects it
  -> BlastZone fires GameEvents.OnFighterKO?.Invoke(...)
  -> GameManager.HandleKO() runs (because it subscribed)
  -> Round ends, score updates, next round starts
```

BlastZone doesn't import GameManager. GameManager doesn't import BlastZone. They communicate through the event. If you deleted GameManager from the scene, BlastZone would still fire the event — it would just go unheard.

### The Rule: Always Unsubscribe

"If you subscribe in `Start` or `OnEnable`, you must unsubscribe in `OnDestroy` or `OnDisable`."

Why? If the subscriber is destroyed but the event still holds a reference, the next `?.Invoke()` calls a dead object. Null reference exception. Memory leak.

It's like giving someone your phone number, then canceling the line without telling them. They call, nobody answers, things break.

```csharp
void Start()    { GameEvents.OnFighterKO += HandleKO; }
void OnDestroy(){ GameEvents.OnFighterKO -= HandleKO; }
```

This subscribe/unsubscribe pair is the pattern they'll use 10+ times in this project.

---

## Work Block 1 (45 min): Wire GameManager

**Student task:** Open Guide 02 (Match Flow and Arena) and complete the 5 TODO steps in `GameManager`.

Steps are sequential — each builds on the last:
1. Subscribe to KO events
2. Position fighters at spawn points
3. Countdown coroutine
4. End round coroutine
5. Win condition check

Each step is small (mostly uncommenting existing code). Encourage testing after each step, not all at once.

### Circulate and Watch For

| What You'll See | What to Say |
|-----------------|-------------|
| "Nothing happens when I press play" | "Is MatchConfig assigned on the GameManager? Check the Inspector. StartMatch aborts if it's missing — check Console for a red message." |
| "Fighters don't move at all" | Same — MatchConfig missing means fighters never initialize. This is the #1 gotcha. |
| "KO happens but no round end" | "Did you do Step 1? Check if the event subscription is uncommented." |
| "Round ends but fighters don't respawn" | "Step 2 — the spawn positioning loop. Is it uncommented?" |
| "Everything works but match never ends" | "Step 5 — the win condition check. Did you uncomment both the method and the call?" |
| Student doing all 5 steps before testing | "Stop. Test after Step 1. You should see a console log on KO. Then do Step 2 and test again." |

### Check-In Question (around 30-40 min mark)

Walk around and ask each team: **"Can you play a full match to a winner?"**

If yes — they're done with this block. Point them to the match test checklist at the bottom of Guide 02.

If no — diagnose. It's almost always one of: MatchConfig missing, event not subscribed, or a TODO step skipped.

---

## Break (15 min)

---

## Lecture 2 (20 min): The Event Web

### The Bigger Picture

"You just wired one event subscriber — GameManager listening for KOs. But the template has eight events. Most of them are firing into the void right now. Let's map the whole system."

Draw or show the event map:

```
SOURCES                     EVENTS                      LISTENERS (current)
-------                     ------                      -------------------
BlastZone          ------>  OnFighterKO         ------> GameManager
GameManager        ------>  OnRoundStart        ------> (nobody yet)
GameManager        ------>  OnRoundEnd          ------> (nobody yet)
GameManager        ------>  OnRoundScoreChanged ------> (nobody yet)
GameManager        ------>  OnGameStateChanged  ------> (nobody yet)
GameManager        ------>  OnMatchEnd          ------> (nobody yet)
FighterHealth      ------>  OnFighterDamaged    ------> (nobody yet)
FighterBase        ------>  OnFighterRespawn    ------> (nobody yet)
```

"See all those '(nobody yet)' entries? Those events fire. Nobody hears them. And the game still works. That's the beauty of events — the sender never fails because nobody is listening."

### Events Enable Modular Teams

"When your UI teammate wires HealthBarUI, they subscribe to `OnFighterDamaged`. When your match UI person wires MatchUI, they subscribe to `OnRoundStart` and `OnMatchEnd`. Neither of them touches GameManager or FighterHealth. They just add listeners."

"If you wanted a camera shake system, you'd subscribe to `OnFighterKO`. Commentary system? `OnFighterDamaged`. Instant replay? Multiple events. The systems that fire events never change."

### Preview: UI Homework

"Guide 03 walks through wiring 3 UI scripts — HealthBarUI, RoundDisplayUI, MatchUI. Each follows the exact pattern you just learned:"

```
1. Subscribe to an event
2. Update visuals in the handler
3. Unsubscribe on destroy
```

"HealthBarUI subscribes to `FighterHealth.OnHealthChanged` and updates a fill bar. RoundDisplayUI subscribes to `OnRoundScoreChanged` and shows round wins. MatchUI subscribes to `OnRoundStart`, `OnGameStateChanged`, and `OnMatchEnd` to show countdowns and the winner."

"This is homework for Thursday. Come to Session 4 with health bars and round display working."

### One More Concept: Component Events vs Global Events

"You'll notice `FighterHealth.OnHealthChanged` is an instance event on a specific component — not a static global event like `GameEvents.OnFighterKO`."

"When should you use which?"
- **Global (GameEvents):** When multiple systems across the game need to react. KOs, round changes, match state.
- **Component (instance events):** When one specific thing cares about one specific component. HealthBarUI cares about one fighter's health, not every fighter's.

"The pattern is the same either way. Subscribe, react, unsubscribe."

---

## Work Block 2 (remaining ~50 min): Test, Commit, Begin Fighters

### Priority Order

1. **Finish GameManager** if not done. Run the full match test checklist from Guide 02.
2. **Full match test** — play a complete match to a winner. Verify: countdown, fighting, KO, round end, respawn, next round, match winner.
3. **Commit.** Working GameManager is a significant milestone. Don't lose it.
4. **Look at ExampleFighter** — read the virtual methods (`OnFighterInitialized`, `OnTakeDamage`, `OnKO`, `OnRespawn`). Think about what their fighters might do in each.
5. **Start Fighter scripts** if time — create `Fighter1.cs` extending `FighterBase`, override `FighterName`, place it in `Fighter1/Scripts/`.
6. **Start UI** if ambitious — Canvas setup, HealthBarUI wiring.

### What to Watch For

- Teams that finish fast: point them toward starting their fighter scripts or UI. Don't let them sit idle.
- Teams that finished but didn't commit: "Commit now. If your partner breaks something tomorrow, you want this checkpoint."
- Teams still stuck on GameManager: sit with them. It's almost always the MatchConfig Inspector assignment.

---

## Homework for Session 4

- [ ] Wire all 3 UI scaffolds following Guide 03 (HealthBarUI, RoundDisplayUI, MatchUI)
- [ ] Build a Canvas with health bars for both fighters
- [ ] Come to Session 4 with a fully playable match including UI
- [ ] Read Guide 04 (Fighter Architecture) and Guide 05 (Combat System) before Thursday
- [ ] Each team member: think about what makes your fighter unique

---

## Session Checkpoint

By end of this session, every team should be able to answer:

- **"How does GameManager know when a fighter is KO'd?"** — It subscribes to `GameEvents.OnFighterKO`. BlastZone fires the event. They're decoupled.
- **"What happens if nobody subscribes to an event?"** — Nothing breaks. The event fires, nothing reacts. That's by design.
- **"Why do you unsubscribe in OnDestroy?"** — To prevent null references when the event tries to call a destroyed object.

---

## Instructor Notes

- This is the most structured session — the scaffold TODOs have a clear path. If students follow Guide 02 in order, it works.
- The most common blocker is MatchConfig not assigned. It causes StartMatch to abort silently, which means fighters never initialize, which means nothing moves. If a team says "nothing works," check this first.
- Resist the urge to demo the wiring live. The guide is step-by-step. Let them struggle with it — the learning is in the doing.
- The conceptual understanding matters more than the code. If a team can wire it but can't explain how events work, push on the "why" during check-ins.
