---
id: pull_at_door_dent
type: interaction
scope: room_local
room: first_room
feature: metal_door
aliases:
  - Pull at door dent
  - Pull on door dent
  - Rip off door protrusion
  - Rip out indentation
  - Open the door dent
  - Widen door gap
  - Widen door's opening
  - Grab door dent
entry:
  completed: inspect_dent_in_door
mutations:
  - open_prompt:
      id: pull_at_door_dent_confirm
      mode: exclusive_global
      lock_other_choices: true
---

Carefully, you grab the {{style:emphasis|protruding corner}} with both hands and slowly begin to pull on it.

@cue sfx: metal_creaking

As you increase the tension, you hear the metal creak, but not giving you much leeway.

It seems to be moving, but I'll really have to strain myself to pry it open. I might get hurt. Should I pull harder?

[[Pull harder door dent]]

[[Leave door dent]]