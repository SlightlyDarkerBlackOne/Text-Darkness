# Branching DSL Spec

## Cilj

Ovaj dokument definira markdown-based DSL za `Text Darkness`.

Cilj DSL-a je da:

- writer moze normalno raditi u Obsidianu
- parser moze pouzdano pretvoriti `.md` u canonical story graph
- branching, conditions, effects i inline keywordovi budu strukturirani
- vizualni graph editor u Unityju moze citati isti model

Ovo je **authoring DSL**.
Runtime ne bi trebao citati raw markdown direktno, nego generated graph asset.

## Osnovna pravila

### 1. Jedan fajl = jedan primary node

Svaki `.md` file predstavlja jedan primary `StoryNode`.

File moze imati:

- jedan glavni node
- opcionalne varijante / reakcije unutar istog nodea
- outgoing linkove prema drugim nodeovima

Ako jedan file sadrzi vise potpuno razlicitih nodeova odvojenih s `////////////////////////////////////////////////////////////////`, to treba smatrati legacy formatom i migrirati.

### 2. Source of truth

`Markdown` je source of truth.

Unity:

- parsira markdown
- validira reference
- generira graph asset
- prikazuje branching vizualno

### 3. Struktura dokumenta

Svaki node file ima:

1. YAML frontmatter
2. body content
3. opcionalne sekcije za outgoing links ili lokalne overrideove

Minimalni primjer:

```md
---
id: who_is_marvin
type: interaction
scope: permanent
aliases:
  - Who's Marvin?
  - Who is Marvin?
entry:
  any:
    - completed: introduce_yourself1
    - completed: right_now_1
links:
  - to: about_marvin
  - to: call_marvin_1
  - to: call_marvin_2
---

Cat. {{keyword:marvin|Marvin}}'s my cat. A lazy tabby.
{{keyword:marvin|Marvin}} cuddles with me when I'm down. So, like, every day...
```

## File format

## Frontmatter

Frontmatter je obavezan.

Podrzani top-level fieldovi:

- `id`
- `type`
- `scope`
- `room`
- `feature`
- `title`
- `tags`
- `aliases`
- `entry`
- `visibility`
- `priority`
- `cooldown`
- `once`
- `mutations`
- `links`
- `timers`
- `metadata`

### Obavezna polja

- `id`
- `type`
- `scope`

### Preporucena polja

- `aliases`
- `entry`
- `links`

## Field definitions

### `id`

Jedinstveni stabilni identifikator.

Pravila:

- lowercase
- snake_case
- ne mijenjati nakon sto ode u content usage

Primjer:

```yaml
id: how_you_got_here
```

### `type`

Podrzane vrijednosti:

- `interaction`
- `reaction`
- `event`
- `timed_event`
- `keyword_topic`
- `system`

Primjer:

```yaml
type: interaction
```

### `scope`

Podrzane vrijednosti:

- `room_local`
- `permanent`
- `global`
- `character`
- `system`

Primjeri:

```yaml
scope: room_local
room: first_room
```

```yaml
scope: permanent
```

### `room`

Obavezan za `room_local`.

Primjer:

```yaml
room: first_room
```

### `feature`

Logical group za graph editor i asset grouping.

Primjer:

```yaml
feature: marvin
```

### `title`

Opcionalni human-readable naslov za editor.

```yaml
title: Who Is Marvin
```

### `tags`

Za search, filtering i tooling.

```yaml
tags:
  - memory
  - character
  - intro
```

### `aliases`

Ovo zamjenjuje legacy `text_variations`.

```yaml
aliases:
  - Who's Marvin?
  - Who is Marvin?
  - Marvin?
  - Marvin
```

Parser generira input matching rule iz `aliases`.

### `entry`

Ovo zamjenjuje legacy `requirements`.

`entry` je structured condition tree.

Podrzani condition operatori:

- `all`
- `any`
- `not`

Podrzani leaf condition tipovi:

- `completed`
- `unlocked`
- `has_item`
- `missing_item`
- `flag`
- `keyword_seen`
- `room_is`
- `scope_active`
- `timer_active`
- `stat_at_least`
- `stat_at_most`

Primjeri:

```yaml
entry:
  completed: pull_franks_leg
```

```yaml
entry:
  any:
    - completed: introduce_yourself1
    - completed: right_now_1
```

```yaml
entry:
  all:
    - completed: pull_franks_leg
    - any:
        - has_item: item_metal_shard
        - has_item: item_wooden_stake
```

```yaml
entry:
  all:
    - flag:
        name: thirst_state
        equals: dehydrated_start
    - not:
        completed: how_you_got_here_sobered
```

### `visibility`

Odreduje je li node:

- `visible`
- `hidden`
- `debug_only`

Primjer:

```yaml
visibility: visible
```

### `priority`

Koristi se ako vise nodeova matcha isti input.

Vece = jace.

```yaml
priority: 200
```

### `cooldown`

Opcionalni throttle za repeatable nodeove.

```yaml
cooldown:
  turns: 2
```

### `once`

Ako je `true`, node se moze izvrsiti samo jednom.

```yaml
once: true
```

### `mutations`

Ovo je lista efekata koji se izvrse kad se node odigra.

`mutations` zamjenjuje legacy `event` string.

Podrzani effect tipovi:

- `grant_item`
- `remove_item`
- `set_flag`
- `unlock_node`
- `unlock_keyword`
- `reveal_choice`
- `play_sfx`
- `play_music`
- `play_vfx`
- `start_timer`
- `stop_timer`
- `transition_room`
- `introduce_mechanic`
- `add_status`
- `remove_status`
- `journal_entry`

Primjeri:

```yaml
mutations:
  - introduce_mechanic: thirst
  - unlock_keyword: water
```

```yaml
mutations:
  - grant_item: item_military_lighter
  - play_sfx: lighter_fail_wet
```

```yaml
mutations:
  - set_flag:
      name: frank_state
      value: hostile
```

### `links`

`links` definira outgoing graph edgeove.

Minimalni oblik:

```yaml
links:
  - to: about_marvin
  - to: call_marvin_1
```

Puni oblik:

```yaml
links:
  - to: about_marvin
    text: About Marvin
    visibility: visible
    priority: 100
  - to: call_marvin_2
    text: Call Marvin 2
    entry:
      completed: call_marvin_1
```

Podrzani fieldovi za link:

- `to`
- `text`
- `entry`
- `mutations`
- `visibility`
- `priority`
- `fallback`

### `timers`

Koristi se za timed event modele.

Primjer:

```yaml
timers:
  - id: frank_window
    duration_seconds: 20
    on_start:
      - play_sfx: approaching_footsteps
    on_expire:
      - unlock_node: stay_silent
```

### `metadata`

Slobodan prostor za editor-only ili import-only podatke.

Primjer:

```yaml
metadata:
  legacy_file_group: frank_sequence
  author_note: rewrite_later
```

## Body content

Body je narativni output nodea.

Body se sastoji od blockova.

Podrzani block tipovi:

- plain paragraph
- cue block
- choice hint block
- variant block

## Plain paragraph

Obican tekst = `NarrativeBlock`.

Primjer:

```md
Cat. {{keyword:marvin|Marvin}}'s my cat. A lazy tabby.
{{keyword:marvin|Marvin}} cuddles with me when I'm down. So, like, every day...
```

## Cue block

Za SFX, music, VFX ili sistemske cueove koristi se directive block.

Sintaksa:

```md
@cue sfx: inaudible_news_speaker
@cue sfx: shambling_walk
@cue sfx: doorbell_247
@cue sfx: sirens_ramping_noise
```

Alternativno, za grouped cues:

```md
@cue sequence:
  - sfx: inaudible_news_speaker
  - sfx: shambling_walk
  - sfx: doorbell_247
  - sfx: sirens_ramping_noise
```

Ovo zamjenjuje legacy:

```md
**==SFX: inaudible news speaker > shambling walk and night street sound > doorbell 24/7 > sirens and slowly ramping white noise==**
```

## Variant block

Ako isti node ima vise result varijanti pod razlicitim uvjetima, koristi `@variant`.

Sintaksa:

```md
@variant default
I was watching the news, trying to inebriate myself to sleep...

@variant sobered
@entry:
  all:
    - completed: how_you_got_here
    - flag:
        name: sobered_up
        equals: true
The things I said really happened, but there's something else I remembered.
```

`@variant default` je obavezan ako postoji vise varijanti.

Svaki `@variant` se mapira na child output branch unutar istog primary nodea ili na generated subnode, ovisno o importer strategiji.

## Inline token DSL

Legacy `==text==` vise nije dovoljan jer ne nosi strukturu.

Novi standard su `{{...}}` tokeni.

Osnovna sintaksa:

```text
{{type:id|display text}}
```

Prosirena sintaksa:

```text
{{type:id|display text|key=value,key2=value2}}
```

### Podrzani token tipovi

- `keyword`
- `item`
- `style`
- `sfx`
- `vfx`
- `status`
- `ref`

### Primjeri

Keyword:

```text
{{keyword:marvin|Marvin}}
```

Item mention:

```text
{{item:military_lighter|military lighter}}
```

Style-only:

```text
{{style:emphasis|door}}
```

Keyword s dodatnim metapodacima:

```text
{{keyword:water|water|color=blue,on_reveal=unlock_keyword:thirst}}
```

SFX trigger na odredenu rijec:

```text
{{sfx:cat_meow|Marvin|trigger=on_reveal}}
```

### Pravila za tokene

- `type` i `id` su obavezni
- `display text` je obavezan
- attributes su opcionalni
- token ne smije biti multiline
- parser mora fallbackati na plain text ako token nije validan, ali validation mora prijaviti gresku

## Outgoing links u bodyju

Za maksimalnu kompatibilnost s Obsidianom i dalje se podrzava:

```md
[[About Marvin]]
[[Call Marvin 1]]
```

Ali canonical parser ih prevodi u `links` format.

Preporuka:

- writer moze koristiti body `[[...]]`
- importer ih normalizira u `links`
- generated asset koristi samo `link.to`

Ako zelis explicitniji oblik u bodyju:

```md
@link about_marvin
@link call_marvin_1
```

### Pravilo

Ne koristiti istovremeno i `links` u frontmatteru i `[[...]]` u bodyju za isti edge, osim ako importer ima dedupe pravilo.

Preporuka:

- ili body `[[...]]`
- ili frontmatter `links`

ne oba.

## Condition DSL reference

### Simple leaf

```yaml
entry:
  has_item: item_metal_coin
```

### `all`

```yaml
entry:
  all:
    - completed: call_out_to_frank
    - has_item: item_metal_coin
```

### `any`

```yaml
entry:
  any:
    - has_item: item_metal_shard
    - has_item: item_wooden_stake
```

### `not`

```yaml
entry:
  not:
    completed: break_franks_leg
```

### Nested

```yaml
entry:
  all:
    - completed: pull_franks_leg
    - any:
        - has_item: item_metal_shard
        - has_item: item_wooden_stake
    - not:
        completed: let_frank_go
```

## Effect DSL reference

### Grant item

```yaml
mutations:
  - grant_item: item_military_lighter
```

### Play sound

```yaml
mutations:
  - play_sfx: stabbing_loop
```

### Mechanic intro

```yaml
mutations:
  - introduce_mechanic: inventory
```

### Transition

```yaml
mutations:
  - transition_room: hallway
```

### Multiple effects

```yaml
mutations:
  - play_sfx: creature_attack
  - set_flag:
      name: frank_state
      value: dead
  - unlock_node: inspect_franks_body
```

## Timed event spec

Timed event je regularni node s `type: timed_event`.

Primjer:

```md
---
id: approaching_footsteps_r1
type: timed_event
scope: room_local
room: first_room
aliases:
  - approaching footsteps
timers:
  - id: frank_window
    duration_seconds: 20
    on_start:
      - play_sfx: approaching_footsteps
    on_expire:
      - unlock_node: stay_silent
links:
  - to: stay_silent
  - to: call_out_to_frank
  - to: illuminate_piece_of_paper_with_franks_light
  - to: illuminate_coin_with_franks_light
  - to: break_down_door_on_frank
---

Slowly, they grow louder, but what's more interesting is the {{style:emphasis|light}}
seeping through the {{keyword:door_dent|door's dent}}.

They have a {{keyword:light_source|light source}}.
```

## Permanent interactions

Permanent interaction nije poseban file format.
To je isti node model sa `scope: permanent`.

Primjer:

```yaml
scope: permanent
```

Time parser i graph editor koriste isti data model za:

- room-local choiceve
- permanent interactions
- character topics
- system eventove

## Keyword catalog pravilo

Ako koristis `{{keyword:marvin|Marvin}}`, `marvin` mora postojati u keyword katalogu.

Minimalna pravila keyword kataloga:

- jedinstveni `id`
- display metadata
- opcionalni default style
- opcionalni default effects

Ako keyword ne postoji:

- parser prijavljuje validation error
- import ne bi trebao silently izmisljati keyword asset

## Naming rules

### Node ids

- `snake_case`
- stabilni
- bez spaceova

### Item ids

- prefiks `item_`

Primjer:

```text
item_metal_shard
item_military_lighter
```

### Flag names

- `snake_case`

### Room ids

- `snake_case`

## Legacy compatibility rules

Importer moze privremeno podrzavati legacy sintaksu:

- `"text_variations":`
- `"requirements":`
- `"event":`
- `**==SFX: ...==**`
- body `[[Link]]`
- `==word==`

Ali canonical output mora koristiti novu strukturu.

### Legacy mapping

#### `text_variations`

Legacy:

```text
"text_variations":
Who's Marvin?, Who is Marvin, Marvin?
```

Novo:

```yaml
aliases:
  - Who's Marvin?
  - Who is Marvin?
  - Marvin?
```

#### `requirements`

Legacy:

```text
"requirements": introduce_yourself1 or right_now_1
```

Novo:

```yaml
entry:
  any:
    - completed: introduce_yourself1
    - completed: right_now_1
```

#### `event`

Legacy:

```text
"event": Introduce thirst mechanic
```

Novo:

```yaml
mutations:
  - introduce_mechanic: thirst
```

#### inline `==word==`

Legacy:

```text
**==Marvin==**
```

Novo:

```text
{{keyword:marvin|Marvin}}
```

## Full examples

## Example 1: Marvin

```md
---
id: who_is_marvin
type: interaction
scope: permanent
feature: marvin
aliases:
  - Who's Marvin?
  - Who is Marvin?
  - Marvin?
  - Marvin
entry:
  any:
    - completed: introduce_yourself1
    - completed: right_now_1
links:
  - to: about_marvin
  - to: call_marvin_1
  - to: call_marvin_2
---

Cat. {{keyword:marvin|Marvin}}'s my cat. A lazy tabby.
{{keyword:marvin|Marvin}} cuddles with me when I'm down. So, like, every day...
```

## Example 2: How you got here

```md
---
id: how_you_got_here
type: interaction
scope: permanent
feature: memory
aliases:
  - How you got here?
  - How you got here
  - How did you get here?
  - How'd you get here?
mutations:
  - introduce_mechanic: thirst
  - unlock_keyword: water
---

@cue sequence:
  - sfx: inaudible_news_speaker
  - sfx: shambling_walk
  - sfx: doorbell_247
  - sfx: sirens_ramping_noise

I was watching the news, trying to inebriate myself to sleep, but I haven't left
the house in days and my stash was well and dry. I took the
{{item:house_keys|house keys}}, though, have I locked the {{keyword:door|door}}
on my way out?

Anyway... Went for more booze. Whiskey. Haven't had that in a long while.
Bounced off of things on my way to the 24/7. Got there. Took two bottles and a
six-pack, just in case.

And then I went home. No, wait. I... Took a walk? I think... Yes, I was going
down the alley and then... Sirens? I don't know, I was piss drunk.

Couldn't differentiate what's flashing outside or inside my head. And then...
Ahh, I can't remember. Now it's as dark inside my head as is outside of it.
I'm dehydrated. If I could get some {{keyword:water|water}} I'd sober up quicker.
I'll remember more.
```

## Example 3: Stab Frank's leg

```md
---
id: stab_franks_leg
type: interaction
scope: room_local
room: first_room
feature: frank
aliases:
  - Stab leg
  - Stab his leg
  - Stab their leg
entry:
  all:
    - completed: pull_franks_leg
    - any:
        - has_item: item_metal_shard
        - has_item: item_wooden_stake
mutations:
  - play_sfx: stabbing_loop
  - set_flag:
      name: frank_state
      value: maimed
---

@variant metal_shard
@entry:
  has_item: item_metal_shard
You take the sharp {{item:metal_shard|metal shard}} and begin to stab him in the
{{style:emphasis|leg}} repeatedly.

@variant wooden_stake
@entry:
  has_item: item_wooden_stake
You take the sharp {{item:wooden_stake|wooden stake}} and begin to stab him in
the {{style:emphasis|leg}} repeatedly.
```

## Validation rules

Importer mora prijaviti gresku ako:

- nedostaje `id`
- nedostaje `type`
- nedostaje `scope`
- `room_local` node nema `room`
- `links.to` ne pokazuje na postojeci node
- `keyword` token referencira nepostojeci keyword
- `entry` condition tree nije validan
- `mutations` sadrzi nepoznat effect
- `@variant` nema unique name
- file koristi i legacy i new syntax na konfliktan nacin

## Recommended migration phases

### Phase 1

- frontmatter `id`, `type`, `scope`, `aliases`, `entry`
- body `[[...]]`
- inline `{{keyword:...}}`

### Phase 2

- `mutations`
- `timers`
- `@variant`

### Phase 3

- puni keyword catalog
- graph validation panel
- Unity visual editor nad generated graphom

## Final recommendation

Za prvi implementation pass preporucujem ovaj minimalni authoring subset:

- YAML frontmatter
- `aliases`
- `entry`
- `mutations`
- body paragraphs
- `[[...]]` outgoing links
- `{{keyword:id|text}}` inline tokeni

To je dovoljno da:

- zamijenis trenutni raw string `requirements`
- strukturiras `event`
- rijesis keyword/object problem
- dobijes cist import u graph model

Bez toga da writerima odmah uvedes pretezak format.
