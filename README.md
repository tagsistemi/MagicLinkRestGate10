# MagicLinkRestGate

Web API REST che funge da gateway verso i servizi SOAP di Mago (LoginManager e TbServices/TBDMS).
Permette di gestire autenticazione e allegati documentali tramite chiamate REST.

## Configurazione - appsettings.json

I parametri di connessione a Mago si configurano nella sezione `MagicLinkParams`:

```json
{
  "MagicLinkParams": {
    "LoginCompany": "NomeAzienda",
    "LoginInstallationName": "magonet",
    "LoginPassword": "password",
    "LoginServerMago": "localhost",
    "LoginUsername": "sa"
  }
}
```

| Parametro | Descrizione |
|---|---|
| `LoginCompany` | Nome dell'azienda Mago a cui connettersi |
| `LoginInstallationName` | Nome dell'istanza/installazione di Mago (es. `magonet`) |
| `LoginPassword` | Password dell'utente Mago |
| `LoginServerMago` | Hostname o IP del server dove gira Mago |
| `LoginUsername` | Username dell'utente Mago |

Questi parametri vengono usati dal metodo `LoginSoapBySettings` e da `GetListaAllegatiOrigine` per effettuare login automatici senza passare le credenziali nella richiesta.

---

## Endpoint API

Base URL: `https://<host>/api/MagicLinkSoap`

### Risposta standard

Tutti gli endpoint restituiscono un oggetto `BeResponseGt<T>`:

```json
{
  "message": "OK",
  "action": "nome_azione",
  "value": "<valore di ritorno>"
}
```

---

### POST /LoginSoap

Effettua il login a Mago con credenziali esplicite. Restituisce un token di autenticazione valido per 20 minuti.

**Parametri (query string):**

| Parametro | Tipo | Descrizione |
|---|---|---|
| `user` | string | Username Mago |
| `passw` | string | Password |
| `companyname` | string | Nome azienda |
| `process` | string | Nome del processo chiamante |

**Esempio:**

```
POST https://localhost:5001/api/MagicLinkSoap/LoginSoap?user=sa&passw=MiaPassword&companyname=AzDemo&process=test
```

**Risposta:**

```json
{
  "message": "OK",
  "action": "loginsoap",
  "value": "b51ab164-f49f-442d-b7b0-07c6d237aa37"
}
```

---

### POST /LoginSoapBySettings

Effettua il login usando i parametri configurati in `appsettings.json`. Non richiede parametri.

**Esempio:**

```
POST https://localhost:5001/api/MagicLinkSoap/LoginSoapBySettings
```

**Risposta:**

```json
{
  "message": "OK",
  "action": "loginsoap",
  "value": "b51ab164-f49f-442d-b7b0-07c6d237aa37"
}
```

---

### POST /LogOffSoap

Effettua il logoff e invalida il token. Da eseguire dopo ogni operazione completata.

**Parametri (query string):**

| Parametro | Tipo | Descrizione |
|---|---|---|
| `token` | string | Token di autenticazione ottenuto dal login |

**Esempio:**

```
POST https://localhost:5001/api/MagicLinkSoap/LogOffSoap?token=b51ab164-f49f-442d-b7b0-07c6d237aa37
```

**Risposta:**

```json
{
  "message": "OK",
  "action": "LogOffSoap",
  "value": "b51ab164-f49f-442d-b7b0-07c6d237aa37"
}
```

---

### POST /TBDMSAttachBinaryContentInDocument

Allega un file (in formato Base64) a un documento Mago.

**Body (JSON):**

```json
{
  "authenticationToken": "token-ottenuto-dal-login",
  "binaryContent": "contenuto del file convertito in stringa Base64",
  "sourceFileName": "test.pdf",
  "description": "descrizione allegato",
  "documentNamespace": "Document.ERP.Sales.Documents.Invoice",
  "documentKey": "SaleDocId:206;",
  "attachmentID": "0",
  "result": ""
}
```

| Campo | Descrizione |
|---|---|
| `authenticationToken` | Token di autenticazione |
| `binaryContent` | File codificato in Base64 |
| `sourceFileName` | Nome del file originale |
| `description` | Descrizione dell'allegato |
| `documentNamespace` | Namespace del documento Mago (es. `Document.ERP.Sales.Documents.Invoice`) |
| `documentKey` | Chiave del documento di destinazione (es. `SaleDocId:206;`) |
| `attachmentID` | ID allegato (`"0"` per nuovo allegato) |

**Risposta:**

```json
{
  "message": "OK",
  "action": "AttachBinaryContentInDocument",
  "value": {
    "attachBinaryContentInDocumentResult": true,
    "attachmentID": 42,
    "result": ""
  }
}
```

---

### POST /TBDMSGetAttachmentIDByFileNameSoap

Recupera l'ID di un allegato cercandolo per nome file.

**Parametri (query string):**

| Parametro | Tipo | Descrizione |
|---|---|---|
| `authenticationToken` | string | Token di autenticazione |
| `documentNamespace` | string | Namespace del documento |
| `documentKey` | string | Chiave del documento |
| `fileName` | string | Nome del file da cercare |

---

### POST /TBDMSGetAttachmentBinaryContent

Scarica il contenuto binario (Base64) di un allegato.

**Parametri (query string):**

| Parametro | Tipo | Descrizione |
|---|---|---|
| `authenticationToken` | string | Token di autenticazione |
| `attachmentID` | string | ID dell'allegato |
| `fileName` | string | Nome del file |
| `bVeryLargeFile` | string | `"true"` / `"false"` (default: `"false"`) |

**Risposta:**

```json
{
  "message": "OK",
  "action": "TBDMSGetAttachmentBinaryContent",
  "value": {
    "getAttachmentBinaryContentResult": true,
    "binaryContent": "base64string...",
    "fileName": "test.pdf",
    "bVeryLargeFile": false
  }
}
```

---

### GET /GetListaAllegatiOrigine

Recupera la lista degli ID degli allegati associati a un documento.

**Parametri (query string):**

| Parametro | Tipo | Descrizione |
|---|---|---|
| `AziendaOrigine` | string | Nome azienda Mago |
| `KeyOrigine` | string | Chiave del documento (es. `SaleDocId:126`) |
| `NamespaceDoc` | string | Namespace del documento |

---

### POST /TrasferisciAllegati

Trasferisce tutti gli allegati da un documento origine a un documento destinazione (anche tra aziende diverse).

**Body (JSON):**

```json
{
  "authenticationTokenOrigine": "token-azienda-origine",
  "authenticationTokenDestinazione": "token-azienda-destinazione",
  "ids": ["1", "2", "3"],
  "namespaceOrigine": "Document.ERP.Sales.Documents.Invoice",
  "namespaceDestinazione": "Document.ERP.Sales.Documents.Invoice",
  "destinationkey": "SaleDocId:198;",
  "description": "trasferimento allegati",
  "bVeryLargeFile": "false"
}
```

---

## Esempio completo: Login, Allega file e Logoff

### 1. Login

```
POST https://localhost:5001/api/MagicLinkSoap/LoginSoap?user=sa&passw=MiaPassword&companyname=AzDemo&process=test
```

Risposta: salva il `value` come token (es. `531dadd9-f291-48a8-9108-4b97e7738de0`).

### 2. Allega un file

```
POST https://localhost:5001/api/MagicLinkSoap/TBDMSAttachBinaryContentInDocument
Content-Type: application/json

{
  "authenticationToken": "531dadd9-f291-48a8-9108-4b97e7738de0",
  "binaryContent": "JVBERi0xLjQKJ...",
  "sourceFileName": "fattura.pdf",
  "description": "Fattura allegata",
  "documentNamespace": "Document.ERP.Sales.Documents.Invoice",
  "documentKey": "SaleDocId:206;",
  "attachmentID": "0",
  "result": ""
}
```

### 3. Logoff

```
POST https://localhost:5001/api/MagicLinkSoap/LogOffSoap?token=531dadd9-f291-48a8-9108-4b97e7738de0
```

---

## Docker

```bash
docker build -t magiclinkrestgate .
docker run -p 8080:8080 magiclinkrestgate
```

## Swagger

In ambiente Development, Swagger UI e' disponibile su:

```
https://localhost:5001/swagger
```

## Test API

Il controller `TestMagicLinkRestGate` (GET `/api/TestMagicLinkRestGate`) restituisce i parametri di configurazione correnti per verificare che i settings siano caricati correttamente.
