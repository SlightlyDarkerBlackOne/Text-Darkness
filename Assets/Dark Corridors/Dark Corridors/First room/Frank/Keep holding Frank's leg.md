---
id: keep_holding_franks_leg
type: interaction
scope: room_local
room: first_room
feature: frank
aliases:
  - Keep holding
  - Don't let go
  - Do not let him go
  - Hold his leg
entry:
  completed: threaten_frank
---

@say player
You know, I don't think I will.

@say stranger
W-What?! But I unbarred the {{style:emphasis|door}} for you! That was the deal!

@say player
Yeah, thanks for that. But a threat is hardly a deal. And you're still in a very vulnerable position. I think I'll ask you a few questions first. And if you don't cooperate...

@say stranger
AGHH! Alright, alright, I get it! I'll tell you whatever you want to know, just don't break my {{style:emphasis|leg}}, please. It's a death sentence.

[[Ask Frank about this place]]

[[Ask about Frank]]

[[Ask Frank about coin]]
