# Lab 5-6: System do umawiania spotkań

## 🎯 Cel projektu

System do rezerwacji spotkań wykorzystujący:
- **Azure Functions** (Serverless API)
- **Azure Cosmos DB** (NoSQL)
- **Frontend webowy** (opcjonalnie)

## 📁 Struktura projektu

### ⭐ SpotkaniaAPI/ - **Backend API (C# .NET 8)**

Kompletna implementacja w C# .NET 8 z Azure Functions Isolated Worker.

**✅ Pełne wymagania na ocenę 5.0:**
- Model `Person` z słownikiem `WorkHours`
- POST /api/persons - dodawanie osób z indywidualnym grafikiem
- GET /api/availability/{id}?date=YYYY-MM-DD - wolne sloty co 30 min
- POST /api/book - rezerwacja z walidacją
- Pełna obsługa Cosmos DB z CosmosClient

📖 **[Dokumentacja C# → SpotkaniaAPI/README.md](SpotkaniaAPI/README.md)**

### 🌐 frontend/ - **Frontend webowy (HTML/CSS/JS)**

Pełna strona internetowa do zarządzania spotkaniami:
- ✅ Dodawanie osób z konfiguracją dni i godzin pracy
- ✅ Wyświetlanie dostępnych slotów czasowych
- ✅ Rezerwacja spotkań przez formularz
- ✅ Lista osób i spotkań
- ✅ Filtry i wyszukiwanie
- ✅ Responsywny design

**Pliki:**
- `index.html` - Struktura strony
- `styles.css` - Stylizacja (gradient, karty, animacje)
- `app.js` - Logika (fetch API, obsługa formularzy)

### 📂 api/ - Node.js/JavaScript (wersja alternatywna backendu)

Implementacja w JavaScript dla tych, którzy preferują Node.js.

📖 **[Dokumentacja JavaScript → INSTRUKCJA.md](INSTRUKCJA.md)**

## 🚀 Szybki start - PEŁNY SYSTEM

### 📋 Wymagania
- .NET 8 SDK
- Azure Functions Core Tools v4 ✅ (już zainstalowane)
- Python 3.x (do frontendu)
- Azure Cosmos DB account

### 1️⃣ Konfiguracja Cosmos DB (Azure Portal)
```
Baza danych: SpotkaniaDB
Kontener: Persons (partition key: /id)
```

### 2️⃣ Uruchomienie BACKENDU (Terminal 1)
```powershell
cd SpotkaniaAPI
func start
```
✅ API na: **http://localhost:7071**

### 3️⃣ Uruchomienie FRONTENDU (Terminal 2)
```powershell
cd frontend
python -m http.server 8000
```
✅ Strona na: **http://localhost:8000**

### 4️⃣ Otwórz w przeglądarce
```
http://localhost:8000
```

🎉 **System gotowy do użycia!**

📖 **[Pełna instrukcja → URUCHOMIENIE.md](URUCHOMIENIE.md)**

## 📡 API Endpoints

| Method | Endpoint | Opis |
|--------|----------|------|
| POST | `/api/persons` | Dodaj osobę z grafikiem |
| GET | `/api/persons` | Lista wszystkich osób |
| GET | `/api/persons/{id}` | Szczegóły osoby |
| GET | `/api/availability/{id}?date=YYYY-MM-DD` | Wolne sloty 30-min |
| POST | `/api/book` | Zarezerwuj spotkanie |

## 🎓 Wymagania na oceny

### Ocena 4.0 (podstawowa):
- ✅ Dodawanie osób (standardowy grafik 8-16)
- ✅ Lista osób
- ✅ Dostępne terminy
- ✅ Rezerwacja spotkań

### Ocena 5.0 (rozszerzona):
- ✅ Wszystko z wersji 4.0
- ✅ **Model z słownikiem WorkHours**
- ✅ **Indywidualny grafik dla każdego dnia**
- ✅ **Generowanie slotów 30-minutowych**
- ✅ **Walidacja dostępności przy rezerwacji**

## 🔗 Connection String Cosmos DB

```
AccountEndpoint=https://TWOJA-COSMOS-DB.documents.azure.com:443/;
AccountKey=TWOJ-ACCOUNT-KEY-TUTAJ==;
```

## 📚 Dokumentacja

### C# .NET 8 (zalecana):
- **[SpotkaniaAPI/README.md](SpotkaniaAPI/README.md)** - Pełna dokumentacja
- **[SpotkaniaAPI/SZYBKI-START.md](SpotkaniaAPI/SZYBKI-START.md)** - Szybkie uruchomienie
- **[SpotkaniaAPI/TESTOWE-DANE.md](SpotkaniaAPI/TESTOWE-DANE.md)** - Przykładowe dane

### JavaScript/Node.js:
- **[INSTRUKCJA.md](INSTRUKCJA.md)** - Pełna instrukcja
- **[SZYBKI-START.md](SZYBKI-START.md)** - Szybkie komendy
- **[TESTOWANIE-API.md](TESTOWANIE-API.md)** - Przykłady requestów

### Ogólne:
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - Rozwiązywanie problemów
- **[SCIAGAWKA.md](SCIAGAWKA.md)** - Ściąga (JavaScript)

## 🧪 Przykład użycia (C#)

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

$personId = $result.id

# 2. Sprawdź dostępność
$slots = Invoke-RestMethod -Uri "http://localhost:7071/api/availability/$personId?date=2026-02-20"
Write-Host "Dostępne sloty: $($slots.availableSlots -join ', ')"

# 3. Zarezerwuj
$booking = @{
    personId = $personId
    date = "2026-02-20"
    time = "10:00"
    clientName = "Jan Nowak"
    clientEmail = "jan.nowak@example.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/book" `
    -Method POST -Body $booking -ContentType "application/json"
```

## 🛠️ Technologie

### C# .NET 8:
- Microsoft.Azure.Functions.Worker v1.21.0
- Microsoft.Azure.Cosmos v3.38.1
- .NET 8.0
- Azure Functions Runtime v4

### JavaScript/Node.js:
- @azure/cosmos
- Azure Functions Node.js v4
- Node.js 18+

## 📞 Pomoc

Jeśli masz problemy:
1. Sprawdź [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
2. Sprawdź [SpotkaniaAPI/README.md](SpotkaniaAPI/README.md)
3. Sprawdź logi w terminalu (func start --verbose)

## 📝 Autor

Projekt edukacyjny dla przedmiotu: **Programowanie w Chmurze Obliczeniowej**
Semestr 2, Magisterka

---

**Zalecam użycie wersji C# .NET 8 (folder SpotkaniaAPI/) - pełna implementacja wymagań! 🚀**
