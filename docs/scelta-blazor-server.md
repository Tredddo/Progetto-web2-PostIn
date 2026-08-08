# Motivazioni Architetturali: Scelta di Blazor Server

Il presente documento illustra le motivazioni tecniche e di business alla base della scelta di adottare **Blazor Server (Interactive Server Mode)** in .NET 9 per lo sviluppo della piattaforma intranet aziendale **PostIn**, preferendolo ad architetture basate su Blazor WebAssembly o Single Page Application (SPA) tradizionali con API REST.

---

## 1. Ambiente Aziendale Intranet e Prestazioni Client

In un contesto aziendale intranet:
- La rete locale (LAN/WLAN) offre una frequenza di banda elevata e latenza trascurabile (solitamente < 5ms).
- I dispositivi in uso ai dipendenti possono essere macchine con risorse hardware limitate o datate.

Adottando **Blazor Server**:
- L'elaborazione del codice C#, del rendering del DOM e delle query al database viene eseguita interamente sul server.
- Il client deve semplicemente scaricare un piccolo footprint JavaScript per la connessione WebSocket (SignalR) e renderizzare le differenze di DOM inviate dal server.
- Si evitano i tempi di caricamento iniziali elevati (*cold start*) legati al download del runtime .NET in WebAssembly e dei relativi assembly DLL nel browser.

---

## 2. Sicurezza dei Dati e Architettura Zero-API Visibili

La riservatezza dei dati aziendali e la protezione della logica di business sono requisiti primari:
- In un'applicazione SPA/WebAssembly, le API REST o GraphQL devono essere esposte pubblicamente o sulla rete intranet, aumentando la superficie di attacco e richiedendo meccanismi complessi di autenticazione e autorizzazione su ciascun endpoint.
- Inoltre, la logica di calcolo del frontend (ad esempio gli algoritmi di attinenza dei contenuti o le regole di business) è visibile ispezionando il codice sorgente o il bytecode scaricato dal browser.

Con **Blazor Server**:
- Non vengono esposte API REST pubbliche sul client per il recupero dei dati.
- Tutta la logica aziendale, compreso il calcolo dell'attinenza dei post, rimane confinata all'interno del processo server.
- Il browser riceve esclusivamente gli aggiornamenti dell'interfaccia utente (HTML diff), eliminando un'intera classe di vulnerabilità legate all'ispezione delle chiamate di rete e all'esposizione indebita di endpoint backend.

---

## 3. Calcoli Complessi lato Server e Rendering Efficiente

L'applicazione include funzionalità di personalizzazione visiva e di rilevanza dei contenuti per ogni dipendente (ad esempio la determinazione del livello di attinenza o l'applicazione di bordi e indicatori visivi colorati basati su categorie preferite e colleghi seguiti):

- In Blazor Server, queste operazioni vengono eseguite direttamente sulla macchina server a stretto contatto con il database tramite Entity Framework Core.
- Il server calcola il diff dell'interfaccia utente e trasmette al client esclusivamente le istruzioni minime per l'aggiornamento visivo (il delta del DOM).
- Questo approccio garantisce un'esperienza d'uso estremamente reattiva e priva di sovraccarichi computazionali sulla macchina dell'utente finale.
