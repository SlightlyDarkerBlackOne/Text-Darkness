# Events Calendar Feature Plan

## Current State

Events Calendar je trenutno MVP koji moze prikazati landing page, otvoriti calendar importer, traziti evente preko Ticketmastera ili LLM providera i poslati rezultate u Google Calendar REST API.

Glavni limit je to sto app jos nema production OAuth flow, sigurno spremanje kljuceva, odvojenu scenu, mock podatke za testiranje bez API-ja i pravi subscription/payment backend.

## Proposed Features

### 1. Authentication And Account Setup

- Google OAuth login za dobivanje i refreshanje calendar access tokena.
- Secure local token storage umjesto rucnog lijepljenja access tokena u Inspector.
- User profile screen s trenutno povezanim Google accountom.
- Logout i revoke access akcije.

### 2. Event Discovery

- Ticketmaster search po lokaciji, radiusu, datumu, zanru i keywordu.
- Multiple music style filteri s jasnim summaryjem odabira.
- LLM enrichment samo kao opcionalni enrichment sloj, ne kao jedini izvor istine.
- Favorite venues i favorite artists.
- Saved searches za ceste kombinacije filtera.

### 3. Calendar Import Flow

- Preview rezultata prije importa.
- Manual select/deselect eventa prije slanja u Google Calendar.
- Duplicate detection po external id-u, titleu, venueu i start timeu.
- Import summary s listom uspjesnih i neuspjesnih eventa.
- Retry za neuspjele importe.

Current implementation:

- Search prvo puni preview listu rezultata.
- Korisnik moze odabrati ili maknuti pojedine evente prije importa.
- Import selected flow oznacava imported, failed i duplicate statuse.
- Retry failed button vraca samo neuspjele evente u preview za novi pokusaj.
- Duplicate detection je trenutno lokalna za aktivnu sesiju; za production treba provjera protiv stvarnog Google Calendar statea.

### 4. Subscription And Monetization

- Pravi payment provider, npr. Stripe.
- Subscription state iz backend servisa, ne iz Unity UI-ja.
- Feature gating za premium mogucnosti kao auto-sync, saved searches i LLM enrichment.
- Grace period i expired subscription handling.

### 5. UX And UI

- Odvojena Events Calendar scena ili entry point da ne prekriva Dark Corridors UI.
- Responsive 16:9 full screen layout za desktop.
- Search result cards s naslovom, venueom, datumom, cijenom i Google Maps linkom.
- Empty states za nema rezultata, missing API key, missing Google account i API error.
- Loading indikator tijekom search/import flowa.

### 6. Reliability

- Centralizirani config za API endpointove i feature flags.
- Rate limit handling za Ticketmaster, LLM i Google Calendar API.
- Network timeouti i user-friendly error messages.
- Local cache za zadnje rezultate.
- Structured logging za import attemptove.

### 7. Testing

- Edit Mode testovi za filtere, mapping i request modele.
- Play Mode testovi za UI flow bez pravih API-ja.
- Mock search provider i mock calendar importer.
- Contract testovi za provider response mapping.
- Manual QA checklist za OAuth, search, preview, import i duplicate handling.

## What We Need For A Proper Working App

### API And Services

- Ticketmaster developer API key.
- Google Cloud project s enabled Google Calendar API.
- OAuth consent screen i desktop/client credentials.
- Backend za subscription status, ako monetizacija ide u production.
- Optional OpenAI-compatible API key za enrichment.

### Unity Work

- Odvojena scene setup za Events Calendar.
- Serialized config asset za non-secret settings.
- Runtime service composition umjesto unosa kljuceva direktno na scene controller.
- UI prefabs ili UI Toolkit layout ako UI naraste iznad trenutnog MVP-a.
- Mock mode za testiranje bez interneta.

### Product Decisions

- Koje drzave/gradove prvo podrzavamo.
- Je li Ticketmaster glavni izvor ili samo jedan od providera.
- Koji featurei su free, a koji su premium.
- Koliko dugo cuvamo cache i import history.
- Treba li app biti dio igre ili zaseban tool/scene.

## Suggested Next Milestones

1. Napraviti mock mode i preview results UI.
2. Dodati Google OAuth flow i sigurno spremanje tokena.
3. Dodati duplicate detection prije importa.
4. Premjestiti Events Calendar u zasebnu scenu.
5. Dodati Play Mode testove za glavni UI flow.
