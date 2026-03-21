---
id: are_you_alone
type: interaction
scope: permanent
feature: permanent_interactions_are_you_alone
aliases:
  - Are you alone?
  - Are you alone
  - Is anyone with you?
  - Is anyone with you
mutations:
  - open_prompt:
      id: are_you_alone_confirm
      mode: exclusive_global
      lock_other_choices: true
---

You mean like, {{style:emphasis|right now}} or in {{style:emphasis|general}}?

[[Right now 1]]

[[In general]]
