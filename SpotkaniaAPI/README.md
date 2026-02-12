# System do umawiania spotkań - C# .NET 8 Isolated

## 📋 Opis projektu

System do umawiania spotkań zbudowany w oparciu o:
- **Azure Functions** (C# .NET 8 Isolated Worker)
- **Azure Cosmos DB** (NoSQL)

## 🎯 Funkcjonalności (ocena 5.0)

✅ Model danych z konfigurowalnymi godzinami pracy
✅ Dodawanie osób z indywidualnym grafikiem
✅ Obliczanie wolnych slotów co 30 minut
✅ Rezerwacja terminów z walidacją
✅ Zarządzanie zarezerwowanymi terminami

## 🏗️ Struktura projektu

```
SpotkaniaAPI/
├── Models/
│   ├── Person.cs                    # Model osoby z grafikiem
│   ├── WorkDay.cs                   # Godziny pracy w dniu
│   ├── BookedSlot.cs                # Zarezerwowany termin
│   ├── BookAppointmentRequest.cs   # DTO rezerwacji
│   └── AvailabilityResponse.cs     # DTO dostępności
│
├── Functions/
│   ├── AddPersonFunction.cs        # POST /api/persons
│   ├── GetPersonsFunction.cs       # GET /api/persons, GET /api/persons/{id}
│   ├── GetAvailabilityFunction.cs  # GET /api/availability/{id}?date=YYYY-MM-DD
│   └── BookAppointmentFunction.cs  # POST /api/book
│
├── Program.cs                       # Konfiguracja DI i Cosmos DB
├── host.json                        # Konfiguracja hosta
├── local.settings.json              # Ustawienia lokalne
└── SpotkaniaAPI.csproj             # Plik projektu
```

## 🚀 Jak uruchomić?

### 1. Wymagania

- **.NET 8 SDK** - https://dotnet.microsoft.com/download/dotnet/8.0
- **Azure Functions Core Tools v4** - `npm install -g azure-functions-core-tools@4`
- **Visual Studio 2022** lub **VS Code** z rozszerzeniem C#
- **Azure Cosmos DB** - konto skonfigurowane w Azure Portal

### 2. Konfiguracja Cosmos DB

W Azure Portal:
1. Przejdź do swojego konta Cosmos DB
2. Utwórz bazę danych: **SpotkaniaDB**
3. Utwórz kontener: **Persons**
   - Partition key: `/id`
   - Throughput: 400 RU/s (manual)

### 3. Konfiguracja projektu

Edytuj `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDbConnectionString": "TWÓJ_CONNECTION_STRING",
    "DatabaseName": "SpotkaniaDB",
    "ContainerName": "Persons"
  }
}
```

### 4. Uruchomienie

```powershell
# Przejdź do folderu projektu
cd SpotkaniaAPI

# Przywróć zależności
dotnet restore

# Zbuduj projekt
dotnet build

# Uruchom Functions
func start
```

Aplikacja będzie dostępna na: **http://localhost:7071**

## 📡 API Endpoints

### POST /api/persons
Dodaje nową osobę do systemu.

**Request Body:**
```json
{
  "name": "Dr Anna Kowalska",
  "email": "anna.kowalska@example.com",
  "workHours": {
    "Monday": { "start": "09:00", "end": "17:00", "enabled": true },
    "Tuesday": { "start": "09:00", "end": "17:00", "enabled": true },
    "Wednesday": { "start": "09:00", "end": "13:00", "enabled": true },
    "Thursday": { "enabled": false },
    "Friday": { "start": "10:00", "end": "18:00", "enabled": true },
    "Saturday": { "enabled": false },
    "Sunday": { "enabled": false }
  }
}
```

**Response:** `201 Created`

---

### GET /api/persons
Pobiera wszystkie osoby.

**Response:** `200 OK`
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "Dr Anna Kowalska",
    "email": "anna.kowalska@example.com",
    "workHours": { ... },
    "bookedSlots": [ ... ]
  }
]
```

---

### GET /api/persons/{id}
Pobiera konkretną osobę.

**Response:** `200 OK` lub `404 Not Found`

---

### GET /api/availability/{id}?date=YYYY-MM-DD
Pobiera dostępne sloty czasowe.

**Przykład:**
```
GET /api/availability/123e4567-e89b-12d3-a456-426614174000?date=2026-02-20
```

**Response:** `200 OK`
```json
{
  "personId": "123e4567-e89b-12d3-a456-426614174000",
  "personName": "Dr Anna Kowalska",
  "date": "2026-02-20",
  "dayOfWeek": "Thursday",
  "availableSlots": [
    "09:00", "09:30", "10:00", "10:30", "11:00",
    "11:30", "12:00", "12:30", "13:00", "13:30"
  ]
}
```

---

### POST /api/book
Rezerwuje spotkanie.

**Request Body:**
```json
{
  "personId": "123e4567-e89b-12d3-a456-426614174000",
  "date": "2026-02-20",
  "time": "10:00",
  "clientName": "Jan Nowak",
  "clientEmail": "jan.nowak@example.com",
  "description": "Konsultacja w sprawie projektu"
}
```

**Response:** `201 Created` lub `409 Conflict` (slot zajęty)

---

## 🧪 Testowanie API

### PowerShell

```powershell
# 1. Dodaj osobę
$person = @{
    name = "Dr Anna Kowalska"
    email = "anna.kowalska@example.com"
    workHours = @{
        Monday = @{ start = "09:00"; end = "17:00"; enabled = $true }
        Tuesday = @{ start = "09:00"; end = "17:00"; enabled = $true }
        Wednesday = @{ start = "09:00"; end = "13:00"; enabled = $true }
        Thursday = @{ enabled = $false }
        Friday = @{ start = "10:00"; end = "18:00"; enabled = $true }
        Saturday = @{ enabled = $false }
        Sunday = @{ enabled = $false }
    }
} | ConvertTo-Json -Depth 10

$result = Invoke-RestMethod -Uri "http://localhost:7071/api/persons" `
    -Method POST -Body $person -ContentType "application/json"

$personId = $result.id
Write-Host "Utworzono osobę o ID: $personId"

# 2. Sprawdź dostępne sloty
$date = "2026-02-20"
$availability = Invoke-RestMethod -Uri "http://localhost:7071/api/availability/$personId?date=$date"
Write-Host "Dostępne sloty: $($availability.availableSlots -join ', ')"

# 3. Zarezerwuj spotkanie
$booking = @{
    personId = $personId
    date = $date
    time = "10:00"
    clientName = "Jan Nowak"
    clientEmail = "jan.nowak@example.com"
    description = "Konsultacja"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/book" `
    -Method POST -Body $booking -ContentType "application/json"
```

### Curl (Git Bash / Linux)

```bash
# Dodaj osobę
curl -X POST http://localhost:7071/api/persons \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Dr Anna Kowalska",
    "email": "anna.kowalska@example.com",
    "workHours": {
      "Monday": {"start": "09:00", "end": "17:00", "enabled": true},
      "Tuesday": {"start": "09:00", "end": "17:00", "enabled": true},
      "Wednesday": {"start": "09:00", "end": "13:00", "enabled": true},
      "Thursday": {"enabled": false},
      "Friday": {"start": "10:00", "end": "18:00", "enabled": true},
      "Saturday": {"enabled": false},
      "Sunday": {"enabled": false}
    }
  }'

# Pobierz dostępne sloty
curl "http://localhost:7071/api/availability/PERSON_ID?date=2026-02-20"

# Zarezerwuj
curl -X POST http://localhost:7071/api/book \
  -H "Content-Type: application/json" \
  -d '{
    "personId": "PERSON_ID",
    "date": "2026-02-20",
    "time": "10:00",
    "clientName": "Jan Nowak",
    "clientEmail": "jan.nowak@example.com"
  }'
```

## 🔧 Rozwiązywanie problemów

### "Cannot find module Microsoft.Azure.Cosmos"
```powershell
dotnet restore
```

### Cosmos DB connection error
Sprawdź:
- Connection string w `local.settings.json`
- Czy baza `SpotkaniaDB` i kontener `Persons` istnieją
- Czy kontener ma partition key `/id`

### Function nie startuje
```powershell
# Zbuduj ponownie
dotnet clean
dotnet build

# Sprawdź logi
func start --verbose
```

## 📚 Technologie i pakiety

- **Microsoft.Azure.Functions.Worker** v1.21.0
- **Microsoft.Azure.Functions.Worker.Extensions.Http** v3.1.0
- **Microsoft.Azure.Cosmos** v3.38.1
- **.NET 8.0**
- **Azure Functions Runtime v4**

## 🎓 Wymagania na oceny

### Ocena 4.0 (podstawowa):
- ✅ Dodawanie osób
- ✅ Pobieranie listy osób
- ✅ Dostępne terminy (standardowy grafik 8-16)
- ✅ Rezerwacja spotkań

### Ocena 5.0 (rozszerzona):
- ✅ Wszystko z wersji 4.0
- ✅ **Konfigurowalne godziny pracy (WorkHours)**
- ✅ **Różne dni tygodnia**
- ✅ **Różne godziny dla każdego dnia**
- ✅ **Walidacja dostępności przy rezerwacji**
- ✅ **Generowanie slotów 30-minutowych**

## 📝 Autor

Projekt stworzony na potrzeby przedmiotu: **Programowanie w Chmurze Obliczeniowej**

---

**Powodzenia! 🚀**
