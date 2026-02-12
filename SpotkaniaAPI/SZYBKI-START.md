# ⚡ Szybki start - C# .NET 8 Azure Functions

## 📋 Przed rozpoczęciem

Sprawdź czy masz zainstalowane:
```powershell
dotnet --version          # Wymagane: 8.0 lub nowsze
func --version            # Wymagane: 4.x
```

## 🚀 Kroki uruchomienia

### 1. Konfiguracja Cosmos DB (Azure Portal)

1. Przejdź do: https://portal.azure.com
2. Otwórz swoje konto Cosmos DB: `umawianie-spotkan-db-twojeinicjaly`
3. Data Explorer → New Database:
   - **Database id:** `SpotkaniaDB`
4. New Container:
   - **Container id:** `Persons`
   - **Partition key:** `/id`
   - **Throughput:** 400 RU/s (Manual)

### 2. Sprawdź konfigurację

Plik `local.settings.json` powinien zawierać:
```json
{
  "Values": {
    "CosmosDbConnectionString": "AccountEndpoint=https://TWOJA-COSMOS-DB.documents.azure.com:443/;AccountKey=TWOJ-ACCOUNT-KEY-TUTAJ==",
    "DatabaseName": "SpotkaniaDB",
    "ContainerName": "Persons"
  }
}
```

### 3. Uruchomienie projektu

```powershell
# Przejdź do folderu projektu
cd SpotkaniaAPI

# Przywróć pakiety NuGet
dotnet restore

# Zbuduj projekt
dotnet build

# Uruchom Azure Functions
func start
```

✅ API będzie dostępne na: **http://localhost:7071**

## 🧪 Test funkcjonalności

### Szybki test w PowerShell:

```powershell
# 1. Dodaj osobę
$person = @{
    name = "Dr Jan Kowalski"
    email = "jan.kowalski@test.pl"
    workHours = @{
        Monday = @{ start = "09:00"; end = "17:00"; enabled = $true }
        Tuesday = @{ start = "09:00"; end = "17:00"; enabled = $true }
        Wednesday = @{ start = "09:00"; end = "17:00"; enabled = $true }
        Thursday = @{ start = "09:00"; end = "17:00"; enabled = $true }
        Friday = @{ start = "09:00"; end = "17:00"; enabled = $true }
        Saturday = @{ enabled = $false }
        Sunday = @{ enabled = $false }
    }
} | ConvertTo-Json -Depth 10

$result = Invoke-RestMethod -Uri "http://localhost:7071/api/persons" `
    -Method POST -Body $person -ContentType "application/json"

Write-Host "✅ Utworzono osobę o ID: $($result.id)"
$personId = $result.id

# 2. Sprawdź dostępność
$date = (Get-Date).AddDays(7).ToString("yyyy-MM-dd")
$slots = Invoke-RestMethod -Uri "http://localhost:7071/api/availability/$personId?date=$date"
Write-Host "📅 Dostępne sloty na $date`:"
$slots.availableSlots | ForEach-Object { Write-Host "  - $_" }

# 3. Zarezerwuj spotkanie
$booking = @{
    personId = $personId
    date = $date
    time = "10:00"
    clientName = "Jan Testowy"
    clientEmail = "jan@test.com"
    description = "Test"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/book" `
    -Method POST -Body $booking -ContentType "application/json"

Write-Host "✅ Zarezerwowano spotkanie!"

# 4. Sprawdź ponownie dostępność
$slots2 = Invoke-RestMethod -Uri "http://localhost:7071/api/availability/$personId?date=$date"
Write-Host "📅 Dostępne sloty po rezerwacji:"
$slots2.availableSlots | ForEach-Object { Write-Host "  - $_" }
```

## 📡 Dostępne endpointy

| Method | Endpoint | Opis |
|--------|----------|------|
| POST | `/api/persons` | Dodaj osobę |
| GET | `/api/persons` | Lista osób |
| GET | `/api/persons/{id}` | Szczegóły osoby |
| GET | `/api/availability/{id}?date=YYYY-MM-DD` | Dostępne sloty |
| POST | `/api/book` | Zarezerwuj spotkanie |

## 🐛 Rozwiązywanie problemów

### Błąd: "Cannot find module"
```powershell
dotnet restore
dotnet build
```

### Błąd: "Cosmos DB unauthorized"
- Sprawdź connection string w `local.settings.json`
- Upewnij się że baza i kontener istnieją

### Błąd: "Function doesn't start"
```powershell
# Wyczyść i zbuduj ponownie
dotnet clean
dotnet build
func start --verbose
```

### Port 7071 zajęty
```powershell
func start --port 7072
```

## ✅ Checklist

- [ ] .NET 8 SDK zainstalowany
- [ ] Azure Functions Core Tools v4 zainstalowany
- [ ] Cosmos DB: baza `SpotkaniaDB` utworzona
- [ ] Cosmos DB: kontener `Persons` utworzony (partition key: `/id`)
- [ ] `local.settings.json` skonfigurowany
- [ ] `dotnet restore` wykonane
- [ ] `dotnet build` powiedzie się
- [ ] `func start` działa bez błędów
- [ ] POST /api/persons - dodawanie osoby działa
- [ ] GET /api/availability/{id}?date=YYYY-MM-DD - zwraca sloty
- [ ] POST /api/book - rezerwacja działa

## 📚 Dodatkowe pliki

- **README.md** - Pełna dokumentacja projektu
- **TESTOWE-DANE.md** - Przykładowe dane do testowania

---

**Gotowy? Wpisz `func start` i zaczynaj! 🚀**
