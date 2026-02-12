# Lab 5-6: System do umawiania spotkań

## 🎯 Zadanie

System do rezerwacji spotkań wykorzystujący:
- **Azure Functions** - Serverless API (C# .NET 8)
- **Azure Cosmos DB** - NoSQL baza danych
- **Frontend webowy** - Interfejs HTML/CSS/JavaScript

## ✨ Realizacja (ocena 5.0)

✅ **Backend API** (C# .NET 8 Isolated)
- Model `Person` z słownikiem `WorkHours` (dni + godziny pracy)
- POST /api/persons - dodawanie osób
- GET /api/availability/{id}?date - wolne sloty co 30 minut
- POST /api/book - rezerwacja z walidacją
- Azure Cosmos DB z CosmosClient

✅ **Frontend webowy**
- Dodawanie osób przez formularz
- Konfiguracja dni i godzin pracy (każdy dzień osobno)
- Wybór osoby i daty
- Wyświetlanie dostępnych slotów
- Rezerwacja spotkań
- Lista osób i spotkań

## 🚀 JAK URUCHOMIĆ?

### ⚡ Lokalnie (development)

**Terminal 1 - Backend:**
```powershell
cd SpotkaniaAPI
func start
```

**Terminal 2 - Frontend:**
```powershell
cd frontend
python -m http.server 8000
```

**Przeglądarka:** http://localhost:8000

📖 **Pełna instrukcja lokalna:** [URUCHOMIENIE.md](URUCHOMIENIE.md)

---

### ☁️ Wdrożenie na Azure (produkcja)

🚀 **[DEPLOY.md](DEPLOY.md)** - Kompletna instrukcja krok po kroku

**Co otrzymasz:**
- ✅ Publiczny URL działający 24/7
- ✅ Automatyczne wdrożenie przez GitHub Actions
- ✅ Frontend + Backend w Azure Static Web Apps
- ✅ Darmowy hosting (Free tier)

**Szybki start wdrożenia:**
1. Wrzuć kod na GitHub
2. Utwórz Azure Static Web App
3. Połącz z repozytorium
4. GitHub Actions automatycznie wdroży!

**➡️ [Przejdź do pełnej instrukcji wdrożenia](DEPLOY.md)**

## 📁 Struktura projektu

```
Lab5/
├── SpotkaniaAPI/          ⭐ Backend (C# .NET 8)
│   ├── Models/            - Person, WorkDay, BookedSlot
│   ├── Functions/         - AddPerson, GetAvailability, Book
│   ├── Program.cs         - Konfiguracja DI + Cosmos DB
│   ├── README.md          - Dokumentacja backendu
│   └── SZYBKI-START.md    - Instrukcje C#
│
├── frontend/              🌐 Frontend (HTML/CSS/JS)
│   ├── index.html         - Strona główna
│   ├── styles.css         - Stylizacja
│   └── app.js             - Logika aplikacji
│
├── URUCHOMIENIE.md        📖 Start lokalny
├── DEPLOY.md              🚀 Wdrożenie na Azure
├── README-GLOWNY.md       📚 Pełna dokumentacja
└── README.md              ℹ️ Ten plik
```

## 🔗 API Endpoints

| Metoda | Endpoint | Opis |
|--------|----------|------|
| POST | `/api/persons` | Dodaj osobę z harmonogramem |
| GET | `/api/persons` | Pobierz wszystkie osoby |
| GET | `/api/persons/{id}` | Pobierz konkretną osobę |
| GET | `/api/availability/{id}?date=YYYY-MM-DD` | Dostępne sloty (30 min) |
| POST | `/api/book` | Zarezerwuj spotkanie |

## 📋 Wymagania

- **.NET 8 SDK**
- **Azure Functions Core Tools v4**
- **Azure Cosmos DB account** (connection string w local.settings.json)
- **Python 3.x** (do serwowania frontendu)
- **Przeglądarka** (Chrome/Edge/Firefox)

## 🎓 Autor

Laboratorium z przedmiotu: **Programowanie w Chmurze Obliczeniowej**
Semestr 2, Magisterka

---

**Powodzenia! 🚀**
