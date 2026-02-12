# 🧪 Testowanie API - Przykładowe żądania

## 1. Dodaj osobę (POST /api/persons)

### Przykład 1: Standardowa dostępność (pn-pt, 8-16)

```powershell
# PowerShell
$body = @{
    name = "Jan Kowalski"
    email = "jan.kowalski@example.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/persons" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

```bash
# Curl (Git Bash / Linux)
curl -X POST http://localhost:7071/api/persons \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Jan Kowalski",
    "email": "jan.kowalski@example.com"
  }'
```

### Przykład 2: Niestandardowa dostępność

```powershell
# PowerShell
$body = @{
    name = "Anna Nowak"
    email = "anna.nowak@example.com"
    availability = @{
        monday = @{
            enabled = $true
            start = "09:00"
            end = "17:00"
        }
        tuesday = @{
            enabled = $true
            start = "09:00"
            end = "17:00"
        }
        wednesday = @{
            enabled = $true
            start = "09:00"
            end = "13:00"
        }
        thursday = @{
            enabled = $false
        }
        friday = @{
            enabled = $true
            start = "10:00"
            end = "18:00"
        }
        saturday = @{
            enabled = $true
            start = "10:00"
            end = "14:00"
        }
        sunday = @{
            enabled = $false
        }
    }
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri "http://localhost:7071/api/persons" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

```bash
# Curl
curl -X POST http://localhost:7071/api/persons \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Anna Nowak",
    "email": "anna.nowak@example.com",
    "availability": {
      "monday": {"enabled": true, "start": "09:00", "end": "17:00"},
      "tuesday": {"enabled": true, "start": "09:00", "end": "17:00"},
      "wednesday": {"enabled": true, "start": "09:00", "end": "13:00"},
      "thursday": {"enabled": false},
      "friday": {"enabled": true, "start": "10:00", "end": "18:00"},
      "saturday": {"enabled": true, "start": "10:00", "end": "14:00"},
      "sunday": {"enabled": false}
    }
  }'
```

---

## 2. Pobierz wszystkie osoby (GET /api/persons)

```powershell
# PowerShell
Invoke-RestMethod -Uri "http://localhost:7071/api/persons" -Method GET
```

```bash
# Curl
curl http://localhost:7071/api/persons
```

**Oczekiwany wynik:**
```json
[
  {
    "id": "person_1707734400000",
    "name": "Jan Kowalski",
    "email": "jan.kowalski@example.com",
    "availability": {
      "monday": {"start": "08:00", "end": "16:00", "enabled": true},
      ...
    }
  }
]
```

---

## 3. Pobierz dostępne terminy (GET /api/slots)

```powershell
# PowerShell
$personId = "person_1707734400000"
$date = "2026-02-20"

Invoke-RestMethod -Uri "http://localhost:7071/api/slots?personId=$personId&date=$date" -Method GET
```

```bash
# Curl
curl "http://localhost:7071/api/slots?personId=person_1707734400000&date=2026-02-20"
```

**Oczekiwany wynik:**
```json
{
  "date": "2026-02-20",
  "personId": "person_1707734400000",
  "personName": "Jan Kowalski",
  "availableSlots": [
    "08:00", "08:30", "09:00", "09:30", "10:00",
    "10:30", "11:00", "11:30", "12:00", "12:30",
    "13:00", "13:30", "14:00", "14:30", "15:00", "15:30"
  ]
}
```

---

## 4. Zarezerwuj spotkanie (POST /api/appointments)

```powershell
# PowerShell
$body = @{
    personId = "person_1707734400000"
    date = "2026-02-20"
    time = "10:00"
    clientName = "Maria Wiśniewska"
    clientEmail = "maria.wisniewska@example.com"
    description = "Konsultacja w sprawie projektu"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/appointments" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

```bash
# Curl
curl -X POST http://localhost:7071/api/appointments \
  -H "Content-Type: application/json" \
  -d '{
    "personId": "person_1707734400000",
    "date": "2026-02-20",
    "time": "10:00",
    "clientName": "Maria Wiśniewska",
    "clientEmail": "maria.wisniewska@example.com",
    "description": "Konsultacja w sprawie projektu"
  }'
```

**Oczekiwany wynik:**
```json
{
  "id": "appointment_1707734500000",
  "personId": "person_1707734400000",
  "date": "2026-02-20",
  "time": "10:00",
  "clientName": "Maria Wiśniewska",
  "clientEmail": "maria.wisniewska@example.com",
  "description": "Konsultacja w sprawie projektu",
  "createdAt": "2026-02-12T10:30:00.000Z"
}
```

---

## 5. Pobierz wszystkie spotkania (GET /api/appointments)

### Wszystkie spotkania:
```powershell
# PowerShell
Invoke-RestMethod -Uri "http://localhost:7071/api/appointments" -Method GET
```

```bash
# Curl
curl http://localhost:7071/api/appointments
```

### Spotkania konkretnej osoby:
```powershell
# PowerShell
$personId = "person_1707734400000"
Invoke-RestMethod -Uri "http://localhost:7071/api/appointments?personId=$personId" -Method GET
```

```bash
# Curl
curl "http://localhost:7071/api/appointments?personId=person_1707734400000"
```

**Oczekiwany wynik:**
```json
[
  {
    "id": "appointment_1707734500000",
    "personId": "person_1707734400000",
    "date": "2026-02-20",
    "time": "10:00",
    "clientName": "Maria Wiśniewska",
    "clientEmail": "maria.wisniewska@example.com",
    "description": "Konsultacja w sprawie projektu",
    "createdAt": "2026-02-12T10:30:00.000Z"
  }
]
```

---

## 🧪 Scenariusz testowy krok po kroku

### Test 1: Pełny flow rezerwacji

```powershell
# 1. Dodaj osobę
$addPerson = @{
    name = "Dr. Piotr Zalewski"
    email = "p.zalewski@example.com"
} | ConvertTo-Json

$person = Invoke-RestMethod -Uri "http://localhost:7071/api/persons" `
    -Method POST -Body $addPerson -ContentType "application/json"

$personId = $person.id
Write-Host "Utworzono osobę: $personId"

# 2. Sprawdź dostępne terminy
$date = (Get-Date).AddDays(7).ToString("yyyy-MM-dd")
$slots = Invoke-RestMethod -Uri "http://localhost:7071/api/slots?personId=$personId&date=$date"

Write-Host "Dostępne sloty: $($slots.availableSlots -join ', ')"

# 3. Zarezerwuj pierwsze dostępne spotkanie
$booking = @{
    personId = $personId
    date = $date
    time = $slots.availableSlots[0]
    clientName = "Testowy Klient"
    clientEmail = "test@example.com"
    description = "Spotkanie testowe"
} | ConvertTo-Json

$appointment = Invoke-RestMethod -Uri "http://localhost:7071/api/appointments" `
    -Method POST -Body $booking -ContentType "application/json"

Write-Host "Zarezerwowano spotkanie: $($appointment.id)"

# 4. Sprawdź wszystkie spotkania
$allAppointments = Invoke-RestMethod -Uri "http://localhost:7071/api/appointments"
Write-Host "Liczba spotkań: $($allAppointments.Count)"
```

### Test 2: Weryfikacja konfliktów

```powershell
# Próba zarezerwowania tego samego slotu (powinno zwrócić błąd 409)
$booking2 = @{
    personId = $personId
    date = $date
    time = $slots.availableSlots[0]
    clientName = "Inny Klient"
    clientEmail = "inny@example.com"
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "http://localhost:7071/api/appointments" `
        -Method POST -Body $booking2 -ContentType "application/json"
} catch {
    Write-Host "Oczekiwany błąd: $_"
}
```

---

## 📊 Testowanie w Postman

### Kolekcja Postman

Stwórz nową kolekcję "Meetings API" z następującymi requestami:

1. **Add Person**
   - Method: POST
   - URL: `http://localhost:7071/api/persons`
   - Body: raw JSON

2. **Get Persons**
   - Method: GET
   - URL: `http://localhost:7071/api/persons`

3. **Get Available Slots**
   - Method: GET
   - URL: `http://localhost:7071/api/slots`
   - Params: `personId`, `date`

4. **Book Appointment**
   - Method: POST
   - URL: `http://localhost:7071/api/appointments`
   - Body: raw JSON

5. **Get Appointments**
   - Method: GET
   - URL: `http://localhost:7071/api/appointments`
   - Params: `personId` (optional)

---

## 🐛 Testowanie błędów

### Test 1: Nieprawidłowe dane (brak wymaganych pól)
```bash
curl -X POST http://localhost:7071/api/appointments \
  -H "Content-Type: application/json" \
  -d '{
    "date": "2026-02-20",
    "time": "10:00"
  }'
```

### Test 2: Nieistniejąca osoba
```bash
curl "http://localhost:7071/api/slots?personId=nieistniejace_id&date=2026-02-20"
```

### Test 3: Nieprawidłowy format daty
```bash
curl "http://localhost:7071/api/slots?personId=person_123&date=20-02-2026"
```

---

## ✅ Checklist testowania

- [ ] Dodanie osoby z domyślną dostępnością
- [ ] Dodanie osoby z niestandardową dostępnością
- [ ] Pobranie listy wszystkich osób
- [ ] Pobranie dostępnych terminów w dzień roboczy
- [ ] Pobranie dostępnych terminów w weekend (gdy osoba nie pracuje)
- [ ] Rezerwacja spotkania
- [ ] Próba rezerwacji tego samego terminu (konflikt)
- [ ] Pobranie wszystkich spotkań
- [ ] Pobranie spotkań konkretnej osoby
- [ ] Testowanie z nieprawidłowymi danymi

---

**Użyj tych przykładów do sprawdzenia czy API działa poprawnie! 🧪**
