---
id: left_pocket
type: interaction
scope: room_local
room: first_room
feature: pockets
aliases:
  - Check left pocket
  - Empty left pocket
  - Inspect left pocket
entry:
  completed: get_up
---

@variant already_checked
@entry:
  any:
    - completed: left_pocket
    - completed: empty_pockets

I already did that. There's nothing there.

@variant default

It's empty.
