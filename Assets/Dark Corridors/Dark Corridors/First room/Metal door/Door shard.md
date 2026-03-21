---
id: item_metal_door_shard
type: interaction
scope: room_local
room: first_room
feature: metal_door
aliases:
  - Pick up metal shard
  - Pick up metal door shard
  - Take door shard
entry:
  any:
    - completed: pull_harder_door_dent
    - completed: pipe_on_door_dent
mutations:
  - grant_item: item_rusty_metal_door_shard
---

You pick up the piece of the {{item:item_rusty_metal_door_shard|style=emphasis|door}} which you broke off. It's rusty, sharp and a bit bent.
