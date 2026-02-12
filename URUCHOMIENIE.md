# 🚀 URUCHOMIENIE SYSTEMU - Pełna instrukcja

## ✅ Co zostało utworzone:

1. **Backend API** (C# .NET 8) - folder `SpotkaniaAPI/`
2. **Frontend** (HTML/CSS/JS) - folder `frontend/`
3. **Baza danych** - Azure Cosmos DB (w chmurze)

---

## 📋 Krok 1: Przygotowanie

### Sprawdź czy masz zainstalowane:
```powershell
dotnet --version        # Wymagane: 8.0+
func --version          # Wymagane: 4.x (już zainstalowane ✅)
python --version        # Do uruchomienia frontendu
```

### Sprawdź Cosmos DB (Azure Portal):
1. Przejdź do: https://portal.azure.com
2. Znajdź: `umawianie-spotkan-db-twojeinicjaly`
3. Data Explorer → sprawdź czy istnieje:
   - Baza: **SpotkaniaDB**
   - Kontener: **Persons** (partition key: `/id`)

Jeśli nie istnieje, utwórz je!

---

## 🎯 Krok 2: Uruchomienie BACKENDU (API)

### Terminal 1 - Azure Functions:

```powershell
# Przejdź do folderu API
cd "C:\Users\norbe\Desktop\Magisterka\Semestr 2\Programowanie w Chmurze Obliczeniowej\Lab5\SpotkaniaAPI"

# Uruchom Functions
func start
```

**Poczekaj aż zobaczysz:**
```
Functions:
    AddPerson: [POST] http://localhost:7071/api/persons
    GetAllPersons: [GET] http://localhost:7071/api/persons
    GetPersonById: [GET] http://localhost:7071/api/persons/{id}
    GetAvailability: [GET] http://localhost:7071/api/availability/{id}
    BookAppointment: [POST] http://localhost:7071/api/book
```

✅ **Backend działa!** API dostępne na: http://localhost:7071

---

## 🌐 Krok 3: Uruchomienie FRONTENDU

### Terminal 2 - Strona webowa:

```powershell
# Przejdź do folderu frontend
cd "C:\Users\norbe\Desktop\Magisterka\Semestr 2\Programowanie w Chmurze Obliczeniowej\Lab5\frontend"

# Uruchom serwer HTTP (Python)
python -m http.server 8000
```

**Alternatywa (Node.js):**
```powershell
npx http-server -p 8000
```

**Alternatywa (VS Code):**
1. Zainstaluj rozszerzenie "Live Server"
2. Kliknij prawym na `index.html` → "Open with Live Server"

✅ **Frontend działa!** Strona dostępna na: http://localhost:8000

---

## 🎉 Krok 4: Użycie systemu

### Otwórz w przeglądarce:
```
http://localhost:8000
```

### Funkcje dostępne na stronie:

#### 1️⃣ **Dodaj osobę** (pierwsza sekcja)
- Wypełnij imię, nazwisko, email
- Zaznacz dni tygodnia i godziny pracy
- Kliknij "Dodaj osobę"

#### 2️⃣ **Umów spotkanie** (druga sekcja)
- Wybierz osobę z listy
- Wybierz datę
- Wybierz dostępną godzinę z siatki
- Wpisz swoje dane
- Kliknij "Zarezerwuj termin"

#### 3️⃣ **Zobacz osoby** (trzecia sekcja)
- Lista wszystkich osób w systemie
- Dni przyjęć i liczba spotkań

#### 4️⃣ **Zobacz spotkania** (czwarta sekcja)
- Lista wszystkich zarezerwowanych spotkań
- Filtrowanie według osoby

---

## 🧪 Szybki test

### Test 1: Dodaj przykładową osobę

Na stronie w pierwszej sekcji:
- **Imię:** Dr Jan Kowalski
- **Email:** jan.kowalski@test.pl
- **Dni:** Zaznacz Pn-Pt, 09:00-17:00
- Kliknij "Dodaj osobę"

### Test 2: Umów spotkanie

W drugiej sekcji:
- **Osoba:** Dr Jan Kowalski
- **Data:** Wybierz dzisiejszą datę + kilka dni
- **Godzina:** Kliknij na dowolny slot (np. 10:00)
- **Twoje dane:** Jan Nowak, jan.nowak@test.com
- Kliknij "Zarezerwuj termin"

### Test 3: Sprawdź listę

Przewiń w dół - powinieneś zobaczyć:
- Dr Jana Kowalskiego w sekcji "Osoby w systemie"
- Swoje spotkanie w sekcji "Twoje spotkania"

---

## 📸 Jak to wygląda:

```
┌─────────────────────────────────────┐
│  📅 System Umawiania Spotkań        │
│  Azure Functions + Cosmos DB        │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ ➕ Dodaj osobę do systemu           │
│                                     │
│ Imię: [Dr Jan Kowalski        ]    │
│ Email: [jan@test.pl           ]    │
│                                     │
│ ⏰ Godziny pracy:                   │
│ ☑ Poniedziałek  [09:00] - [17:00] │
│ ☑ Wtorek        [09:00] - [17:00] │
│ ...                                 │
│                                     │
│ [     Dodaj osobę     ]            │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ 📆 Umów spotkanie                   │
│                                     │
│ Osoba: [Dr Jan Kowalski      ▼]   │
│ Data:  [2026-02-20           ]     │
│                                     │
│ Dostępne godziny:                   │
│ [09:00] [09:30] [10:00] [10:30]   │
│ [11:00] [11:30] [12:00] [12:30]   │
│ ...                                 │
│                                     │
│ Twoje imię: [Jan Nowak        ]    │
│ Email: [jan.nowak@test.com    ]    │
│                                     │
│ [   Zarezerwuj termin   ]          │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ 👥 Osoby w systemie                │
│                                     │
│ 📌 Dr Jan Kowalski                 │
│    jan.kowalski@test.pl            │
│    Spotkań: 5                       │
│    Pn Wt Śr Cz Pt                  │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ 📋 Twoje spotkania                  │
│                                     │
│ 📅 czwartek, 20 lutego 2026        │
│ Godzina: 10:00                      │
│ Z: Dr Jan Kowalski                  │
│ Klient: Jan Nowak                   │
└─────────────────────────────────────┘
```

---

## 🔧 Rozwiązywanie problemów

### Problem: Backend nie startuje
```powershell
cd SpotkaniaAPI
dotnet clean
dotnet build
func start
```

### Problem: CORS error w przeglądarce
Sprawdź czy:
1. Backend działa na http://localhost:7071
2. W pliku `app.js` jest: `API_BASE_URL = 'http://localhost:7071/api'`

### Problem: Baza danych nie odpowiada
1. Sprawdź Azure Portal czy kontener istnieje
2. Sprawdź connection string w `local.settings.json`

### Problem: Nie widać dostępnych slotów
1. Sprawdź czy wybrałeś osobę i datę
2. Sprawdź czy osoba pracuje w tym dniu (np. sobota/niedziela)
3. Otwórz DevTools (F12) → Console i sprawdź błędy

---

## 🎯 Wymagania spełnione (ocena 5.0)

### Backend (C# .NET 8):
✅ Model `Person` z słownikiem `WorkHours`
✅ POST /api/persons - dodawanie osób
✅ GET /api/availability/{id}?date - wolne sloty co 30 min
✅ POST /api/book - rezerwacja z walidacją
✅ Azure Cosmos DB z CosmosClient

### Frontend:
✅ Dodawanie osób przez formularz webowy
✅ Konfigurowalne godziny pracy (każdy dzień osobno)
✅ Wybór osoby i daty
✅ Wyświetlanie dostępnych slotów
✅ Rezerwacja spotkań
✅ Lista osób i spotkań
✅ Responsive design

---

## 📝 Podsumowanie

**Terminal 1:** `func start` (w folderze SpotkaniaAPI)
**Terminal 2:** `python -m http.server 8000` (w folderze frontend)
**Przeglądarka:** http://localhost:8000

**System działa lokalnie, ale używa Cosmos DB w Azure! ☁️**

---

**Powodzenia! 🚀**
