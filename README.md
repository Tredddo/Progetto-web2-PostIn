# PostIn — Portale Intranet per la Condivisione della Conoscenza Aziendale

Progetto finale per il corso di **Laboratorio di Programmazione Web II**.  
Applicazione web basata su tecnologia **Microsoft Blazor Server (.NET 9)** per la gestione e la condivisione di articoli, idee e conoscenze tra dipendenti in ambiente intranet aziendale.

---

## 1. Descrizione del Caso d'Uso

**PostIn** è una piattaforma intranet locale concepita per favorire la comunicazione interna, l'open innovation e la gestione della conoscenza (*Knowledge Management*) all'interno dell'organizzazione.

### Funzionalità principali della piattaforma
* **Gestione Articoli (Post)**: Creazione, pubblicazione e consultazione di contenuti formattati, identificati da un ID univoco, con titolo, corpo del testo, eventuale immagine di copertina, data/ora di pubblicazione e indicazione dell'autore.
* **Classificazione per Categorie (Hashtag)**: Organizzazione degli articoli tramite categorie tematiche. La relazione molti-a-molti consente di associare un articolo a più categorie e viceversa.
* **Profili Dipendente e Ruoli**: Gestione degli utenti aziendali (ID, Nome, Cognome, Username, Password Hash, Stato Account e Ruolo di Sistema: *Utente Base* o *Amministratore*).
* **Personalizzazione dei Contenuti**: Definizione delle categorie preferite da parte di ciascun dipendente per filtrare e personalizzare la propria dashboard di lettura.
* **Social Intranet & Network Tra Colleghi**: Possibilità di seguire altri utenti (iscrizioni follow) tracciando la data di inizio dell'interazione.
* **Lista di Lettura Privata ("Da leggere dopo")**: Salvataggio degli articoli di interesse con tracciamento della data di salvataggio.
* **Tracking delle Visualizzazioni**: Registrazione automatica di ciascuna apertura di un articolo da parte di un dipendente, con salvataggio di data e ora esatte.
* **Interazioni Social**: Rilascio di "Mi piace" con tracciamento temporale e pubblicazione di commenti ai singoli articoli.

---

## 2. Architettura del Sistema e Scelte Tecniche

Il sistema è stato realizzato utilizzando **.NET 9** e il framework **Blazor Server (Interactive Server Mode)**.

### Sintesi delle motivazioni architetturali
1. **Ambiente Aziendale Intranet**: L'infrastruttura di rete locale garantisce elevata velocità e bassa latenza. Blazor Server delega l'elaborazione al server, consentendo l'esecuzione immediata dell'applicazione anche su postazioni client con risorse limitate, evitando il download del runtime WebAssembly.
2. **Sicurezza e Riservatezza dei Dati**: L'assenza di API REST pubbliche esposte verso il client riduce la superficie di attacco. La logica di calcolo dei contenuti e il codice C# rimangono confinati sul server.
3. **Efficienza nel Calcolo e Rendering**: La logica di attinenza e di evidenziazione visiva dei post viene eseguita sul server a diretto contatto con la base dati tramite Entity Framework Core, inviando al browser soltanto le modifiche al DOM (diff).

Per una trattazione dettagliata delle scelte architetturali, consultare il documento:  
📄 **[Documentazione Scelta Blazor Server](docs/scelta-blazor-server.md)**

---

## 3. Modello dei Dati (Entity Framework Core)

La base dati dell'applicazione è stata progettata ed implementata tramite **Entity Framework Core**, definendo un modello relazionale strutturato che include 10 entità/tabelle di snodo e relative enumerazioni di stato e ruoli (`RuoloUtente`, `StatoAccountUtente`).

### Sintesi del modello relazionale:
* **Entità Principali**: `Dipendente`, `Articolo`, `Categoria`, `Visualizzazione`, `Commento`.
* **Tabelle di Snodo N:M**: `ArticoloCategoria`, `CategoriaPreferita`, `SalvataggioDaLeggere`, `Like`.
* **Relazione Riflessiva N:M**: `IscrizioneFollow` (gestione dei colleghi seguiti con data di inizio interazione).

Per l'analisi completa della struttura delle tabelle, attributi, chiavi primarie/composte e vincoli referenziali, consultare la documentazione dedicata:  
📄 **[Documentazione Modello dei Dati e Database](docs/modello-dati-database.md)**

---

## 4. Flussi Operativi e Ruoli Utente

### 4.1 Utente Base
* Autenticazione ed accesso alla piattaforma.
* Consultazione del feed articoli personalizzato in base alle categorie preferite ed agli autori seguiti.
* Creazione e pubblicazione di nuovi articoli con associazione a categorie ed eventuale immagine di copertina.
* Consultazione del dettaglio articolo con tracciamento automatico di data e ora di visualizzazione.
* Inserimento o rimozione di "Mi piace", salvataggio nella lista privata "Da leggere dopo" e pubblicazione di commenti.
* Gestione delle proprie iscrizioni (follow/unfollow nei confronti di altri colleghi).

### 4.2 Amministratore
* Tutte le funzionalità riservate all'Utente Base.
* Gestione del catalogo delle Categorie (inserimento, modifica, eliminazione).
* Moderazione dei contenuti (gestione/rimozione di articoli o commenti non idonei).
* Gestione degli account dei dipendenti (modifica ruolo e stato dell'account).

---

## 5. Linee Guida e Metodologia d'Uso dell'Intelligenza Artificiale (GitHub Copilot)

In conformità con le direttive del corso *Laboratorio di Programmazione Web II*, il progetto prevede l'integrazione trasparente e consapevole degli strumenti di supporto all'analisi e alla stesura del codice, nello specifico **GitHub Copilot**.

### 5.1 Principi di utilizzo definiti per lo sviluppo
* **Supporto allo Scaffolding**: Utilizzo dell'IA per la generazione automatizzata di codice ripetitivo (DTO, configurazioni standard di Entity Framework Core e form Razor).
* **Controllo Architetturale**: Responsabilità diretta dello sviluppatore nel verificare che il codice generato rispetti rigorosamente le chiavi composte, i vincoli d'integrità referenziale del database e i pattern di Blazor Server.
* **Validazione del Codice**: Ogni suggerimento fornito dall'IA viene sottoposto a revisione critica prima dell'integrazione, assicurando piena comprensione della logica implementata ai fini della valutazione d'esame.

---

## 6. Configurazione ed Esecuzione Locale

### Prerequisiti
* **.NET 9.0 SDK** installato sull'ambiente di sviluppo.

### Istruzioni per l'avvio

1. **Clonare il repository**:
   ```bash
   git clone https://github.com/Tredddo/Progetto-web2-PostIn.git
   cd Progetto-web2-PostIn/PostIn
   ```

2. **Ripristinare i pacchetti NuGet ed applicare le migrazioni**:
   ```bash
   dotnet restore
   dotnet ef database update
   ```

3. **Eseguire l'applicazione**:
   ```bash
   dotnet run
   ```

4. **Accesso all'applicazione**:
   Navigare da browser all'indirizzo HTTPS indicato nel terminale (es. `https://localhost:7150`).
