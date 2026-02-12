# 🚀 Wdrożenie na Azure - Instrukcja krok po kroku

## 📋 Czego użyjemy?

- **Azure Static Web Apps** - hosting frontendu + backendu (Azure Functions)
- **GitHub Actions** - automatyczne wdrożenie (CI/CD)
- **Azure Cosmos DB** - baza danych (już masz)

## ✅ Wymagania

- [x] Konto GitHub
- [x] Konto Azure z aktywną subskrypcją
- [x] Azure Cosmos DB (już skonfigurowana)
- [x] Git zainstalowany lokalnie

---

## 🔥 KROK 1: Przygotowanie projektu

### 1.1. Dostosuj strukturę projektu

Struktura dla Azure Static Web Apps:
```
Lab5/
├── frontend/              ← Root frontendu
│   ├── index.html
│   ├── styles.css
│   └── app.js
├── SpotkaniaAPI/          ← Azure Functions
│   ├── Models/
│   ├── Functions/
│   └── ...
```

### 1.2. Zmień URL API w frontend/app.js

Otwórz `frontend/app.js` i zmień na początek:

```javascript
// Dla produkcji na Azure (API będzie automatycznie dostępne)
const API_BASE_URL = '/api';

// Dla lokalnego testowania odkomentuj:
// const API_BASE_URL = 'http://localhost:7071/api';
```

---

## 🔥 KROK 2: GitHub Repository

### 2.1. Utwórz nowe repozytorium

1. Idź na https://github.com
2. Kliknij **New repository**
3. Nazwa: `meeting-scheduler-azure`
4. Wybierz **Private** (jeśli chcesz prywatne)
5. **NIE** zaznaczaj "Initialize with README"
6. Kliknij **Create repository**

### 2.2. Wrzuć kod na GitHub

Otwórz terminal w katalogu Lab5 i wykonaj:

```powershell
# Inicjalizuj Git (jeśli jeszcze nie zrobione)
git init

# Dodaj .gitignore
@"
bin/
obj/
.vs/
.vscode/
*.user
local.settings.json
node_modules/
__pycache__/
*.pyc
"@ | Out-File -FilePath .gitignore -Encoding utf8

# Dodaj pliki
git add .
git commit -m "Initial commit - Meeting Scheduler"

# Połącz z GitHub (ZMIEŃ na swój URL!)
git remote add origin https://github.com/TWOJ-USERNAME/meeting-scheduler-azure.git

# Wypchnij kod
git branch -M main
git push -u origin main
```

**UWAGA:** Zamień `TWOJ-USERNAME/meeting-scheduler-azure.git` na faktyczny URL z GitHub!

### 2.3. Sprawdź na GitHub

Odśwież stronę repozytorium - powinieneś zobaczyć swoje pliki.

---

## 🔥 KROK 3: Azure Static Web App

### 3.1. Utwórz Static Web App

1. Idź na **Azure Portal** (https://portal.azure.com)
2. Kliknij **Create a resource**
3. Wyszukaj **Static Web App**
4. Kliknij **Create**

### 3.2. Konfiguracja (zakładka "Basics")

| Pole | Wartość |
|------|---------|
| **Subscription** | Twoja subskrypcja |
| **Resource Group** | Utwórz nową: `rg-meeting-scheduler` |
| **Name** | `meeting-scheduler-app` (musi być unikalna) |
| **Plan Type** | **Free** |
| **Region** | **West Europe** |
| **Source** | **GitHub** |

### 3.3. Połącz z GitHub

1. Kliknij **Sign in with GitHub**
2. Autoryzuj Azure
3. Wybierz:
   - **Organization:** Twoja nazwa użytkownika
   - **Repository:** `meeting-scheduler-azure`
   - **Branch:** `main`

### 3.4. Build Details

| Pole | Wartość |
|------|---------|
| **Build Presets** | **Custom** |
| **App location** | `/frontend` |
| **Api location** | `/SpotkaniaAPI` |
| **Output location** | *(pozostaw puste)* |

### 3.5. Końcowe kroki

1. Kliknij **Review + create**
2. Kliknij **Create**
3. **Czekaj 2-3 minuty** ⏳

Azure automatycznie:
- Utworzy GitHub Action workflow
- Zbuduje aplikację
- Wdroży ją na Azure

---

## 🔥 KROK 4: Konfiguracja Connection String

### 4.1. Znajdź swój Static Web App

1. W Azure Portal, idź do **Resource Groups**
2. Otwórz `rg-meeting-scheduler`
3. Kliknij na `meeting-scheduler-app`

### 4.2. Dodaj Connection String

1. W menu po lewej, kliknij **Configuration**
2. Kliknij **+ Add**
3. Dodaj zmienne:

| Name | Value |
|------|-------|
| `CosmosDbConnectionString` | Twój connection string z Cosmos DB |

**Przykład:**
```
AccountEndpoint=https://umawianie-spotkan-db-twojeinicjaly.documents.azure.com:443/;AccountKey=TWOJ-KLUCZ-TUTAJ==
```

4. Kliknij **OK**
5. Kliknij **Save** na górze

---

## 🔥 KROK 5: Sprawdź GitHub Actions

### 5.1. Zobacz workflow

1. Idź na GitHub: `https://github.com/TWOJ-USERNAME/meeting-scheduler-azure`
2. Kliknij zakładkę **Actions**
3. Powinieneś zobaczyć workflow: `Azure Static Web Apps CI/CD`
4. Kliknij na najnowszy run

### 5.2. Monitoruj build

- **Build and Deploy Job** - kompilacja i wdrożenie
- Status:
  - 🟡 Żółty = w trakcie
  - 🟢 Zielony = sukces
  - 🔴 Czerwony = błąd

**Czekaj aż się skończy (3-5 minut)**

---

## 🔥 KROK 6: Testowanie aplikacji

### 6.1. Znajdź URL aplikacji

**Sposób 1** - Azure Portal:
1. Idź do Static Web App w Azure Portal
2. Na stronie Overview zobaczysz **URL**
3. Przykład: `https://meeting-scheduler-app.azurestaticapps.net`

**Sposób 2** - GitHub Actions:
1. W zakładce Actions, otwórz ostatni workflow run
2. Na końcu logu zobaczysz URL

### 6.2. Otwórz aplikację

1. Otwórz URL w przeglądarce
2. Poczekaj chwilę na załadowanie
3. Przetestuj wszystkie funkcje:
   - ✅ Dodaj osobę z harmonogramem
   - ✅ Sprawdź wolne terminy
   - ✅ Zarezerwuj spotkanie
   - ✅ Zobacz listę osób i spotkań

### 6.3. Sprawdź API

Otwórz narzędzia deweloperskie (F12) → Network:
- API endpoints powinny działać: `/api/persons`, `/api/availability`, `/api/book`
- Status: **200 OK**

---

## 🔥 KROK 7: Cosmos DB - Sprawdź dane

### 7.1. Azure Portal → Cosmos DB

1. Idź do swojej Cosmos DB
2. Kliknij **Data Explorer**
3. Rozwiń: **SpotkaniaDB** → **Persons**
4. Kliknij **Items**
5. Powinieneś zobaczyć dodane osoby i spotkania

---

## 🎯 GOTOWE! Aplikacja działa na Azure!

### 📍 Twoje zasoby na Azure:

| Zasób | Cel |
|-------|-----|
| **Static Web App** | Frontend + Backend API |
| **Cosmos DB** | Baza danych NoSQL |
| **GitHub Actions** | Automatyczne wdrożenie |

### 🔄 Jak aktualizować aplikację?

1. Zmieniaj kod lokalnie
2. Commit i push do GitHub:
   ```powershell
   git add .
   git commit -m "Opis zmian"
   git push
   ```
3. GitHub Actions automatycznie wdroży nową wersję!
4. Odśwież aplikację w przeglądarce (CTRL+F5)

---

## 🐛 Rozwiązywanie problemów

### Problem: GitHub Actions = czerwony

**Rozwiązanie:**
1. Otwórz failed workflow na GitHub
2. Przeczytaj logi błędów
3. Najczęstsze przyczyny:
   - Błędne ścieżki (`app_location`, `api_location`)
   - Brak pliku `.csproj` w folderze API
   - Błędy kompilacji C#

### Problem: API nie działa (404, 500)

**Rozwiązanie:**
1. Sprawdź Configuration w Azure Portal
2. Czy `CosmosDbConnectionString` jest ustawiony?
3. Sprawdź logi:
   - Azure Portal → Static Web App → **Functions** → **Monitor**

### Problem: CORS Error w przeglądarce

**Rozwiązanie:**
- Static Web Apps automatycznie obsługuje CORS
- Jeśli problem występuje, sprawdź czy `API_BASE_URL` w `app.js` to `/api` (nie `http://localhost:7071/api`)

### Problem: Cosmos DB connection error

**Rozwiązanie:**
1. Sprawdź connection string w Configuration
2. Upewnij się, że w Cosmos DB:
   - Database: `SpotkaniaDB`
   - Container: `Persons`
   - Partition key: `/id`

---

## 💰 Koszty

| Zasób | Plan | Koszt |
|-------|------|-------|
| **Static Web App** | Free | **$0** (100 GB transfer) |
| **Cosmos DB** | Free tier | **$0** (400 RU/s, 25 GB) |
| **GitHub** | Publiczne repo | **$0** |

**Całkowity koszt miesięczny: ~$0** ✨

---

## 📚 Dodatkowe zasoby

- [Azure Static Web Apps - Dokumentacja](https://learn.microsoft.com/en-us/azure/static-web-apps/)
- [GitHub Actions - Dokumentacja](https://docs.github.com/en/actions)
- [Azure Cosmos DB - Dokumentacja](https://learn.microsoft.com/en-us/azure/cosmos-db/)

---

**Powodzenia z wdrożeniem! 🚀**
