# ✅ Checklist przed wdrożeniem na Azure

## 1. Test lokalny - Backend (Azure Functions)

### Sprawdź czy backend uruchamia się:

```powershell
cd SpotkaniaAPI
func start
```

**Oczekiwany wynik:**
```
Azure Functions Core Tools
Core Tools Version: 4.x.x
Function Runtime Version: 8.0.x

Functions:
  AddPersonFunction: [POST] http://localhost:7071/api/persons
  GetPersonsFunction: [GET] http://localhost:7071/api/persons
  GetAvailabilityFunction: [GET] http://localhost:7071/api/availability/{id}
  BookAppointmentFunction: [POST] http://localhost:7071/api/book
```

### Test API w PowerShell:

```powershell
# Test dodawania osoby
$person = @{
    name = "Jan Kowalski"
    email = "jan@email.com"
    workHours = @{
        Monday = @{ start = "09:00"; end = "17:00"; enabled = $true }
        Tuesday = @{ start = "09:00"; end = "17:00"; enabled = $true }
    }
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri "http://localhost:7071/api/persons" -Method POST -Body $person -ContentType "application/json"

# Test pobierania osób
Invoke-RestMethod -Uri "http://localhost:7071/api/persons" -Method GET
```

**✅ Jeśli dostałeś odpowiedź JSON - backend działa!**

---

## 2. Test lokalny - Frontend

### Uruchom frontend:

```powershell
cd frontend
python -m http.server 8000
```

**Otwórz:** http://localhost:8000

### Sprawdź DevTools (F12):

1. Przejdź do zakładki **Console**
2. Sprawdź czy NIE MA błędów CORS
3. Przejdź do zakładki **Network**
4. Spróbuj dodać osobę
5. Sprawdź czy request do `/api/persons` ma status **200 OK**

**✅ Jeśli widzisz 200 OK - frontend komunikuje się z backend!**

---

## 3. Test Cosmos DB Connection

### Sprawdź czy dane zapisują się w Cosmos DB:

1. Dodaj osobę przez frontend
2. Idź na Azure Portal → Twoja Cosmos DB
3. **Data Explorer** → **SpotkaniaDB** → **Persons** → **Items**
4. Powinieneś zobaczyć nowy dokument JSON

**✅ Jeśli widzisz dane - Cosmos DB działa!**

---

## 4. Test pełnegoFlow

### Scenariusz testowy:

1. **Dodaj osobę**
   - Imię: Test User
   - Email: test@email.com
   - Włącz Monday: 09:00 - 17:00
   - Włącz Tuesday: 10:00 - 16:00
   - Kliknij "Dodaj osobę"

2. **Sprawdź dostępność**
   - Wybierz "Test User" z listy
   - Wybierz datę (poniedziałek lub wtorek)
   - Kliknij "Sprawdź dostępność"
   - Powinny pojawić się sloty co 30 minut

3. **Zarezerwuj spotkanie**
   - Wybierz slot (np. 10:00)
   - Wypełnij: Client Name, Email
   - Kliknij "Zarezerwuj"
   - Powinieneś zobaczyć toast "Spotkanie zarezerwowane!"

4. **Sprawdź listę**
   - Kliknij "Odśwież listę"
   - W sekcji "Zarezerwowane spotkania" powinieneś zobaczyć swoje spotkanie

**✅ Jeśli wszystko zadziałało - aplikacja gotowa do wdrożenia!**

---

## 5. Przygotuj connection string dla Azure

### Skopiuj swój Cosmos DB connection string:

1. Azure Portal → Twoja Cosmos DB
2. **Keys** w menu po lewej
3. Skopiuj **PRIMARY CONNECTION STRING**
4. Zapisz w notatniku - będzie potrzebny w Azure Static Web App Configuration

**Format:**
```
AccountEndpoint=https://xxx.documents.azure.com:443/;AccountKey=xxx==
```

---

## ✅ GOTOWOŚĆ DO WDROŻENIA

Jeśli wszystkie punkty przeszły pomyślnie:

1. ✅ Backend startuje bez błędów
2. ✅ API odpowiada na requesty
3. ✅ Frontend ładuje się poprawnie
4. ✅ Cosmos DB zapisuje i odczytuje dane
5. ✅ Cały flow działa end-to-end
6. ✅ Masz connection string do Cosmos DB

**➡️ Możesz przejść do [DEPLOY.md](DEPLOY.md) lub [QUICK-DEPLOY.md](QUICK-DEPLOY.md)**

---

## 🐛 Typowe problemy

### Problem: func start nie działa

**Rozwiązanie:**
```powershell
dotnet build
# Jeśli są błędy - napraw je przed uruchomieniem
```

### Problem: Frontend nie łączy się z API

**Rozwiązanie:**
- Sprawdź w `frontend/app.js` czy `API_BASE_URL` to `'http://localhost:7071/api'`
- Upewnij się, że backend działa (func start)

### Problem: Cosmos DB connection error

**Rozwiązanie:**
- Sprawdź `SpotkaniaAPI/local.settings.json`
- Upewnij się, że connection string jest poprawny
- Sprawdź czy database "SpotkaniaDB" i container "Persons" istnieją

### Problem: CORS error

**Rozwiązanie:**
- To normalne dla lokalnego testowania
- Na Azure Static Web Apps CORS jest automatycznie obsługiwany
- Możesz zignorować ten błąd lokalnie
