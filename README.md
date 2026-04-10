# Dark Corridors (Unity Narrative Prototype)

## What this app does

Dark Corridors is a **text-driven narrative game prototype** built in Unity.
The player explores scenes by typing free-text actions (for example: `look around`, `open eyes`, `call marvin`), and the game resolves those inputs into story choices, room transitions, events, and inventory updates.

At runtime, the game currently reads story content from a JSON dataset and drives:

- room/scene descriptions
- choice matching via text variations
- conditional choices (required items / required previous choices)
- inventory rewards
- simple puzzle flows
- room transitions
- typewriter-style output and title reveal moments

---

## Core gameplay flow

1. **Game starts** from a configured JSON file.
2. **RoomManager** displays the current room description with a typewriter effect.
3. Player enters text in the input field.
4. **ChoiceManager** tries to match input against available `text_variations`.
5. If a valid choice is found and requirements are satisfied:
   - result text is queued and displayed
   - optional puzzle state is handled
   - inventory/title/room transition effects are applied
6. Flow repeats until the story reaches an ending or loops into new rooms/branches.

---

## Runtime architecture (high level)

- `GameStateManager`
  - bootstraps the runtime and initializes the first dataset
- `RoomManager`
  - owns current room state
  - prints narrative text and handles typewriter/skip behavior
  - transitions between rooms
- `ChoiceManager`
  - resolves typed input to matching choice definitions
  - enforces requirements
  - tracks completed choices and inventory
  - handles developer helper commands (`/choices`, `/finished`)
- `GameDataDeserializer`
  - parses JSON into runtime room/choice objects
- `NotificationManager`
  - shows inventory pickup notifications

Main runtime data models:

- `RoomData`
- `Choice`
- `Puzzle`

---

## Content sources

### 1) Runtime content (active)

- `Assets/Game v1/Dark.json`

This JSON is the active source used by the current runtime parser and gameplay loop.

### 2) Story authoring docs/content (work-in-progress)

- `Assets/Dark Corridors/...` (`.md` story files, design notes, branching docs)

This folder includes legacy markdown story files and migration/spec documents for a more structured branching DSL.

---

## JSON shape expected by runtime

Top-level format:

```json
{
  "rooms": {
    "room_id": {
      "title": "Optional title",
      "description": ["line1", "line2"],
      "choices": {
        "choice_key": {
          "id": "choice_id",
          "text": "UI text",
          "text_variations": ["typed synonym 1", "typed synonym 2"],
          "result": ["narrative line 1", "narrative line 2"],
          "requires_item": "optional_item",
          "requires_choices": ["optional_previous_choice_id"],
          "adds_to_inventory": "optional_item_reward",
          "next_room": "optional_next_room_id",
          "puzzle": {
            "type": "number_code",
            "correct_code": "4732",
            "result": "post puzzle line",
            "adds_to_inventory": "optional_item"
          }
        }
      }
    }
  }
}
```

---

## How to run

## Prerequisites

- Unity Editor `6000.0.43f1` (from `ProjectSettings/ProjectVersion.txt`)

## Steps

1. Open the project in Unity.
2. Open scene: `Assets/Scenes/GameScene.unity`
3. Press Play.
4. Type narrative commands in the input field to progress.

---

## Testing helpers

- `Assets/Game v1/Tests/StorySimulator.cs` contains a simulation utility.
- You can run simulation methods through Unity Inspector context menu (`Start Simulation`) on the component.

---

## Current limitations

- Input matching is based on `Contains` over `text_variations` (simple but permissive).
- Puzzle handling is basic (`number_code` path is implemented; other puzzle types are minimal).
- Story authoring is split between active JSON runtime format and markdown drafting/spec files.

---

## Project structure (important folders)

- `Assets/Game v1/` - runtime scripts + active JSON story dataset
- `Assets/Dark Corridors/` - markdown story content, branching specs, and design notes
- `Assets/Scenes/` - Unity scenes
- `ProjectSettings/` - Unity project configuration

