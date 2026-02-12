# Naprawienie Zmiennych Środowiskowych - Azure Static Web Apps

## Problem
API zwraca HTTP 500 bo zmienne środowiskowe (`CosmosDbConnectionString`) nie są dostępne dla Node.js Azure Functions.

## Rozwiązanie: 3 kroki

### KROK 1: Pobierz Connection String z Azure Park

1. Otwórz Azure Portal: https://portal.azure.com
2. Przejdź do **Azure Cosmos DB** > Your account
3. Kliknij **Keys** (lewy panel)
4. Skopiuj **Primary Connection String** (DUżY STRING z `AccountEndpoint=...`)
5. Zapisz sobie gdzieś tymczasowo

### KROK 2: Dodaj GitHub Actions Secret

1. Otwórz GitHub: https://github.com/Invigio/Lab5
2. Settings (tab górny) → **Secrets and variables** → **Actions** (lewy panel)
3. Kliknij **New repository secret**
4. Nazwa: `COSMOS_DB_CONNECTION_STRING`
5. Wartość: Wklej CONNECTION STRING ze STEP 1
6. Kliknij **Add secret**

### KROK 3: Zweryfikuj Workflow (juz zaaktualizujesz)

Workflow juz ma dodane zmienne. Teraz:

1. Przejdź do GitHub: https://github.com/Invigio/Lab5
2. Kliknij **Actions** (tab górny)
3. Czekaj aż job się uruchomi i deploy
4. Powinno być zielone ✅ (success)

### KROK 4: Testuj API

1. Otwórz aplikację: https://black-sky-0f820dc03.1.azurestaticapps.net
2. Otwórz **DevTools** (F12)
3. Przejdź do **Console**
4. Powinieneś zobaczyć listę osób zamiast błędu:
   - Zamiast: `Error: Błąd podczas pobierania osób`
   - Powinно: `[]` (pusta lista)

## Co się zmienoło?

### 1. cosmosClient.js
- Teraz akceptuje `CosmosDbConnectionString`, `COSMOS_DB_CONNECTION_STRING`, lub `CosmosConnectionString`
- Wypisuje jakie zmienne są dostepne (dla debugowania)

### 2. Workflow (GitHub Actions)
```yaml
env:
  CosmosDbConnectionString: ${{ secrets.COSMOS_DB_CONNECTION_STRING }}
  COSMOS_DB_CONNECTION_STRING: ${{ secrets.COSMOS_DB_CONNECTION_STRING }}
```
- Zmienne są teraz przekazywane podczas deployment

### 3. Błędy API
- Teraz pokazują stack trace i dok. informacje
- Możesz zobaczyć dok. diagnostyce jakie zmienne są dostępne

## Troubleshooting

Jeśli **NIE DZIAŁA:**

1. **Sprawdź czy secret został dodany:**
   - GitHub: Settings → Secrets and variables → Actions
   - Powiniien widać `COSMOS_DB_CONNECTION_STRING` na liście

2. **Sprawdź build logs:**
   - GitHub: Actions → Latest workflow
   - Powinno być GREEN ✅

3. **Sprawdź Azure Portal logs:**
   - Azure Portal → Log Stream (Stream Logs)
   - Szukaj `AddPerson ERROR` lub `GetPersons ERROR`
   - Te komunikaty pokażą dokładny problem

4. **Jeśli błęd mówi o "CosmosDbConnectionString not configured":**
   - Secret nie został dodany PRAWIDŁOWO
   - Powtórz KROK 2 - upewnij się że nazwa jest `COSMOS_DB_CONNECTION_STRING`

## Expected Errors wśród Process

Po naprawie powinieneś ZOBACZYĆ:
- ✅ GET /api/persons → 200 OK [] (pusta lista osób)
- ✅ POST /api/persons → 201 Created (dodaje osobę)
- ✅ Frontend ładuje się bez błędów

Jeśli coś NADAL nie działa:
- Sprawdź Browser Console (F12) - jakie błędy widzisz?
- Sprawdź Azure Portal logs - jakie są ostatnie komunikaty?
