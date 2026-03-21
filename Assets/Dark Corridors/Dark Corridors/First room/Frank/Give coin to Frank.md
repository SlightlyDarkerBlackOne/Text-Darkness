---
id: give_coin_to_frank
type: interaction
scope: room_local
room: first_room
feature: frank
aliases:
  - Give him coin
  - Give coin
  - Offer metal coin
entry:
  all:
    - has_item: item_metal_coin
    - completed: call_out_to_frank
mutations:
  - remove_item:
      item: item_metal_coin
      trigger: on_reveal
      token: item_metal_coin
---

You stretch your hand through the {{style:emphasis|door's crack}} and offer the {{style:emphasis|coin}} to the stranger. As the person reaches to pick up the {{style:emphasis|coin}} resting on your open palm, they brush their fingers across it. You immediately notice them being worn out - cracked nails and arid skin. The person snatches the the {{item:item_metal_coin|style=emphasis|coin}} with intent. No, not intent. Something more akin to greed and desperation.

@say stranger
Ha-haha-HA! I never thought I'd get one as easily as this.

@say player
So it's worth a lot?

@say stranger
Oh, it's worth much more than that.

@cue sfx: steps_walking_away

@say player
Hey, where are you going? Let me out first!

@say stranger
Why should I? You're useless to me now.

@say player
WHAT?! You said we had a deal!

@say stranger
And you trust a stranger in the dark without ever seeing their face? Don't worry, I'll make more use of it than you ever could.

@say player
No! NO! Come back! Please! I...

@say player
I'll help you! It's easier if there's two of us, right?

@say stranger
You're not worth anything. I don't need a gullible mouth to feed. Just stay there and die quietly. It's for the best.

@cue sfx: banging_on_door

@say player
You son of a bitch! FUCK YOU! I'll rip you apart when I find you!

Ignoring your threats, the person leaves. You can hear their footsteps getting further and further away.

I swear to fucking Christ, when I get out of here I'll break all of his wrinkly, evil, fucking fingers…
