---
id: explore_room_1
type: interaction
scope: room_local
room: first_room
feature: general
aliases:
  - Explore room
  - Explore the room
  - Inspect room
  - Inspect the room
  - Search room
  - Search the room
  - Explore wall
  - Explore the wall
  - Search wall
  - Search the wall
  - Inspect wall
  - Inspect the wall
  - Look for exit
  - Look for an exit
  - Search for an exit
  - Look for door
  - Look for a door
  - Look for window
  - Look for a window
  - Find door
  - Find a door
  - Find exit
  - Find an exit
  - Find window
  - Find a window
  - Explore apartment
entry:
  completed: not_at_home
mutations:
  - sanity:
      value: 20
      trigger: on_reveal
      token: sanity_drop
  - sanity:
      value: 70
      trigger: on_reveal
      token: sanity_recover
metadata:
  legacy_effect_note: Map gets updated, with metal panel and wooden object
---

With both palms on the wall, you feel your way around the room. The walls are cold and damp. Something you think might be old dried up paint is crumbling to the touch and sticking to your hands. You keep going until you come to the first corner of the {{style:emphasis|room}}, as well as accidentally kick something with your foot.

@cue sfx: wooden_hit

“There’s something there. Like a wooden object.” You keep exploring.

There's the second corner. Then the third. Your pores start to open as you realize the angle of the upcoming wall leads you to the start. There might not be any openings. Fear starts to crawl up your {{status:sanity_drop|spine}}, but after a few more steps between the third and the fourth corner of the room, the texture changes. “Wait, this is {{status:sanity_recover|different}}! It feels like a {{style:emphasis|metal panel}}… A rusty {{style:emphasis|metal panel}}. It might be a {{style:emphasis|door}}, but I should be careful how I touch it. The last thing I need is a tetanus countdown.”

[[Metal panel]]

[[Wooden cabinet]]
