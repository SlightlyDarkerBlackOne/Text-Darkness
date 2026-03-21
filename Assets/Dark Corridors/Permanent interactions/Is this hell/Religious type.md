---
id: religious_type
type: interaction
scope: permanent
feature: is_this_hell
aliases:
  - You are not?
  - You aren't?
  - You're not?
  - You are not religious?
  - You're not religious?
  - Why are you not religious?
  - Why are you not religious
  - Why aren't you religious?
  - Why aren't you religious
  - Why are you not the religious type?
  - Why are you not the religious type
  - Why aren't you the religious type?
  - Why aren't you the religious type
  - How come you are not religious?
  - How come you are not religious
  - How come you're not religious?
  - How come you're not religious
  - How come you are not the religious type?
  - How come you are not the religious type
  - How come you're not the religious type?
  - How come you're not the religious type
entry:
  completed: literal_hell
mutations:
  - open_prompt:
      id: religious_type_confirm
      mode: exclusive_global
      lock_other_choices: true
---

Never believed in those bedtime stories. Neither did my {{style:emphasis|parents}}. Why would you care all of a sudden? Scared of the {{style:emphasis|afterlife}}?

[[Not scared of afterlife]]

[[Scared of afterlife]]
