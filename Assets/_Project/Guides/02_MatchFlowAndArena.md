# Guide 02: Match Flow and Arena

This guide explains how the match system works and how the arena components fit together. The `GameManager` is one of the scaffold scripts your team needs to wire up.

---

## Match State Machine

The game flows through these states:

```
Match Start
    │
    ▼
┌──────────┐
│ Countdown│  "3... 2... 1... GO!"
└────┬─────┘
     │
     ▼
┌──────────┐
│ Fighting │◄────────────────┐
└────┬─────┘                 │
     │ (KO happens)          │
     ▼                       │
┌──────────┐                 │
│ RoundEnd │  Award point    │
└────┬─────┘                 │
     │                       │
     ▼                       │
  Winner? ──No──────────────►┘
     │
    Yes
     │
     ▼
┌──────────┐
│ MatchEnd │  "GAME!"
└──────────┘
```

The `GameManager` drives this loop. It tracks the current `GameState`, manages rounds, and determines a winner.

---

## How GameManager Works

The scaffold in `_Shared/Scripts/Core/GameManager.cs` has TODO comments walking through 5 steps. Here's what each step accomplishes and why:

### Event Subscription
`GameEvents.OnFighterKO` fires whenever a fighter touches a blast zone. The GameManager needs to listen for this event so it knows when a round should end. Subscribe in `Start()`, unsubscribe in `OnDestroy()`.

### Fighter Positioning
At the start of each round, fighters need to be placed at their spawn points. The `SpawnPoint` components in the scene define where each player spawns. Call `Respawn()` on each fighter with the spawn position.

### Countdown
Between rounds, there's a countdown before fighting begins. This uses a coroutine for timing. During countdown, fighters shouldn't be able to act. Fire `GameEvents.OnRoundStart` so the UI knows to display the countdown.

### Round End
When a KO happens, figure out who won (the *other* player), award the round, and fire `GameEvents.OnRoundEnd`. Add a brief pause before the next round starts.

### Win Condition Check
After each round, check if someone has won enough rounds (configured in `MatchConfig.roundsToWin`). If yes, end the match. If not, start the next round.

---

## Key Events

The `GameEvents` class is the communication bus. These are the events the match system uses:

| Event | Fires When | Who Listens |
|-------|-----------|-------------|
| `OnFighterKO` | Fighter hits blast zone | GameManager |
| `OnRoundStart` | New round begins | MatchUI |
| `OnRoundEnd` | KO registered, round awarded | MatchUI, RoundDisplayUI |
| `OnRoundScoreChanged` | Round win count updates | RoundDisplayUI |
| `OnGameStateChanged` | State transitions | MatchUI |
| `OnMatchEnd` | Someone wins the match | MatchUI |

The pattern is always the same: subscribe with `+=`, unsubscribe with `-=`, and fire with `?.Invoke()`.

---

## MatchConfig

`MatchConfig` is a ScriptableObject that controls match rules. Create one via **Create > Brawler > Match Config**.

| Property | Default | What It Does |
|----------|---------|-------------|
| `roundsToWin` | 2 | Rounds needed to win (2 = best of 3) |
| `roundStartDelay` | 3.0 | Countdown duration in seconds |
| `roundEndDelay` | 2.0 | Pause after round ends |
| `matchTimeLimit` | 0 | Time limit per round (0 = no limit) |
| `respawnInvincibilityDuration` | 2.0 | Invincibility after respawn |
| `enableSuddenDeath` | false | Sudden death if time runs out |

Your team can modify these values or change the match logic entirely. The `DESIGN_DOC.md` is where you plan your game's rules.

---

## Arena Systems

### BlastZone

`BlastZone` is a trigger collider that KOs any fighter that enters it. Place four around the arena edges (top, bottom, left, right) to define the boundaries.

Each BlastZone has a `ZoneType` (Top, Bottom, Left, Right) so the game knows which direction the fighter was knocked out.

### SpawnPoint

`SpawnPoint` marks where a fighter spawns. Each has a `playerIndex` (0 or 1). You need at least two in your scene — one per player.

The `Position` property returns the spawn location for the GameManager to use during respawns.

### Platform (Scaffold)

`Platform` uses Unity's `PlatformEffector2D` for one-way collision — fighters can jump up through it but stand on top. The scaffold has a TODO for drop-through logic: when a fighter presses down, temporarily disable collision so they fall through.

The hint in the code: use `Physics2D.IgnoreCollision()` to temporarily turn off collision between the fighter and platform, wait a short duration, then re-enable it.

---

## Building Your Arena

A basic arena needs:
- A ground platform (with collider, not a trigger)
- 4 blast zones around the edges (trigger colliders)
- 2 spawn points (playerIndex 0 and 1)
- Optional: additional platforms for verticality

Your team decides the arena layout. You can have multiple arenas in different scenes if you want.

---

## Testing

When the GameManager is wired up, you should see:
- [ ] Fighters spawn at their assigned positions
- [ ] Countdown happens before fighting begins
- [ ] KO triggers round end (fighter hits blast zone → round awarded to the other player)
- [ ] Round wins are tracked correctly
- [ ] Match ends when someone wins enough rounds
- [ ] The cycle resets properly for subsequent rounds

---

**Next:** [Guide 03: UI System](03_UISystem.md) — how UI components connect to these events
