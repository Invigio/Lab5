# ⚡ Szybki Start - Lab 5-6

## Komendy do wykonania po kolei

### 1. Inicjalizacja projektu Azure Functions
```powershell
# Utwórz folder api i przejdź do niego
mkdir api
cd api

# Inicjalizuj projekt Functions (JavaScript)
func init . --javascript

# Zainstaluj Cosmos DB SDK
npm install @azure/cosmos
```

### 2. Konfiguracja Cosmos DB

Edytuj `api/local.settings.json`:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "node",
    "CosmosDBConnection": "AccountEndpoint=https://TWOJA-COSMOS-DB.documents.azure.com:443/;AccountKey=TWOJ-ACCOUNT-KEY-TUTAJ=="
  },
  "Host": {
    "CORS": "*",
    "CORSCredentials": false
  }
}
```

### 3. Tworzenie funkcji API

```powershell
# W folderze api/ wykonaj:
func new --name AddPerson --template "HTTP trigger" --authlevel "anonymous"
func new --name GetPersons --template "HTTP trigger" --authlevel "anonymous"
func new --name GetAvailableSlots --template "HTTP trigger" --authlevel "anonymous"
func new --name BookAppointment --template "HTTP trigger" --authlevel "anonymous"
func new --name GetAppointments --template "HTTP trigger" --authlevel "anonymous"
```

**Następnie edytuj każdą funkcję zgodnie z kodem w INSTRUKCJA.md (sekcja 4)**

### 4. Przygotowanie Cosmos DB w Azure Portal

1. Przejdź do: https://portal.azure.com
2. Znajdź swoją bazę Cosmos DB: `umawianie-spotkan-db-twojeinicjaly`
3. Stwórz bazę danych: **MeetingsDB**
4. Stwórz 2 kontenery:
   - **Persons** - Partition key: `/id`
   - **Appointments** - Partition key: `/personId`

### 5. Uruchomienie backendu

```powershell
cd api
func start
```

Backend będzie działać na: http://localhost:7071

### 6. Przygotowanie frontendu

```powershell
# Wróć do głównego folderu i stwórz frontend
cd ..
mkdir frontend
cd frontend

# Stwórz 3 pliki:
# - index.html (kod w INSTRUKCJA.md sekcja 5.2)
# - styles.css (kod w INSTRUKCJA.md sekcja 5.3)
# - app.js (kod w INSTRUKCJA.md sekcja 5.4)
```

### 7. Uruchomienie frontendu

**Opcja A - Python (jeśli masz Pythona):**
```powershell
python -m http.server 8000
```

**Opcja B - Live Server w VS Code:**
1. Zainstaluj rozszerzenie "Live Server"
2. Kliknij prawym na `index.html` → "Open with Live Server"

**Opcja C - Node.js http-server:**
```powershell
npm install -g http-server
http-server -p 8000
```

Frontend będzie dostępny na: http://localhost:8000

### 8. Testowanie

1. **Otwórz:** http://localhost:8000
2. **Dodaj osobę** z konfiguracją dostępności
3. **Wybierz osobę i datę** aby zobaczyć wolne terminy
4. **Zarezerwuj spotkanie**
5. **Sprawdź listę spotkań** na dole strony

---

## 📋 Checklist implementacji

- [ ] Azure Functions utworzone i skonfigurowane
- [ ] Cosmos DB: baza MeetingsDB + 2 kontenery
- [ ] Funkcja AddPerson - działa
- [ ] Funkcja GetPersons - działa
- [ ] Funkcja GetAvailableSlots - działa
- [ ] Funkcja BookAppointment - działa
- [ ] Funkcja GetAppointments - działa
- [ ] Frontend: index.html, styles.css, app.js
- [ ] Dodawanie osób z dostępnością - działa
- [ ] Wyświetlanie dostępnych terminów - działa
- [ ] Rezerwacja spotkań - działa
- [ ] Lista spotkań - wyświetla się

---

## 🚨 Najczęstsze błędy

### "Cannot find module '@azure/cosmos'"
```powershell
cd api
npm install @azure/cosmos
```

### "CORS error" w przeglądarce
Dodaj w `api/local.settings.json`:
```json
"Host": {
  "CORS": "*"
}
```
Zrestartuj `func start`

### "Database/Container not found"
Sprawdź w Azure Portal czy stworzyłeś:
- Bazę: `MeetingsDB`
- Kontenery: `Persons` i `Appointments`

### Frontend nie łączy się z API
Sprawdź w `frontend/app.js` czy masz:
```javascript
const API_BASE_URL = 'http://localhost:7071/api';
```

---

## 📁 Potrzebne pliki

### Pliki do skopiowania z INSTRUKCJA.md:

1. **api/AddPerson/index.js** - sekcja 4.1
2. **api/AddPerson/function.json** - sekcja 4.1
3. **api/GetPersons/index.js** - sekcja 4.2
4. **api/GetPersons/function.json** - sekcja 4.2
5. **api/GetAvailableSlots/index.js** - sekcja 4.3
6. **api/GetAvailableSlots/function.json** - sekcja 4.3
7. **api/BookAppointment/index.js** - sekcja 4.4
8. **api/BookAppointment/function.json** - sekcja 4.4
9. **api/GetAppointments/index.js** - sekcja 4.5
10. **api/GetAppointments/function.json** - sekcja 4.5
11. **frontend/index.html** - sekcja 5.2
12. **frontend/styles.css** - sekcja 5.3
13. **frontend/app.js** - sekcja 5.4

---

## ⏱️ Szacowany czas realizacji

- **Konfiguracja środowiska:** 15 min
- **Implementacja API:** 45-60 min
- **Implementacja frontendu:** 30 min
- **Testowanie:** 15 min
- **Wdrożenie (opcjonalne):** 30 min

**RAZEM:** ~2-2.5 godziny

---

## 🎯 Wymagania na oceny

### Ocena 4.0 (podstawowa):
- ✅ Dodawanie osób (domyślna dostępność pn-pt 8-16)
- ✅ Lista osób
- ✅ Dostępne terminy
- ✅ Rezerwacja spotkań
- ✅ Lista spotkań

### Ocena 5.0 (rozszerzona):
- ✅ Wszystko z oceny 4.0
- ✅ **Konfigurowalna dostępność dla każdej osoby**
- ✅ Wybór dni tygodnia
- ✅ Wybór godzin pracy
- ✅ Frontend z formularzem dostępności

---

**Gotowy? Zacznij od kroku 1! 🚀**

Jeśli masz problemy, sprawdź pełną INSTRUKCJA.md
