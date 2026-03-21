---
id: calm_down
type: interaction
scope: room_local
room: first_room
feature: general
aliases:
  - Calm down
  - Relax
  - Take it easy
  - Take it slow
  - Try to calm down
  - Try to relax
  - Ease your breathing
  - Breathe slowly
  - Get a grip
  - Stay calm
entry:
  completed: not_at_home
mutations:
  - sanity:
      amount: 15
      trigger: on_reveal
      token: sanity
---

(breathes out slowly) Alright, just stay calm, breathe, we can do this. We can figure this out. There must be a door or a window somewhere.

Your breathing steadies, and the panic eases just enough to get yourself under {{status:sanity|style=emphasis|control}}.
