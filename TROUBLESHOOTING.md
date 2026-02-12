# 🔧 Rozwiązywanie problemów

## 🚨 Problemy z Azure Functions

### Problem 1: "func: command not found"

**Przyczyna:** Azure Functions Core Tools nie są zainstalowane.

**Rozwiązanie:**
```powershell
npm install -g azure-functions-core-tools@4 --unsafe-perm true
```

Jeśli to nie zadziała:
```powershell
# Alternatywnie przez Chocolatey:
choco install azure-functions-core-tools-4

# Lub pobierz MSI installer:
# https://go.microsoft.com/fwlink/?linkid=2174087
```

### Problem 2: "Cannot find module '@azure/cosmos'"

**Przyczyna:** Brak zainstalowanego SDK Cosmos DB.

**Rozwiązanie:**
```powershell
cd api
npm install @azure/cosmos
```

### Problem 3: Funkcje nie uruchamiają się

**Rozwiązanie:**
```powershell
# 1. Sprawdź czy jesteś w folderze api/
cd api

# 2. Sprawdź czy istnieje host.json
dir host.json

# 3. Sprawdź czy są zainstalowane zależności
dir node_modules

# 4. Jeśli nie - zainstaluj:
npm install

# 5. Uruchom ponownie:
func start
```

### Problem 4: Błąd "The listener for function 'X' was unable to start"

**Przyczyna:** Konflikt portów lub błąd w kodzie funkcji.

**Rozwiązanie:**
```powershell
# 1. Sprawdź logi błędów w terminalu
# 2. Sprawdź składnię w pliku index.js funkcji
# 3. Sprawdź function.json czy route jest unikalne

# Uruchom na innym porcie:
func start --port 7072
```

---

## 🌐 Problemy z CORS

### Problem: "CORS error" w konsoli przeglądarki

**Komunikat:**
```
Access to fetch at 'http://localhost:7071/api/persons' from origin
'http://localhost:8000' has been blocked by CORS policy
```

**Rozwiązanie 1 - Konfiguracja local.settings.json:**
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "node",
    "CosmosDBConnection": "YOUR_CONNECTION_STRING"
  },
  "Host": {
    "CORS": "*",
    "CORSCredentials": false
  }
}
```

**Rozwiązanie 2 - Specific origins:**
```json
"Host": {
  "CORS": "http://localhost:8000,http://127.0.0.1:8000",
  "CORSCredentials": false
}
```

**⚠️ Ważne:** Po edycji `local.settings.json`, zrestartuj `func start`!

---

## 🗄️ Problemy z Cosmos DB

### Problem 1: "Unauthorized" lub "403 Forbidden"

**Przyczyna:** Nieprawidłowy klucz lub connection string.

**Rozwiązanie:**
```powershell
# 1. Sprawdź connection string w Azure Portal:
# Cosmos DB → Settings → Keys → PRIMARY CONNECTION STRING

# 2. Zaktualizuj local.settings.json:
"CosmosDBConnection": "AccountEndpoint=https://...;AccountKey=..."

# 3. Upewnij się, że nie ma spacji na początku/końcu
```

### Problem 2: "Database/Container not found"

**Przyczyna:** Baza danych lub kontenery nie zostały utworzone.

**Rozwiązanie - Azure Portal:**
1. Przejdź do Cosmos DB
2. Data Explorer
3. New Database → Nazwa: `MeetingsDB`
4. New Container:
   - Database: `MeetingsDB`
   - Container: `Persons`
   - Partition key: `/id`
5. New Container:
   - Database: `MeetingsDB`
   - Container: `Appointments`
   - Partition key: `/personId`

**Rozwiązanie - Azure CLI:**
```powershell
# Utwórz bazę
az cosmosdb sql database create `
  --account-name umawianie-spotkan-db-twojeinicjaly `
  --resource-group YOUR_RESOURCE_GROUP `
  --name MeetingsDB

# Utwórz kontener Persons
az cosmosdb sql container create `
  --account-name umawianie-spotkan-db-twojeinicjaly `
  --resource-group YOUR_RESOURCE_GROUP `
  --database-name MeetingsDB `
  --name Persons `
  --partition-key-path "/id"

# Utwórz kontener Appointments
az cosmosdb sql container create `
  --account-name umawianie-spotkan-db-twojeinicjaly `
  --resource-group YOUR_RESOURCE_GROUP `
  --database-name MeetingsDB `
  --name Appointments `
  --partition-key-path "/personId"
```

### Problem 3: "Request rate is large" (429 Error)

**Przyczyna:** Przekroczono limit RU/s (Request Units per second).

**Rozwiązanie:**
1. Poczekaj chwilę (throttling)
2. W Azure Portal zwiększ RU/s dla kontenera
3. Zoptymalizuj zapytania (użyj indeksów)

### Problem 4: Połączenie z Cosmos DB timeout

**Rozwiązanie:**
```powershell
# Sprawdź połączenie internetowe
Test-NetConnection google.com

# Sprawdź firewall Cosmos DB w Azure Portal:
# Cosmos DB → Settings → Firewall and virtual networks
# Dodaj swoje IP lub wybierz "Accept connections from within public Azure datacenters"
```

---

## 🖥️ Problemy z Frontendem

### Problem 1: Frontend nie wyświetla się

**Rozwiązanie - Python HTTP Server:**
```powershell
cd frontend
python --version  # Sprawdź czy Python jest zainstalowany
python -m http.server 8000
```

**Rozwiązanie - Node.js:**
```powershell
npm install -g http-server
http-server -p 8000
```

**Rozwiązanie - VS Code Live Server:**
1. Zainstaluj rozszerzenie "Live Server"
2. Kliknij prawym na `index.html`
3. "Open with Live Server"

### Problem 2: "API_BASE_URL is not defined"

**Rozwiązanie:**
Sprawdź `frontend/app.js` - pierwsza linia:
```javascript
const API_BASE_URL = 'http://localhost:7071/api';
```

### Problem 3: Przyciski nie działają

**Rozwiązanie:**
```powershell
# Otwórz konsolę przeglądarki (F12)
# Sprawdź błędy JavaScript

# Sprawdź czy app.js jest załadowany:
# DevTools → Network → app.js (powinien być status 200)
```

### Problem 4: Lista osób jest pusta

**Rozwiązanie:**
```javascript
// Otwórz konsolę (F12) i wykonaj:
fetch('http://localhost:7071/api/persons')
  .then(r => r.json())
  .then(data => console.log(data))

// Sprawdź czy są jakieś osoby w bazie
// Jeśli nie - dodaj osobę przez frontend lub POST request
```

---

## 🔍 Problemy z danymi

### Problem 1: Sloty czasowe nie wyświetlają się

**Możliwe przyczyny:**
1. Brak osób w bazie
2. Wybrany dzień jest wyłączony w dostępności
3. Wszystkie sloty są zarezerwowane

**Debugowanie:**
```javascript
// W konsoli przeglądarki:
const personId = 'person_123';
const date = '2026-02-20';

fetch(`http://localhost:7071/api/slots?personId=${personId}&date=${date}`)
  .then(r => r.json())
  .then(data => console.log(data));
```

**Sprawdź w Cosmos DB:**
1. Azure Portal → Cosmos DB → Data Explorer
2. MeetingsDB → Persons
3. Sprawdź pole `availability` dla wybranej osoby

### Problem 2: Nie można zarezerwować spotkania (409 Conflict)

**Przyczyna:** Slot jest już zajęty.

**Rozwiązanie:**
1. Odśwież listę dostępnych slotów
2. Wybierz inny slot
3. Sprawdź w Appointments czy faktycznie istnieje rezerwacja

### Problem 3: Duplikaty osób w bazie

**Przyczyna:** Brak walidacji przed dodaniem.

**Rozwiązanie - ręczne usunięcie:**
1. Azure Portal → Cosmos DB → Data Explorer
2. MeetingsDB → Persons
3. Znajdź duplikat i kliknij "Delete"

**Rozwiązanie - przez API (dodaj funkcję DELETE):**
```javascript
// Dodaj w api/DeletePerson/index.js
const { CosmosClient } = require("@azure/cosmos");

module.exports = async function (context, req) {
    const connectionString = process.env.CosmosDBConnection;
    const client = new CosmosClient(connectionString);

    const database = client.database("MeetingsDB");
    const container = database.container("Persons");

    const personId = req.params.id;

    try {
        await container.item(personId, personId).delete();
        context.res = {
            status: 200,
            body: { message: "Person deleted" }
        };
    } catch (error) {
        context.res = {
            status: 404,
            body: { error: "Person not found" }
        };
    }
};
```

---

## 🛠️ Narzędzia debugowania

### 1. Sprawdź czy Functions działają

```powershell
# Test GET endpoint
Invoke-RestMethod -Uri "http://localhost:7071/api/persons" -Method GET

# Test POST endpoint
$body = @{ name = "Test"; email = "test@test.com" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:7071/api/persons" `
    -Method POST -Body $body -ContentType "application/json"
```

### 2. Logi Azure Functions

```powershell
# Functions wyświetlają logi w terminalu
# Szukaj linii z:
# - [Information]
# - [Error]
# - [Warning]

# Dodaj własne logi:
context.log('Debug: personId =', personId);
```

### 3. Logi przeglądarki (F12)

```javascript
// W app.js dodaj logi:
console.log('API Response:', data);
console.log('Selected slot:', selectedSlot);
console.log('Form data:', { personId, date, time });
```

### 4. Network Monitoring (F12 → Network)

Sprawdź:
- Status Code (200 = OK, 404 = Not Found, 500 = Server Error)
- Response body (kliknij request → Preview/Response)
- Request headers (czy Content-Type jest ustawiony)
- Request payload (co wysyłasz do API)

---

## 📊 Checklista diagnostyczna

Jeśli coś nie działa, przejdź przez tę listę:

- [ ] Azure Functions Core Tools zainstalowane (`func --version`)
- [ ] Node.js zainstalowany (`node --version`)
- [ ] Folder api/ zawiera host.json i package.json
- [ ] npm install wykonane w folderze api/
- [ ] @azure/cosmos zainstalowany w api/node_modules
- [ ] local.settings.json zawiera prawidłowy CosmosDBConnection
- [ ] local.settings.json zawiera CORS: "*"
- [ ] func start działa bez błędów
- [ ] Cosmos DB: baza MeetingsDB istnieje
- [ ] Cosmos DB: kontener Persons istnieje (partition key: /id)
- [ ] Cosmos DB: kontener Appointments istnieje (partition key: /personId)
- [ ] Frontend: index.html, styles.css, app.js istnieją
- [ ] Frontend: serwer HTTP uruchomiony (port 8000)
- [ ] app.js: API_BASE_URL = 'http://localhost:7071/api'
- [ ] Konsola przeglądarki (F12) - brak błędów
- [ ] Network tab (F12) - requesty do API (status 200)

---

## 🆘 Ostateczne rozwiązanie

Jeśli nic nie pomaga, zacznij od zera:

```powershell
# 1. Usuń folder api (jeśli istnieje)
Remove-Item -Recurse -Force api

# 2. Utwórz nowy projekt
mkdir api
cd api
func init . --javascript
npm install @azure/cosmos

# 3. Skopiuj local.settings.json z prawidłowym connection string
# 4. Stwórz funkcje ponownie (func new ...)
# 5. Skopiuj kod z INSTRUKCJA.md

# 6. Testuj każdą funkcję pojedynczo
func start

# 7. W nowym terminalu testuj:
Invoke-RestMethod -Uri "http://localhost:7071/api/persons"
```

---

## 📞 Gdzie szukać pomocy?

1. **Dokumentacja Azure Functions:**
   - https://docs.microsoft.com/azure/azure-functions/

2. **Dokumentacja Cosmos DB:**
   - https://docs.microsoft.com/azure/cosmos-db/

3. **Stack Overflow:**
   - Tag: [azure-functions]
   - Tag: [azure-cosmosdb]

4. **Azure Functions na GitHub:**
   - https://github.com/Azure/Azure-Functions

5. **Forum MS Learn:**
   - https://learn.microsoft.com/answers/

---

**Powodzenia w debugowaniu! 🔧**
