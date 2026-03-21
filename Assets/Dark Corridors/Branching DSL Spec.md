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
- `prompt`
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
- `prompt_active`
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

```yaml
entry:
  all:
    - completed: pull_at_door_dent
    - prompt_active: pull_at_door_dent_confirm
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

### `prompt`

`prompt` oznacava da je node odgovor unutar privremenog prompt konteksta, npr. modalni `yes/no` izbor.

Primjer:

```yaml
prompt:
  id: pull_at_door_dent_confirm
  role: no
```

Podrzani fieldovi:

- `id`
- `role`

Tipicni `role` primjeri:

- `yes`
- `no`
- custom vrijednosti poput `left`, `right`, `accept`, `refuse`, `literal`, `metaphoric`

`open_prompt.mode` preporucene vrijednosti:

- `exclusive_global` za prompt koji privremeno zakljuca sve ostale choiceve
- `local` za prompt koji otvara follow-up opcije bez globalnog blocka

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
- `open_prompt`
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

Uklanjanje itema iz inventoryja na tocnom trenutku reveal-a:

```yaml
mutations:
  - remove_item:
      item: item_metal_coin
      trigger: on_reveal
      token: item_metal_coin
```

Stat mutation vezan uz odredeni inline token:

```yaml
mutations:
  - sanity:
      amount: 15
      trigger: on_reveal
      token: sanity
```

Stat mutation koji postavlja apsolutnu ciljnu vrijednost:

```yaml
mutations:
  - sanity:
      value: 20
      trigger: on_reveal
      token: sanity_drop
  - sanity:
      value: 70
      trigger: on_reveal
      token: sanity_recover
```

```yaml
mutations:
  - set_flag:
      name: frank_state
      value: hostile
```

Otvaranje ekskluzivnog prompta:

```yaml
mutations:
  - open_prompt:
      id: pull_at_door_dent_confirm
      mode: exclusive_global
      lock_other_choices: true
```

Otvaranje lokalnog prompta koji ne blokira sve ostale choiceve:

```yaml
mutations:
  - open_prompt:
      id: zodiac_sign_confirm
      mode: local
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
- dialogue block
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

## Dialogue block

Za dijalog koristi se `@say` block sa speaker id-em.

Sintaksa:

```md
@say player
So it's worth a lot?

@say stranger
Oh, it's worth much more than that.
```

Pravila:

- speaker id je obavezan
- speaker id treba biti stabilan i u `snake_case` ili jednostavnom lowercase obliku
- renderer moze mapirati speaker id na boju, portrait, voice ili drugi presentation layer
- ako govornik nije poznat, koristi privremeni id poput `stranger`, `unknown_voice`, `creature`

Primjer s naracijom izmedu linija:

```md
@say stranger
Ha-haha-HA! I never thought I'd get one as easily as this.

@say player
So it's worth a lot?

@cue sfx: steps_walking_away

Ignoring your threats, the person leaves.
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

Item s emphasisom, prikazana rijec (on-screen word) kao zadnji segment nakon `|`:

```text
{{item:item_rusty_metal_door_shard|style=emphasis|door}}
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

Item token koji oznacava trenutak kad item izlazi iz inventoryja:

```text
{{item:item_metal_coin|style=emphasis|coin}}
```

Status token koji oznacava tocan trenutak kad se mutation okida:

```text
{{status:sanity|style=emphasis|control}}
```

Reference token za runtime-injected vrijednosti poput imena igraca:

```text
{{ref:player_name|PlayerName}}
```

### Pravila za tokene

- `type` i `id` su obavezni
- `display text` je obavezan
- kad postoje dodatni dijelovi odvojeni s `|`, prikazana rijec moze biti zadnji segment (npr. `{{item:...|style=emphasis|door}}`) da je eksplicitno sto se ispisuje u tekstu
- attributes su opcionalni
- `id` tokena moze sluziti kao anchor za mutation trigger, npr. `token: sanity` + `trigger: on_reveal`
- `ref` token sluzi za runtime reference; engine treba pokusati resolveati vrijednost po `id`, a ako ne uspije koristiti `display text` kao fallback
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

Primjer kad se room promijeni odmah, a content target novog rooma jos nije implementiran:

```md
---
id: air_duct_left_confirmed
type: interaction
scope: room_local
room: first_room
feature: air_duct_cover
aliases:
  - Keep going
entry:
  completed: air_duct_left
mutations:
  - transition_room: lower_room
---

It doesn't. The segment you're on collapses. Screaming, you tumble further down into the darkness.

[[Lower room]]
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

## Example 4: Prompt-based yes/no choice

Trigger node:

```md
---
id: pull_at_door_dent
type: interaction
scope: room_local
room: first_room
feature: metal_door
aliases:
  - Pull at door dent
entry:
  completed: inspect_dent_in_door
mutations:
  - open_prompt:
      id: pull_at_door_dent_confirm
      mode: exclusive_global
      lock_other_choices: true
---

It seems to be moving, but I'll really have to strain myself to pry it open.
Should I pull harder?

[[Pull harder door dent]]
[[Leave door dent]]
```

No response:

```md
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
entry:
  all:
    - completed: pull_at_door_dent
    - prompt_active: pull_at_door_dent_confirm
---

You let go of the door.
```

Yes response:

```md
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
entry:
  all:
    - completed: pull_at_door_dent
    - prompt_active: pull_at_door_dent_confirm
---

You pull harder.
```

Drugi primjer za binary follow-up prompt:

```md
---
id: metaphoric_hell
type: interaction
scope: permanent
feature: is_this_hell
entry:
  completed: is_this_hell_1
mutations:
  - open_prompt:
      id: metaphoric_hell_confirm
      mode: exclusive_global
      lock_other_choices: true
---

Most definitely. I doubt hell exists.

[[Hell exists]]
[[Hell doesn't exist]]
```

Prompt moze otvoriti dodatni prompt:

```md
---
id: is_this_hell_1
type: interaction
scope: permanent
feature: is_this_hell
mutations:
  - open_prompt:
      id: is_this_hell_1_confirm
      mode: exclusive_global
      lock_other_choices: true
---

Depends how you view it. Like literal or metaphoric hell?

[[Literal Hell]]
[[Metaphoric hell]]
```

```md
---
id: metaphoric_hell
type: interaction
scope: permanent
feature: is_this_hell
prompt:
  id: is_this_hell_1_confirm
  role: metaphoric
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

Most definitely. I doubt hell exists.

[[Hell exists]]
[[Hell doesn't exist]]
```

Ovo znaci da response node iz prvog prompta moze odmah otvoriti novi, ugnijezdeni follow-up prompt.

Primjer lokalnog prompta bez globalnog blocka:

```md
---
id: zodiac_sign
type: interaction
scope: permanent
feature: zodiac_sign
mutations:
  - open_prompt:
      id: zodiac_sign_confirm
      mode: local
---

Who gives a shit? Gemini.

[[Zodiac no]]
[[Zodiac yes]]
```

Prompt response moze i dalje imati vlastite `@variant` grane:

```md
---
id: right_now_1
type: interaction
scope: permanent
feature: permanent_interactions_are_you_alone
prompt:
  id: are_you_alone_confirm
  role: right_now
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

## Recommendations

- koristi `transition_room` za stvarnu gameplay/state promjenu sobe
- koristi `[[Room Name]]` ili drugi content link za narrative target kad postoji ili ce postojati poseban node
- ako room state treba promijeniti odmah, `transition_room` ne treba cekati da ciljni node vec postoji
- ako i `transition_room` i `[[...]]` postoje zajedno, tretiraj ih kao dva odvojena sloja: state change i content continuation
- za buduce room entry nodeove preporuka je da `room id` i buduci target node budu imenovani sto konzistentnije, npr. `lower_room`