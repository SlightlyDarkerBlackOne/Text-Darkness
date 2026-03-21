---
id: air_duct_left_confirmed
type: interaction
scope: room_local
room: first_room
feature: air_duct_cover
aliases:
  - Keep going
  - Keep going left
  - Follow left path
  - Go forward
  - Forward
  - Left
entry:
  completed: air_duct_left
mutations:
  - transition_room: lower_room
---

@cue sfx: metal_creaking

The {{style:emphasis|duct}} creaks under your weight, it starts to wobble even more.

“Come on, hold out. Hold out just a bit-”

@cue sfx: metal_collapsing_and_mc_screaming

It doesn’t. The segment you’re on collapses. Screaming, you tumble further down into the darkness.

[[Lower room]]
