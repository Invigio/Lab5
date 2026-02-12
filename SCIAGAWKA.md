# 📝 Ściągawka - Najważniejsze komendy

## 🚀 Szybkie uruchomienie

```powershell
# Backend (Terminal 1)
cd api
func start

# Frontend (Terminal 2)
cd frontend
python -m http.server 8000
```

**URLs:**
- Backend API: http://localhost:7071/api
- Frontend: http://localhost:8000

---

## 🔧 Inicjalizacja projektu (tylko raz)

```powershell
# 1. Stwórz projekt Functions
mkdir api
cd api
func init . --javascript
npm install @azure/cosmos

# 2. Stwórz funkcje API
func new --name AddPerson --template "HTTP trigger" --authlevel "anonymous"
func new --name GetPersons --template "HTTP trigger" --authlevel "anonymous"
func new --name GetAvailableSlots --template "HTTP trigger" --authlevel "anonymous"
func new --name BookAppointment --template "HTTP trigger" --authlevel "anonymous"
func new --name GetAppointments --template "HTTP trigger" --authlevel "anonymous"

# 3. Edytuj pliki zgodnie z INSTRUKCJA.md
```

---

## 📡 API Endpoints

| Method | Endpoint | Opis |
|--------|----------|------|
| POST | `/api/persons` | Dodaj osobę |
| GET | `/api/persons` | Lista osób |
| GET | `/api/slots?personId=X&date=Y` | Dostępne terminy |
| POST | `/api/appointments` | Zarezerwuj |
| GET | `/api/appointments` | Lista spotkań |
| GET | `/api/appointments?personId=X` | Spotkania osoby |

---

## 🧪 Szybkie testy (PowerShell)

```powershell
# Test 1: Dodaj osobę
$body = @{name="Jan Kowalski"; email="jan@test.com"} | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:7071/api/persons" -Method POST -Body $body -ContentType "application/json"

# Test 2: Lista osób
Invoke-RestMethod -Uri "http://localhost:7071/api/persons"

# Test 3: Dostępne sloty
Invoke-RestMethod -Uri "http://localhost:7071/api/slots?personId=PERSON_ID&date=2026-02-20"

# Test 4: Zarezerwuj spotkanie
$booking = @{
    personId="PERSON_ID"; date="2026-02-20"; time="10:00";
    clientName="Test"; clientEmail="test@test.com"
} | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:7071/api/appointments" -Method POST -Body $booking -ContentType "application/json"

# Test 5: Lista spotkań
Invoke-RestMethod -Uri "http://localhost:7071/api/appointments"
```

---

## 🗄️ Cosmos DB - Konfiguracja

### Azure Portal:
1. Cosmos DB → Data Explorer
2. New Database: `MeetingsDB`
3. New Container: `Persons` (partition key: `/id`)
4. New Container: `Appointments` (partition key: `/personId`)

### Connection string w `api/local.settings.json`:
```json
{
  "Values": {
    "CosmosDBConnection": "AccountEndpoint=https://...;AccountKey=...;"
  },
  "Host": {
    "CORS": "*"
  }
}
```

---

## 🐛 Troubleshooting - Top 5

### 1. CORS Error
```json
// api/local.settings.json
"Host": { "CORS": "*" }
```
Zrestartuj: `func start`

### 2. Module not found
```powershell
cd api
npm install @azure/cosmos
```

### 3. Database not found
Azure Portal → Cosmos DB → Data Explorer → Utwórz bazę i kontenery

### 4. Functions nie startują
```powershell
cd api
func start --verbose  # Zobacz szczegółowe logi
```

### 5. Frontend nie łączy się
Sprawdź w `app.js`:
```javascript
const API_BASE_URL = 'http://localhost:7071/api';
```

---

## 📁 Struktura projektu

```
Lab5/
├── api/
│   ├── AddPerson/
│   │   ├── index.js          # Logika
│   │   └── function.json     # Konfiguracja
│   ├── GetPersons/
│   ├── GetAvailableSlots/
│   ├── BookAppointment/
│   ├── GetAppointments/
│   ├── host.json
│   ├── local.settings.json   # Nie commituj!
│   └── package.json
│
└── frontend/
    ├── index.html
    ├── styles.css
    └── app.js
```

---

## 🎯 Punkty kontrolne

### Backend (API):
- [ ] `func start` działa bez błędów
- [ ] GET `/api/persons` zwraca []
- [ ] POST `/api/persons` dodaje osobę (status 201)
- [ ] GET `/api/slots` zwraca sloty (status 200)
- [ ] POST `/api/appointments` rezerwuje (status 201)
- [ ] GET `/api/appointments` zwraca listę (status 200)

### Frontend:
- [ ] Strona się ładuje (http://localhost:8000)
- [ ] Formularz dodawania osoby - działa
- [ ] Dropdown z osobami - wypełnia się
- [ ] Wybór daty - pokazuje sloty
- [ ] Rezerwacja - dodaje spotkanie
- [ ] Lista spotkań - wyświetla rezerwacje

### Cosmos DB:
- [ ] Baza `MeetingsDB` istnieje
- [ ] Kontener `Persons` istnieje
- [ ] Kontener `Appointments` istnieje
- [ ] Dane są zapisywane (sprawdź Data Explorer)

---

## 💾 Przykładowe dane testowe

### Osoba (POST /api/persons):
```json
{
  "name": "Dr. Anna Kowalska",
  "email": "anna.kowalska@example.com",
  "availability": {
    "monday": {"enabled": true, "start": "09:00", "end": "17:00"},
    "tuesday": {"enabled": true, "start": "09:00", "end": "17:00"},
    "wednesday": {"enabled": true, "start": "09:00", "end": "13:00"},
    "thursday": {"enabled": false},
    "friday": {"enabled": true, "start": "10:00", "end": "18:00"},
    "saturday": {"enabled": false},
    "sunday": {"enabled": false}
  }
}
```

### Spotkanie (POST /api/appointments):
```json
{
  "personId": "person_1707734400000",
  "date": "2026-02-20",
  "time": "10:00",
  "clientName": "Jan Nowak",
  "clientEmail": "jan.nowak@example.com",
  "description": "Konsultacja w sprawie projektu"
}
```

---

## 🔑 Najbardziej przydatne komendy

```powershell
# Uruchom backend
cd api; func start

# Uruchom frontend
cd frontend; python -m http.server 8000

# Zainstaluj zależności
cd api; npm install

# Sprawdź logi Functions (w terminalu gdzie działa func start)

# Test API
Invoke-RestMethod -Uri "http://localhost:7071/api/persons"

# Restart Functions (Ctrl+C potem func start)

# Sprawdź błędy w przeglądarce (F12 → Console)
```

---

## 📚 Pliki dokumentacji

- **README.md** - Ogólne info o projekcie
- **SZYBKI-START.md** - Komendy do szybkiego uruchomienia
- **INSTRUKCJA.md** - Pełna instrukcja krok po kroku
- **TESTOWANIE-API.md** - Przykłady requestów
- **TROUBLESHOOTING.md** - Rozwiązywanie problemów
- **SCIAGAWKA.md** - Ten plik :)

---

## ⏱️ Workflow pracy

```
1. Terminal 1: cd api → func start
2. Terminal 2: cd frontend → python -m http.server 8000
3. Przeglądarka: http://localhost:8000
4. Dodaj osobę
5. Sprawdź czy osoba jest w liście
6. Wybierz osobę i datę
7. Sprawdź dostępne sloty
8. Zarezerwuj spotkanie
9. Sprawdź czy spotkanie się wyświetla
10. Powtórz dla różnych scenariuszy testowych
```

---

## 🎓 Ocenianie

### Wersja na 4.0:
✅ API dodawania osób
✅ API pobierania osób
✅ API dostępnych terminów
✅ API rezerwacji
✅ Frontend działający

### Wersja na 5.0:
✅ Wszystko z wersji 4.0
✅ **Konfigurowalna dostępność**
✅ Różne dni tygodnia
✅ Różne godziny pracy
✅ UI do edycji dostępności

---

## 🌐 Linki

- **Azure Portal:** https://portal.azure.com
- **Azure Functions Docs:** https://docs.microsoft.com/azure/azure-functions/
- **Cosmos DB Docs:** https://docs.microsoft.com/azure/cosmos-db/

---

**Wydrukuj tę ściągawkę i trzymaj pod ręką podczas pracy! 📄**
