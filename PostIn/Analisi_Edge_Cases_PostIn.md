# Documento Tecnico: Analisi degli Edge Cases per la Piattaforma Intranet "PostIn"

**Progetto Web:** PostIn - Piattaforma Intranet di Sharing & Knowledge  
**Tecnologie di Riferimento:** .NET C#, Blazor, Entity Framework Core, SQLite, Radzen UI  
**Destinatari:** Team di Sviluppo, Commissione d'Esame / Revisione Codice  

---

## 1. Introduzione e Obiettivo

Il presente documento stabilisce la mappatura professionale ed esaustiva di tutti gli **Edge Cases** (casi limite, condizioni al contorno e anomalie operative) da gestire nello sviluppo della piattaforma intranet aziendale **PostIn**.

In un’applicazione web Single Page Application (SPA) basata su **Blazor** ed **EF Core**, la gestione corretta degli edge cases garantisce tre pilastri fondamentali:
1. **Integrità dei Dati e del Database**: Prevenzione di orfani, violazioni di vincoli relazionali, duplicati e stati inconsistenti.
2. **Sicurezza e Controllo degli Accessi**: Garanzia che ogni operazione sia autorizzata lato server e non solo nascosta nell'interfaccia grafica.
3. **Robustezza dell'Esperienza Utente (UX)**: Gestione fluida di caricamenti, errori di rete, formattazione dati e input imprevisti senza crash dell'applicazione.

---

## 2. Analisi degli Edge Cases per Sezione Operativa e Dominio

---

### 2.1 Gestione Dipendenti, Profilo e Autenticazione

La sezione gestisce l'anagrafica dei dipendenti, il collegamento 1:1 con **ASP.NET Core Identity** (`IdentityUserId`), i ruoli di sistema (`Utente Base` / `Amministratore`) e le relazioni di iscrizione tra colleghi.

#### Edge Cases da Gestire:

1. **Desincronizzazione Identity vs Profilo Dominio (`Dipendente`)**:
   - *Caso*: Un utente viene creato in `AspNetUsers` ma la creazione del relativo record `Dipendente` fallisce (o viceversa).
   - *Soluzione*: Racchiudere la creazione utente e profilazione in una **transazione di database** (`IDbContextTransaction`).

2. **Username / Email Duplicati o Spazi Imprevisti**:
   - *Caso*: L'utente inserisce uno username con spazi iniziali/finali o con maiuscole/minuscole diverse rispetto a uno già esistente (es. `mario.rossi` vs `Mario.Rossi`).
   - *Soluzione*: Applicare `.Trim()` e `.ToLower()` in fase di validazione. Impostare l'indice `UNIQUE` sul campo `Username` e `Email` in EF Core tramite `OnModelCreating`.

3. **Modifica del Ruolo Utente Durante una Sessione Attiva**:
   - *Caso*: Un Admin declassa un dipendente da `Amministratore` a `Utente Base` mentre quest'ultimo è connesso e sta lavorando su un pannello di gestione.
   - *Soluzione*: Verificare i ruoli non solo all'inizializzazione del componente ma re-validare le autorizzazioni lato server ad ogni operazione di scrittura (CRUD).

4. **Self-Following (Iscrizione a Sé Stessi)**:
   - *Caso*: Un dipendente tenta di "seguire" sé stesso per alterare le metriche o la logica di feed.
   - *Soluzione*: Validare nei metodi di servizio che `FollowerId != FollowedId`. Disabilitare o nascondere il pulsante "Segui" sul proprio profilo UI.

5. **Doppia Iscrizione / Race Condition (Click Multiplo su "Segui")**:
   - *Caso*: L'utente clicca ripetutamente sul tasto "Segui" prima del completamento della richiesta asincrona.
   - *Soluzione*: Disabilitare il pulsante durante l'esecuzione (`isProcessing = true`). Definire la chiave primaria composta o un indice unico `UNIQUE(FollowerId, FollowedId)` nella tabella `Iscrizioni`.

6. **Iscrizione verso Utente Eliminato o Disattivato**:
   - *Caso*: L'utente A tenta di seguire l'utente B proprio mentre l'utente B viene rimosso dal sistema.
   - *Soluzione*: Intercettare l'assenza del record nel DB tramite `DbUpdateException` o controllo d'esistenza preventivo, mostrando un avviso `RadzenNotification`.

---

### 2.2 Gestione Articoli (Post) e Contenuti Media

Gli articoli costituiscono il nucleo informativo del sistema, identificati da codice univoco, titolo, corpo, data/ora di pubblicazione, autore e immagine di copertina facoltativa.

#### Edge Cases da Gestire:

1. **Titolo o Corpo Composti Solo da Spazi Vuoti**:
   - *Caso*: L'autore invia un post con titolo o testo contenente unicamente spazi o a capo (`"   "`).
   - *Soluzione*: Usare attributi DataAnnotations custom o controlli stringa con `string.IsNullOrWhiteSpace()`.

2. **Superamento della Lunghezza Massima dei Testi**:
   - *Caso*: Inserimento di articoli estremamente lunghi che saturano il DB o compromettono il layout grafico.
   - *Soluzione*: Validazione con `[StringLength(..., MinimumLength = ...)]` sui ViewModel e limiti precisi nei campi SQL (`HasMaxLength`).

3. **Caricamento Immagine di Copertina: Formati Non Validi o File Malevoli**:
   - *Caso*: L'utente carica un file `.exe`, `.bat` o un PDF rinominato `.png`.
   - *Soluzione*: Validare il MIME type e l'estensione del file lato C#. Consentire solo `.jpg`, `.jpeg`, `.png`, `.webp`.

4. **Immagine di Copertina di Dimensioni Eccessive**:
   - *Caso*: Caricamento di un'immagine da 50MB che manda in crash il buffer SignalR / Blazor o satura la memoria del server.
   - *Soluzione*: Impostare un limite massimo di dimensione (es. 5MB) su `InputFile` o sui componenti Radzen Upload.

5. **Attacchi Path Traversal nel Nome del File Immagine**:
   - *Caso*: L'utente carica un file denominato `../../wwwroot/js/malicious.js`.
   - *Soluzione*: Rigenerare sempre il nome del file sul server tramite un GUID unico (es. `Guid.NewGuid().ToString() + extension`), ignorando il nome originale fornito dal client.

6. **Immagine Eliminata dal File System ma Presente nel DB**:
   - *Caso*: Il file fisico in `wwwroot/uploads` viene rimosso o rinominato manualmente, ma la tabella `Articoli` contiene ancora il percorso relativo.
   - *Soluzione*: Implementare la verifica di esistenza del file prima del rendering oppure impostare un fallback grafico (immagine placeholder predefinita `default-cover.png`).

7. **Cancellazione dell'Autore dell'Articolo (`DeleteBehavior.Restrict`)**:
   - *Caso*: Tentativo di cancellazione di un dipendente che ha pubblicato diversi articoli.
   - *Soluzione*: Con `DeleteBehavior.Restrict`, la cancellazione diretta lancia un'eccezione SQL. Occorre bloccare la cancellazione via UI spiegando che l'utente ha articoli attivi, oppure disattivare il profilo (Soft Delete / `IsActive = false`) mantenendo lo storico degli articoli.

---

### 2.3 Gestione Categorie Tematiche (Hashtag) e Preferiti

Le categorie permettono l'organizzazione tematica N:N degli articoli e la personalizzazione dell'esperienza utente tramite le categorie preferite.

#### Edge Cases da Gestire:

1. **Categorie Duplicate con Diversa Formattazione (Case-Sensitivity)**:
   - *Caso*: Creazione delle categorie `"Tech"`, `"tech"` e `"TECH"`.
   - *Soluzione*: Normalizzazione in minuscolo/maiuscolo (es. slugification o `.ToLower()`) e vincolo `UNIQUE` nel database sul campo `NomeCategoria`.

2. **Inserimento di Categorie con Caratteri Speciali o Spazi Illegali**:
   - *Caso*: Creazione di hashtag del tipo `#te@ch!` o `te ch`.
   - *Soluzione*: Validazione mediante Regular Expressions (Regex) per ammettere solo caratteri alfanumerici e trattini.

3. **Articolo Senza Categorie o con Troppe Categorie**:
   - *Caso*: Un articolo viene pubblicato senza alcuna categoria (impossibile da filtrare) o associato a 50 categorie.
   - *Soluzione*: Imporre nel form di creazione una regola di validazione: minimo 1 categoria, massimo N categorie (es. max 5).

4. **Eliminazione di una Categoria Collegata ad Articoli**:
   - *Caso*: L'amministratore elimina una categoria attualmente associata a 100 articoli.
   - *Soluzione*: Intercettare il vincolo relazionale. Chiedere conferma all'amministratore rimuovendo preventivamente le relazioni dalla tabella ponte `ArticoloCategoria`, senza eliminare gli articoli fisici.

5. **Rimozione Completa delle Categorie Preferite dal Profilo**:
   - *Caso*: Il dipendente deseleziona tutte le sue categorie preferite.
   - *Soluzione*: Gestire correttamente la vista Home/Dashboard mostrando un feed generico (es. "Tutti gli articoli recenti") anziché una schermata vuota o un errore di `NullReferenceException`.

---

### 2.4 Interazioni Sociali (Mi Piace, Commenti, Salvataggi)

I dipendenti interagiscono con gli articoli tramite Mi Piace, commenti e la lista privata "Da leggere dopo".

#### Edge Cases da Gestire:

1. **Toggle Rapido "Mi Piace" (Race Condition Asincrona)**:
   - *Caso*: L'utente clicca ripetutamente in rapida successione su "Mi Piace" e "Rimuovi Mi Piace". Le chiamate asincrone sul server potrebbero completarsi fuori ordine.
   - *Soluzione*: Implementare il debouncing o disabilitare l'interazione fino al completamento dell'operazione asincrona corrente. Garantire un indice `UNIQUE(DipendenteId, ArticoloId)` nella tabella `MiPiace`.

2. **Commento Vuoto o Solo Spazi/A Capo**:
   - *Caso*: L'utente invia un commento privo di testo reale.
   - *Soluzione*: Validazione `[Required]` e `.Trim()` sul campo di testo prima dell'invio.

3. **Articolo Eliminato Mentre un Utente Sta Scrivendo un Commento**:
   - *Caso*: L'utente legge un articolo, scrive un lungo commento e clicca "Invia", ma l'articolo è stato appena eliminato da un Admin.
   - *Soluzione*: Gestire l'eccezione di Foreign Key Mancante (`DbUpdateException`). Catturare l'errore e mostrare un messaggio chiaro: *"Impossibile salvare il commento: l'articolo non è più disponibile"*.

4. **Salvataggio Duplicato in "Da Leggere Dopo"**:
   - *Caso*: Tentativo di aggiungere un articolo già presente nella lista salvati.
   - *Soluzione*: Usare il pattern `FirstOrDefaultAsync` per verificare la presenza oppure un indice unico nel DB. Rendere il pulsante dinamico ("Salva" vs "Rimuovi dai Salvati").

5. **Articolo Salvato Successivamente Eliminato**:
   - *Caso*: Nella pagina "Da leggere dopo", l'elenco fa riferimento a un articolo che è stato eliminato.
   - *Soluzione*: Se la relazione non è a cascata, usare filtri LINQ `Where(s => s.Articolo != null)` ed eventualmente pulire i salvataggi orfani durante la lettura.

---

### 2.5 Registro delle Visualizzazioni (Article Views Tracking)

Ogni apertura di un articolo deve registrare la data e l'orario esatto di visualizzazione.

#### Edge Cases da Gestire:

1. **Spam di Visualizzazioni da Re-rendering Blazor**:
   - *Caso*: In Blazor, se il tracciamento della visualizzazione è inserito nel metodo del ciclo di vita errato (es. `OnParametersSet` o ad ogni re-rendering del componente), una singola lettura può registrare decine di record nel DB.
   - *Soluzione*: Eseguire la registrazione **solo una volta per sessione/caricamento pagina**, usando una guardia booleana (`hasTrackedView`) o memorizzando l'evento all'interno di `OnInitializedAsync` / `firstRender` in `OnAfterRenderAsync`.

2. **Visualizzazioni Consecutive dello Stesso Utente (Throttle / Debounce)**:
   - *Caso*: L'utente fa refresh continuo della pagina dell'articolo per falsare il contatore letture.
   - *Soluzione*: Applicare un filtro temporale: registrare la visualizzazione solo se sono trascorsi almeno X minuti (es. 5 minuti) dall'ultima visualizzazione dello stesso articolo da parte dello stesso dipendente.

---

## 3. Edge Cases Tecnologici (Architettura Blazor, EF Core e UI)

---

### 3.1 Ciclo di Vita Blazor e Rendering

1. **Esecuzione Doppia con Pre-rendering (Blazor Auto / Server)**:
   - *Caso*: `OnInitializedAsync` viene eseguito due volte se il pre-rendering è attivo.
   - *Soluzione*: Fare attenzione alle operazioni di scrittura su DB nel metodo di inizializzazione per evitare inserimenti duplicati.

2. **Stati Espliciti della UI (Caricamento, Errore, Vuoto)**:
   - *Caso*: Invocazione asincrona lenta dal DB mentre l'interfaccia visualizza elementi `null` producendo `NullReferenceException`.
   - *Soluzione*: Gestire sempre la triade di stato visivo:
     - `if (isLoading) { <RadzenProgressBar /> }`
     - `else if (hasError) { <AlertError /> }`
     - `else if (!items.Any()) { <EmptyState /> }`

---

### 3.2 Persistenza Dati e Database (EF Core / SQLite)

1. **Violazione dei Vincoli Foreign Key (`DeleteBehavior.Restrict`)**:
   - *Caso*: Cancellazione di un'entità padre che causa eccezioni di sistema non gestite.
   - *Soluzione*: Wrappare ogni chiamata `SaveChangesAsync()` a rischio all'interno di blocchi `try-catch (DbUpdateException ex)` e mostrare notifiche utente amichevoli.

2. **Data Seeding Idempotente (`DbSeeder`)**:
   - *Caso*: Il seeder iniziale viene invocato all'avvio dell'applicazione ed inserisce duplicati ogni volta che il server si riavvia.
   - *Soluzione*: Verificare l'esistenza dei dati con `if (!context.Articoli.Any())` prima di aggiungere record di prova.

---

### 3.3 Gestione File e JS Interop

1. **Perdita di Memoria su Trasferimento File Base64**:
   - *Caso*: La conversione di grandi file in Base64 per la trasmissione tramite JS Interop esaurisce la memoria allocata al circuito SignalR.
   - *Soluzione*: Utilizzare lo stream diretto fornito dal componente `InputFile` (`OpenReadStream(maxAllowedSize)`).

2. **Percorsi File Assoluti vs Relativi (`wwwroot`)**:
   - *Caso*: Salvataggio del percorso assoluto del disco (es. `C:\Users\...` o `/home/...`) nel DB anziché del percorso relativo web (`/uploads/cover.jpg`).
   - *Soluzione*: Memorizzare esclusivamente percorsi relativi utilizzabili dai tag HTML `<img>`.

---

### 3.4 Componenti Grafici Radzen UI

1. **Filtraggio e Ordinamento su Campi Nullable in `RadzenDataGrid`**:
   - *Caso*: Ordinamento di una colonna che contiene valori `null` (es. data di modifica o copertina).
   - *Soluzione*: Gestire i campi `Nullable<T>` nelle espressioni di binding e configurare fallback grafici o proprietà calcolate.

2. **Fusi Orari e Formattazione Date**:
   - *Caso*: Inserimento date con orari disallineati tra client e server SQLite (UTC vs Local Time).
   - *Soluzione*: Utilizzare `DateTime.UtcNow` per la memorizzazione su DB e formattare in ora locale (`ToLocalTime()`) durante la visualizzazione UI.

---

## 4. Matrice Riassuntiva degli Edge Cases

| ID | Sezione / Ambito | Descrizione Edge Case | Impatto / Rischio | Soluzione Tecnico-Architetturale |
|---|---|---|---|---|
| **EC-01** | Utenti & Identity | Mancata corrispondenza tra `AspNetUsers` e `Dipendenti`. | Dati orfani, crash login. | Transazione DB atomica (`IDbContextTransaction`). |
| **EC-02** | Iscrizioni | Utente prova a seguire sé stesso (Self-follow). | Inconsistenza logica. | Controllo `FollowerId != FollowedId` + UI Guard. |
| **EC-03** | Iscrizioni / Like | Doppio click rapido su tasti azione (Race Condition). | Registrazioni duplicate / DB Exception. | Disabilitazione bottone + Vincolo `UNIQUE` su DB. |
| **EC-04** | Articoli / Media | Upload file non immagine o con estensione camuffata. | Vulnerabilità sicurezza. | Controllo MIME type + whitelist estensioni `.jpg,.png,.webp`. |
| **EC-05** | Articoli / Media | Nome file caricato con attacco Path Traversal (`../`). | Sovrascrittura file di sistema. | Generazione nome file univoco lato server tramite `Guid`. |
| **EC-06** | Articoli / Modello | Titolo o corpo del post vuoti o di soli spazi. | Dati spazzatura nel DB. | Attributes `[Required]`, `.Trim()` e `[StringLength]`. |
| **EC-07** | Articoli / Eliminazione | Eliminazione dipendente con articoli pubblicati. | Eccezione `DbUpdateException` (`Restrict`). | Intercettazione eccezione, Soft Delete o dialog bloccante. |
| **EC-08** | Categorie | Creazione categorie duplicate con maiuscole/minuscole. | Ridondanza e confusione nei filtri. | Conversione `.ToLower()` + Indice `UNIQUE` su DB. |
| **EC-09** | Categorie | Eliminazione categoria collegata ad articoli. | Errore vincolo relazionale. | Pulizia relazioni ponte N:N o blocco operazione via UI. |
| **EC-10** | Visualizzazioni | Molteplici scritture letture per re-render Blazor. | Saturazione tabella visualizzazioni. | Tracciamento in `firstRender` / Guardia booleana. |
| **EC-11** | Social / Commenti | Invio commento su articolo eliminato in contemporanea. | Crash app per Foreign Key fallita. | Blocco `try-catch` con notifica `RadzenNotification`. |
| **EC-12** | Social / Salvati | Lista "Da leggere dopo" contiene articoli eliminati. | `NullReferenceException` in pagina. | Filtro LINQ `Where(s => s.Articolo != null)`. |
| **EC-13** | Sicurezza | Navigazione diretta via URL a rotte Admin da Utente Base. | Escalation di privilegi. | Protezione con `@attribute [Authorize(Roles = "Admin")]`. |
| **EC-14** | Architettura UI | Stato di caricamento lento dal DB non gestito. | Render improprio di variabili `null`. | Pattern triplo stato: `isLoading`, `hasError`, `data`. |

---

## 5. Conclusioni e Raccomandazioni per il Testing

La corretta gestione degli edge cases sopra analizzati garantisce la produzione di un software di livello enterprise, conforme ai requisiti didattici e professionali.

**Raccomandazioni operative per il collaudo:**
1. **Verifica dei Vincoli DB**: Eseguire script o ispezioni con *DB Browser for SQLite* per accertarsi dell'effettiva presenza degli indici `UNIQUE` e delle regole di cancellazione `Restrict`.
2. **Test delle Form Validation**: Sollecitare i form di creazione con stringhe di prova estreme, script HTML/JS, e spazi vuoti.
3. **Simulazione di Accessi Non Autorizzati**: Testare l'accesso a rotte riservate digitando direttamente gli URL da un browser con sessione da `Utente Base`.
