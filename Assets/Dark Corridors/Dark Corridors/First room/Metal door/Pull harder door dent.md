---
id: pull_harder_door_dent
type: interaction
scope: room_local
room: first_room
feature: metal_door
prompt:
  id: pull_at_door_dent_confirm
  role: yes
aliases:
  - "Yes"
  - Pull harder
  - Rip it off
  - Tear it off
  - Widen it
  - Widen gap
  - Open gap more
entry:
  all:
    - completed: pull_at_door_dent
    - prompt_active: pull_at_door_dent_confirm
---

@variant protected_hands
@entry:
  completed: status_hands_bandaged
You wrap the "DIY" {{style:emphasis|bandages}} around your hands to protect them and grip the {{style:emphasis|door's protrusion}}. As you start pulling at it with full force, you can feel the sharp metal edge digging into your palms, but the {{style:emphasis|bandages}} buffer them well enough for you to keep going. The metal creaks louder and louder, until...

@cue sfx: breaking_off_a_metal_shard

You rip off a part of the rusty {{style:emphasis|door}} itself.

Good, now that hole is bigger, still not enough to crawl through it. The {{style:emphasis|shard}} seems useful, though.

@variant default
You tighten your grip on the {{style:emphasis|door's protrusion}}, sink your feet firmly into the floor and begin to pull on it once more with all the strength you have. The metal creaks louder and louder, until...

@cue sfx: slight_slicing_noise

@cue sfx: metal_breaking_falling_on_floor

AH, FUCK!

Though rusty and old, the {{style:emphasis|door}} refused to give in and in the process cuts your {{style:emphasis|left palm}}. You're bleeding slightly.

Fucking great, I can't even see how deep it is! Ugh, doesn't feel too bad. It'll probably stop by itself, but a {{style:emphasis|bandage}} would still be nice.