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
