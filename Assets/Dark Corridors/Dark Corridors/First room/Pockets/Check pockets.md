---
id: check_pockets
type: interaction
scope: room_local
room: first_room
feature: pockets
aliases:
  - Check pockets
  - Check pocket
  - Check your pockets
  - Inspect pockets
  - Inspect your pockets
  - Inspect pocket
---

@variant carried_nothing_known
@entry:
  any:
    - completed: empty_pockets
    - completed: right_pocket

I already did that. I'm not carrying anything unknown at this time.

[[Empty pockets]]

@variant after_first_check
@entry:
  completed: check_pockets

There's definitely something in my {{style:emphasis|right pocket}}. I should see what it is.

@variant default
@entry:
  completed: get_up

@cue sfx: ruffling_pockets

Let's see… It's always good to check your pockets after blacking out.

As you pat your **{{style:emphasis|pockets}}** for any contents, you feel a {{style:emphasis|small object}} with smooth edges in the **{{style:emphasis|right pocket}}**.
