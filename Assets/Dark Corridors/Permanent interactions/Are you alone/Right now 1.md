---
id: right_now_1
type: interaction
scope: permanent
feature: permanent_interactions_are_you_alone
prompt:
  id: are_you_alone_confirm
  role: right_now
aliases:
  - Now
  - Right now
  - I mean right now
  - I mean now
  - Right now I mean
  - Now I mean
  - I meant now
  - I meant right now
  - Now I meant
  - Right now I meant
entry:
  all:
    - completed: are_you_alone
    - prompt_active: are_you_alone_confirm
---

@variant encountered_people
@entry:
  any:
    - completed: meet_frank
    - completed: meet_bellen
Right now, yes. But it seems there are more people here.

@variant at_home
@entry:
  completed: at_home
No one's here. Unless you count {{style:emphasis|Marvin}} sneaking somewhere about.

@variant default
@entry:
  completed: not_at_home
Seems like it. I don't think there's anyone around.