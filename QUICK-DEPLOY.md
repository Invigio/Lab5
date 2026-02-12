# 🎯 QUICK START - Wdrożenie na Azure w 5 minut

## Krok 1: GitHub (2 minuty)

```powershell
# W katalogu Lab5
git init
git add .
git commit -m "Meeting Scheduler - Initial commit"

# Zmień URL na swój!
git remote add origin https://github.com/TWOJ-USERNAME/meeting-scheduler.git
git branch -M main
git push -u origin main
```

## Krok 2: Azure Portal (3 minuty)

1. Idź na https://portal.azure.com
2. **Create a resource** → **Static Web App**
3. Wypełnij:
   - Name: `meeting-scheduler-app`
   - Plan: **Free**
   - Region: **West Europe**
   - Source: **GitHub** (autoryzuj)
   - Repository: `meeting-scheduler`
   - Branch: `main`
   - App location: `/frontend`
   - Api location: `/SpotkaniaAPI`
4. **Create** i czekaj 2-3 minuty

## Krok 3: Configuration

1. Znajdź swoją Static Web App
2. **Configuration** → **+ Add**
3. Dodaj:
   ```
   Name: CosmosDbConnectionString
   Value: [Twój connection string z Cosmos DB]
   ```
4. **Save**

## ✅ GOTOWE!

- URL: `https://meeting-scheduler-app.azurestaticapps.net`
- GitHub Actions: automatyczne wdrożenie przy każdym push
- Każda zmiana kodu = automatyczny deployment!

## 📖 Pełna dokumentacja

Jeśli coś nie działa, zobacz: **[DEPLOY.md](DEPLOY.md)**
