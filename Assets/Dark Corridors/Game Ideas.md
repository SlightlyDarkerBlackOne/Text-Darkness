# Game Ideas

## Purpose

Ovaj dokument sluzi kao mjesto za spremanje gameplay, narrative i UX ideja koje zelimo kasnije razraditi.

---

## Idea 001 - Track unavailable player choices

### Summary

Biljeziti koje free-text opcije igraci pokusavaju upisati u odredenom dijelu igre, ali trenutno ne postoje kao validan choice.

Primjer:

- igrac upise `find electric box`
- trenutni scene/context nema takav choice
- unos se ipak spremi u history radi kasnije analize

### Why this is useful

- pokazuje sto igraci prirodno ocekuju da mogu probati
- otkriva missing interactions i nejasne scene
- pomaze kod prioritizacije novih choiceva
- daje signal gdje parser, synonym support ili content coverage nisu dovoljno dobri

### What to store

Za svaki unavailable input spremiti:

- raw player input
- normalized input
- scene / room / node identifier
- active game state ili context identifier
- in-game timestamp
- real timestamp
- session id / save id
- optional player language

### Possible storage

Opcije:

- local JSON log file
- lightweight local database
- batched analytics event queue

Za pocetak je najjednostavnije krenuti s `JSON` history zapisom, pa kasnije po potrebi prebaciti na strukturiraniji storage.

### Example record

```json
{
  "sessionId": "run_2026_03_20_001",
  "sceneId": "first_room",
  "nodeId": "air_duct_cover",
  "rawInput": "find electric box",
  "normalizedInput": "find electric box",
  "gameTimestamp": "00:12:43",
  "realTimestampUtc": "2026-03-20T18:42:11Z",
  "wasAvailable": false
}
```

### How we could use it later

- grupirati najcesce nepostojece pokusaje po roomu
- otkriti sinonime koje igraci cesto koriste
- dodati hidden ili contextual choiceve tamo gdje postoji jak pattern
- poboljsati feedback kad choice nije dostupan

### Open questions

- zapisivati samo potpuno nepostojeci input ili i "blizu pogodene" choiceve
- koliko dugo cuvati history
- treba li log biti per save, per session ili globalno
- treba li ovakve inpute prikazivati i u internom debug viewu

---

## Idea 002 - Hover na emphasis rijeci otvara prozor s choicevima

### Summary

Na hover preko emphasis rijeci u narativnom tekstu (npr. `**==keyword==**` ili ekvivalent u runtimeu) otvara se mali prozor (tooltip / popover) koji prikazuje relevantne choiceve ili akcije vezane uz tu rijec ili kontekst.

Klik na jedan od choiceva u tom prozoru triggera isti choice kao da je igrac unio taj tekst ili odabrao tu opciju u glavnom inputu.

### Why this is useful

- smanjuje trial-and-error kod free-text ili sirokog skupa opcija
- naglasene rijeci postaju discoverable entry pointi u interakciju
- brzi pristup bez pisanja cijelog stringa
- moze pomoci novim igracima da vide sto je uopce "na stolu" u tom trenutku

### Behaviour notes

- hover delay ili explicit toggle da se ne otvara prozor slucajno pri skeniranju teksta
- lista u prozoru filtrirana po trenutnom stanju (requirements, unlocks)
- opcionalno: sort po vjerojatnosti / frekvenciji ili po kategoriji
- klik izvan prozora zatvara bez akcije

### Open questions

- treba li hover biti dostupan na svim emphasis rijecima ili samo na onima koje imaju mapiranje na choiceve
- koliko choiceva maksimalno u jednom hoveru prije nego postane pretrpano
- treba li isti mehanizam raditi i na touchu (long-press umjesto hovera)
- odnos prema Idea 001: ako igrac i dalje upise nesto rucno, i dalje logirati unavailable pokusaje

---

## Idea 003 - Speaker boje i prosireni dialogue blockovi

### Summary

`@say <speaker_id>` blockovi u DSL-u mogu sluziti ne samo za oznaku tko prica, nego i kao presentation hook za boju teksta, portrait, voice styling ili druge UI efekte vezane uz govornika.

Osnovna ideja:

- `@say player` = jedna boja i stil
- `@say stranger` = druga boja i stil
- `@say creature` = treca boja, glitch ili drugi efekt

Tako renderer ne mora pogadati iz samog teksta tko govori, nego dobije stabilni `speaker_id`.

### Why this is useful

- dijalog je citljiviji na prvi pogled
- igrac lakse prati tko govori u scenama bez vizualnog kontakta
- isti speaker sustav se kasnije moze spojiti na portrait, audio ili subtitle styling
- dobro pase uz free-text i narativni format gdje nema klasicnog dialogue UI-a cijelo vrijeme

### Base format

Najcisti start:

```md
@say player
So it's worth a lot?

@say stranger
Oh, it's worth much more than that.
```

### Optional extended forms

Ako kasnije zatreba vise kontrole, mozemo podrzati i prosirene oblike:

```md
@say stranger angry
...
```

ili:

```md
@say stranger
@tone angry
...
```

ili cak strukturiraniji oblik:

```md
@say:
  speaker: stranger
  tone: angry
  color_key: hostile_npc
...
```

### Possible runtime usage

- `speaker_id -> color preset`
- `speaker_id -> portrait / avatar`
- `speaker_id -> voice / filter`
- `speaker_id + tone -> override boje ili animacije`

### Open questions

- je li dovoljno da boja bude vezana samo na `speaker_id`
- treba li `tone` biti poseban koncept ili samo presentation override
- treba li unknown speaker uvijek poceti kao `stranger`, pa se kasnije remapira kad se identitet otkrije
- koliko daleko ici sa syntax sugarom prije nego DSL postane pretezak za pisanje
