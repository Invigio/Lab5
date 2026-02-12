# 🧪 Przykładowe dane testowe

## Scenario 1: Standardowy lekarz (pn-pt, 9-17)

```json
{
  "name": "Dr Jan Kowalski",
  "email": "jan.kowalski@przychodnia.pl",
  "workHours": {
    "Monday": { "start": "09:00", "end": "17:00", "enabled": true },
    "Tuesday": { "start": "09:00", "end": "17:00", "enabled": true },
    "Wednesday": { "start": "09:00", "end": "17:00", "enabled": true },
    "Thursday": { "start": "09:00", "end": "17:00", "enabled": true },
    "Friday": { "start": "09:00", "end": "17:00", "enabled": true },
    "Saturday": { "enabled": false },
    "Sunday": { "enabled": false }
  }
}
```

## Scenario 2: Konsultant z elastycznymi godzinami

```json
{
  "name": "Anna Nowak",
  "email": "anna.nowak@consulting.pl",
  "workHours": {
    "Monday": { "start": "10:00", "end": "18:00", "enabled": true },
    "Tuesday": { "start": "08:00", "end": "16:00", "enabled": true },
    "Wednesday": { "start": "12:00", "end": "20:00", "enabled": true },
    "Thursday": { "enabled": false },
    "Friday": { "start": "09:00", "end": "15:00", "enabled": true },
    "Saturday": { "start": "10:00", "end": "14:00", "enabled": true },
    "Sunday": { "enabled": false }
  }
}
```

## Scenario 3: Fizjoterapeuta (krótkie dyżury)

```json
{
  "name": "Piotr Wiśniewski",
  "email": "piotr.wisniewski@fizjo.pl",
  "workHours": {
    "Monday": { "start": "08:00", "end": "12:00", "enabled": true },
    "Tuesday": { "enabled": false },
    "Wednesday": { "start": "14:00", "end": "18:00", "enabled": true },
    "Thursday": { "enabled": false },
    "Friday": { "start": "08:00", "end": "12:00", "enabled": true },
    "Saturday": { "enabled": false },
    "Sunday": { "enabled": false }
  }
}
```

## Scenario 4: Psycholog (weekendy)

```json
{
  "name": "Dr Maria Kamińska",
  "email": "maria.kaminska@terapia.pl",
  "workHours": {
    "Monday": { "enabled": false },
    "Tuesday": { "enabled": false },
    "Wednesday": { "enabled": false },
    "Thursday": { "start": "16:00", "end": "20:00", "enabled": true },
    "Friday": { "start": "16:00", "end": "20:00", "enabled": true },
    "Saturday": { "start": "09:00", "end": "17:00", "enabled": true },
    "Sunday": { "start": "10:00", "end": "16:00", "enabled": true }
  }
}
```

## Przykładowe rezerwacje

### Rezerwacja 1
```json
{
  "personId": "PERSON_ID",
  "date": "2026-02-20",
  "time": "10:00",
  "clientName": "Jan Nowak",
  "clientEmail": "jan.nowak@example.com",
  "description": "Konsultacja ogólna"
}
```

### Rezerwacja 2
```json
{
  "personId": "PERSON_ID",
  "date": "2026-02-20",
  "time": "10:30",
  "clientName": "Ewa Kowalczyk",
  "clientEmail": "ewa.kowalczyk@example.com",
  "description": "Kontrola"
}
```

### Rezerwacja 3 (sobota)
```json
{
  "personId": "PERSON_ID",
  "date": "2026-02-21",
  "time": "11:00",
  "clientName": "Tomasz Zieliński",
  "clientEmail": "tomasz.zielinski@example.com",
  "description": "Pierwsze spotkanie"
}
```

## 📝 Skrypt testowy PowerShell

```powershell
# Funkcja pomocnicza do dodawania osoby
function Add-TestPerson {
    param(
        [string]$Name,
        [string]$Email,
        [hashtable]$WorkHours
    )

    $body = @{
        name = $Name
        email = $Email
        workHours = $WorkHours
    } | ConvertTo-Json -Depth 10

    $result = Invoke-RestMethod -Uri "http://localhost:7071/api/persons" `
        -Method POST -Body $body -ContentType "application/json"

    Write-Host "✅ Dodano: $Name (ID: $($result.id))" -ForegroundColor Green
    return $result.id
}

# Dodaj testowych lekarzy
$drKowalski = Add-TestPerson -Name "Dr Jan Kowalski" -Email "jan.kowalski@przychodnia.pl" -WorkHours @{
    Monday = @{ start = "09:00"; end = "17:00"; enabled = $true }
    Tuesday = @{ start = "09:00"; end = "17:00"; enabled = $true }
    Wednesday = @{ start = "09:00"; end = "17:00"; enabled = $true }
    Thursday = @{ start = "09:00"; end = "17:00"; enabled = $true }
    Friday = @{ start = "09:00"; end = "17:00"; enabled = $true }
    Saturday = @{ enabled = $false }
    Sunday = @{ enabled = $false }
}

$annaNowak = Add-TestPerson -Name "Anna Nowak" -Email "anna.nowak@consulting.pl" -WorkHours @{
    Monday = @{ start = "10:00"; end = "18:00"; enabled = $true }
    Tuesday = @{ start = "08:00"; end = "16:00"; enabled = $true }
    Wednesday = @{ start = "12:00"; end = "20:00"; enabled = $true }
    Thursday = @{ enabled = $false }
    Friday = @{ start = "09:00"; end = "15:00"; enabled = $true }
    Saturday = @{ start = "10:00"; end = "14:00"; enabled = $true }
    Sunday = @{ enabled = $false }
}

Write-Host "`n📋 Lista wszystkich osób:" -ForegroundColor Cyan
$persons = Invoke-RestMethod -Uri "http://localhost:7071/api/persons"
$persons | ForEach-Object { Write-Host "  - $($_.name) ($($_.email))" }

Write-Host "`n📅 Sprawdzanie dostępności Dr Kowalskiego na 2026-02-20:" -ForegroundColor Cyan
$availability = Invoke-RestMethod -Uri "http://localhost:7071/api/availability/$drKowalski?date=2026-02-20"
Write-Host "Dostępne sloty: $($availability.availableSlots -join ', ')"

Write-Host "`n🎯 Rezerwacja pierwszego dostępnego slotu:" -ForegroundColor Cyan
$booking = @{
    personId = $drKowalski
    date = "2026-02-20"
    time = $availability.availableSlots[0]
    clientName = "Jan Testowy"
    clientEmail = "jan.testowy@example.com"
    description = "Spotkanie testowe"
} | ConvertTo-Json

$bookingResult = Invoke-RestMethod -Uri "http://localhost:7071/api/book" `
    -Method POST -Body $booking -ContentType "application/json"

Write-Host "✅ Zarezerwowano: $($bookingResult.date) $($bookingResult.time)" -ForegroundColor Green

Write-Host "`n🔄 Ponowne sprawdzenie dostępności:" -ForegroundColor Cyan
$availability2 = Invoke-RestMethod -Uri "http://localhost:7071/api/availability/$drKowalski?date=2026-02-20"
Write-Host "Dostępne sloty: $($availability2.availableSlots -join ', ')"
Write-Host "(Pierwszy slot został zarezerwowany)" -ForegroundColor Yellow
```

---

**Użyj tych danych do testowania systemu! 🧪**
