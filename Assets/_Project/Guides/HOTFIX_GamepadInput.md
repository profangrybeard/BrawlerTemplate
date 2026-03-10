# Hotfix: Gamepad Input (Both Controllers Move Both Fighters)

**Posted:** March 10, 2026
**Affects:** Any team using gamepads (two controllers, or one controller + one keyboard)
**Does NOT affect:** Teams using keyboard only (two players sharing one keyboard)

---

## The Problem

If you plug in two gamepads, both controllers move **both** fighters at the same time. If you use one gamepad and one keyboard, the gamepad still moves both fighters while the keyboard side works fine.

This is a bug in `PlayerInputHandler.cs`. The gamepad bindings use the generic path `<Gamepad>`, which matches every connected controller. There is no code telling Unity "Player 1 should only listen to the first gamepad" and "Player 2 should only listen to the second gamepad." Keyboard input is not affected because Player 1 and Player 2 already use completely different keys (WASD vs Arrows).

---

## The Fix (2 minutes)

**One file. One paste. No other changes.**

### Step 1 — Open the file

Open `Assets/_Project/_Shared/Scripts/Input/PlayerInputHandler.cs` in your IDE.

### Step 2 — Find this line

Search for:

```csharp
moveAction = playerMap.FindAction("Move");
```

It should be around **line 199** (the exact number may vary if you've added code above it). It is inside the `SetupInputActions()` method.

### Step 3 — Paste this block DIRECTLY ABOVE that line

```csharp
            // Restrict this cloned asset to only this player's devices.
            // Without this, <Gamepad> bindings match ALL connected gamepads,
            // causing both fighters to respond to every controller.
            // Keyboard is always included (bindings are already unique per map).
            var gamepads = Gamepad.all;
            if (gamepads.Count > 0)
            {
                InputDevice keyboard = Keyboard.current;
                InputDevice gamepad = gamepads.Count > playerIndex
                    ? gamepads[playerIndex] : null;

                if (keyboard != null && gamepad != null)
                    inputActions.devices = new InputDevice[] { keyboard, gamepad };
                else if (keyboard != null)
                    inputActions.devices = new InputDevice[] { keyboard };
                else if (gamepad != null)
                    inputActions.devices = new InputDevice[] { gamepad };

                Debug.Log($"[InputFix] P{playerIndex + 1} devices: " +
                    $"keyboard={keyboard?.name ?? "none"}, " +
                    $"gamepad={gamepad?.name ?? "none"} " +
                    $"(of {gamepads.Count} connected)", this);
            }
```

### Step 4 — Save and test

That's it. No other files need to change. No new `using` statements are needed — the types (`InputDevice`, `Gamepad`, `Keyboard`) are already covered by the existing `using UnityEngine.InputSystem;` at the top of the file.

---

## After the fix, your method should look like this

```csharp
private void SetupInputActions()
{
    if (inputActions == null)
    {
        Debug.LogError($"[PlayerInputHandler] '{gameObject.name}' (P{playerIndex + 1}) " +
            "InputActionAsset not assigned! Drag 'BrawlerInputActions' into the Input Actions field.", this);
        return;
    }

    // Find the action map - we use Player1 or Player2 based on index
    string mapName = playerIndex == 0 ? "Player1" : "Player2";
    var playerMap = inputActions.FindActionMap(mapName);

    // Fallback to generic "Player" map if specific maps don't exist
    if (playerMap == null)
    {
        playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogError($"[PlayerInputHandler] '{gameObject.name}' (P{playerIndex + 1}) " +
                $"No '{mapName}' or 'Player' action map found in the InputActionAsset!", this);
            return;
        }
    }

    // Restrict this cloned asset to only this player's devices.
    // Without this, <Gamepad> bindings match ALL connected gamepads,
    // causing both fighters to respond to every controller.
    // Keyboard is always included (bindings are already unique per map).
    var gamepads = Gamepad.all;
    if (gamepads.Count > 0)
    {
        InputDevice keyboard = Keyboard.current;
        InputDevice gamepad = gamepads.Count > playerIndex
            ? gamepads[playerIndex] : null;

        if (keyboard != null && gamepad != null)
            inputActions.devices = new InputDevice[] { keyboard, gamepad };
        else if (keyboard != null)
            inputActions.devices = new InputDevice[] { keyboard };
        else if (gamepad != null)
            inputActions.devices = new InputDevice[] { gamepad };
    }

    moveAction = playerMap.FindAction("Move");
    jumpAction = playerMap.FindAction("Jump");
    dashAction = playerMap.FindAction("Dash");
    attackAction = playerMap.FindAction("Attack");
    specialAction = playerMap.FindAction("Special");

    // ... error logging continues below ...
}
```

---

## How to test

| Setup | Expected behavior |
|---|---|
| Two gamepads | Gamepad 1 controls only Player 1. Gamepad 2 controls only Player 2. |
| One gamepad + keyboard | Gamepad controls one fighter. Keyboard (WASD or Arrows) controls the other. |
| Keyboard only | No change from before — WASD is P1, Arrows is P2. |

If gamepads seem swapped (P1's controller moves P2), try swapping which USB port each controller is plugged into. The operating system assigns controller order by connection order.

---

## Why this works

Each `PlayerInputHandler` already clones the `InputActionAsset` in `Awake()` so the two players get independent action state. The fix sets `inputActions.devices` on each clone to restrict which physical devices it listens to:

- **Player 1's clone** listens to Keyboard + Gamepad[0] (first connected controller)
- **Player 2's clone** listens to Keyboard + Gamepad[1] (second connected controller)

When no gamepads are connected (`gamepads.Count == 0`), the block is skipped entirely and behavior is identical to the original code.
