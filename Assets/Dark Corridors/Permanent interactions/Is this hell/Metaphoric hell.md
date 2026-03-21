---
id: metaphoric_hell
type: interaction
scope: permanent
feature: is_this_hell
prompt:
  id: is_this_hell_1_confirm
  role: metaphoric
aliases:
  - Metaphoric
  - Metaphoric hell
  - Metaphorical
  - Metaphorical hell
  - As metaphoric hell
  - As metaphorical hell
  - I view it as metaphoric
  - I view it as metaphoric hell
  - I view it as metaphorical
  - I view it as metaphorical hell
entry:
  all:
    - completed: is_this_hell_1
    - prompt_active: is_this_hell_1_confirm
mutations:
  - open_prompt:
      id: metaphoric_hell_confirm
      mode: exclusive_global
      lock_other_choices: true
---

Most definitely. I doubt {{style:emphasis|hell exists}}.

[[Hell exists]]

[[Hell doesn't exist]]
