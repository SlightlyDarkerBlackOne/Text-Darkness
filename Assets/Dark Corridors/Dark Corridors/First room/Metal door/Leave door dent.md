---
id: leave_door_dent
type: interaction
scope: room_local
room: first_room
feature: metal_door
prompt:
  id: pull_at_door_dent_confirm
  role: no
aliases:
  - "No"
  - Don't do it
  - Do not do it
  - Leave it
  - Leave it be
  - Don't risk it
entry:
  all:
    - completed: pull_at_door_dent
    - prompt_active: pull_at_door_dent_confirm
---

Still fearful of "the tetanus countdown", you let go of the {{style:emphasis|door}}.

If only I had something to use as a fulcrum, it would be far easier and safer to pry it open.
