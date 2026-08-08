# Modello dei Dati e Architettura Database (Entity Framework Core)

Il presente documento descrive in dettaglio la modellazione concettuale e relazionale del database per la piattaforma **PostIn**, implementata tramite **Entity Framework Core**.

---

## 1. Enumerazioni di Sistema

L'applicazione definisce due enumerazioni per la gestione dei ruoli e dello stato degli account utente:

```csharp
public enum RuoloUtente
{
    UtenteBase,
    Amministratore
}

public enum StatoAccountUtente
{
    Attivo,
    Disabilitato,
    Sospeso
}
```

---

## 2. Struttura delle Entità e Modello Relazionale

### 2.1 Dipendente
Rappresenta l'utente della piattaforma intranet aziendale.

- **Chiave Primaria**: `ID_Dipendente` (int)
- **Attributi**:
  - `Nome` (string, max 50, obbligatorio)
  - `Cognome` (string, max 50, obbligatorio)
  - `Username` (string, max 50, obbligatorio)
  - `PasswordHash` (string, max 255, obbligatorio)
  - `Ruolo` (`RuoloUtente`, default `UtenteBase`)
  - `StatoAccount` (`StatoAccountUtente`, default `Attivo`)
  - `UltimoAccesso` (DateTime?, opzionale)
- **Relazioni**:
  - 1:N con `Articolo` (`ArticoliCreati`)
  - 1:N con `Commento` (`Commenti`)
  - 1:N con `Visualizzazione` (`Visualizzazioni`)
  - N:M tramite `Like` (`Likes`)
  - N:M tramite `SalvataggioDaLeggere` (`SalvataggiDaLeggere`)
  - N:M tramite `CategoriaPreferita` (`CategoriePreferite`)
  - N:M Riflessiva tramite `IscrizioneFollow` (`Following`, `Followers`)

---

### 2.2 Articolo
Rappresenta il contenuto o post aziendale pubblicato sul portale.

- **Chiave Primaria**: `ID_Articolo` (int)
- **Attributi**:
  - `Titolo` (string, max 255, obbligatorio)
  - `CorpoTesto` (string, obbligatorio)
  - `ImmagineCopertina` (string, max 255, opzionale)
  - `DataOraCreazione` (DateTime, default `DateTime.Now`)
- **Chiavi Esterne**:
  - `FK_Autore` -> `Dipendente.ID_Dipendente`
- **Relazioni**:
  - N:M con `Categoria` tramite `ArticoloCategoria`
  - 1:N con `Commento`
  - 1:N con `Visualizzazione`
  - N:M tramite `Like`
  - N:M tramite `SalvataggioDaLeggere`

---

### 2.3 Categoria
Rappresenta l'hashtag o il tema di classificazione degli articoli.

- **Chiave Primaria**: `ID_Categoria` (int)
- **Attributi**:
  - `NomeCategoria` (string, max 50, obbligatorio)
- **Relazioni**:
  - N:M con `Articolo` tramite `ArticoloCategoria`
  - N:M con `Dipendente` tramite `CategoriaPreferita`

---

### 2.4 ArticoloCategoria (Tabella di Snodo N:M)
Associa ciascun articolo ad una o più categorie tematiche.

- **Chiavi Esterne / Chiave Primaria Composta**:
  - `FK_Articolo` -> `Articolo.ID_Articolo`
  - `FK_Categoria` -> `Categoria.ID_Categoria`

---

### 2.5 CategoriaPreferita (Tabella di Snodo N:M)
Memorizza le categorie preferite da ogni dipendente per la personalizzazione del feed.

- **Chiavi Esterne / Chiave Primaria Composta**:
  - `FK_Dipendente` -> `Dipendente.ID_Dipendente`
  - `FK_Categoria` -> `Categoria.ID_Categoria`

---

### 2.6 IscrizioneFollow (Tabella di Snodo N:M Riflessiva)
Gestisce le relazioni tra dipendenti che si seguono a vicenda.

- **Chiavi Esterne / Chiave Primaria Composta**:
  - `FK_Follower` -> `Dipendente.ID_Dipendente` (Utente seguitore)
  - `FK_Followed` -> `Dipendente.ID_Dipendente` (Utente seguito)
- **Attributo Temporale**:
  - `DataInizioInteraction` (DateTime, default `DateTime.Now`)

---

### 2.7 SalvataggioDaLeggere (Tabella di Snodo N:M)
Rappresenta la lista privata degli articoli salvati da un dipendente per una lettura successiva.

- **Chiavi Esterne / Chiave Primaria Composta**:
  - `FK_Dipendente` -> `Dipendente.ID_Dipendente`
  - `FK_Articolo` -> `Articolo.ID_Articolo`
- **Attributo Temporale**:
  - `DataSalvataggio` (DateTime, default `DateTime.Now`)

---

### 2.8 Like (Tabella di Snodo N:M)
Gestisce i "Mi piace" rilasciati dai dipendenti sui singoli articoli.

- **Chiavi Esterne / Chiave Primaria Composta**:
  - `FK_Dipendente` -> `Dipendente.ID_Dipendente`
  - `FK_Articolo` -> `Articolo.ID_Articolo`
- **Attributo Temporale**:
  - `DataRilascio` (DateTime, default `DateTime.Now`)

---

### 2.9 Visualizzazione
Registra l'evento di apertura e lettura di un articolo da parte di un dipendente.

- **Chiave Primaria**: `ID_Visualizzazione` (int)
- **Chiavi Esterne**:
  - `FK_Dipendente` -> `Dipendente.ID_Dipendente`
  - `FK_Articolo` -> `Articolo.ID_Articolo`
- **Attributo Temporale**:
  - `DataOraVisualizzazione` (DateTime, default `DateTime.Now`)

---

### 2.10 Commento
Memorizza i commenti testuali pubblicati dai lettori sotto gli articoli.

- **Chiave Primaria**: `ID_Commento` (int)
- **Chiavi Esterne**:
  - `FK_Articolo` -> `Articolo.ID_Articolo`
  - `FK_Autore` -> `Dipendente.ID_Dipendente`
- **Attributi**:
  - `TestoCommento` (string, obbligatorio)
  - `DataPubblicazione` (DateTime, default `DateTime.Now`)

---

## 3. Mappa Sintetica delle Relazioni

| Entità Origine | Entità Destinazione | Tipo Relazione | Tabella di Snodo / FK |
| :--- | :--- | :--- | :--- |
| `Dipendente` | `Articolo` | 1:N | `FK_Autore` |
| `Dipendente` | `Commento` | 1:N | `FK_Autore` |
| `Dipendente` | `Visualizzazione` | 1:N | `FK_Dipendente` |
| `Articolo` | `Commento` | 1:N | `FK_Articolo` |
| `Articolo` | `Visualizzazione` | 1:N | `FK_Articolo` |
| `Articolo` | `Categoria` | N:M | `ArticoloCategoria` |
| `Dipendente` | `Categoria` | N:M | `CategoriaPreferita` |
| `Dipendente` | `Dipendente` | N:M (Riflessiva) | `IscrizioneFollow` |
| `Dipendente` | `Articolo` | N:M | `SalvataggioDaLeggere` |
| `Dipendente` | `Articolo` | N:M | `Like` |
