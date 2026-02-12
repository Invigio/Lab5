# Lab 5-6: System do umawiania spotkań - Instrukcja krok po kroku

## 📋 Spis treści
1. [Przygotowanie środowiska](#krok-1-przygotowanie-środowiska)
2. [Struktura projektu](#krok-2-struktura-projektu)
3. [Konfiguracja Azure Functions](#krok-3-konfiguracja-azure-functions)
4. [Implementacja API](#krok-4-implementacja-api)
5. [Implementacja frontendu](#krok-5-implementacja-frontendu)
6. [Testowanie](#krok-6-testowanie)
7. [Wdrożenie na Azure](#krok-7-wdrożenie-na-azure)

---

## Krok 1: Przygotowanie środowiska

### 1.1 Wymagane narzędzia
```powershell
# Sprawdź wersje zainstalowanych narzędzi:
node --version          # Wymagane: v18.x lub nowsze
npm --version           # Wymagane: v9.x lub nowsze
dotnet --version        # Wymagane: .NET 6.0 lub nowsze
func --version          # Azure Functions Core Tools v4.x
```

### 1.2 Instalacja Azure Functions Core Tools (jeśli nie masz)
```powershell
npm install -g azure-functions-core-tools@4 --unsafe-perm true
```

### 1.3 Instalacja Azure CLI (opcjonalne, do wdrożenia)
```powershell
# Pobierz z: https://aka.ms/installazurecliwindows
# Lub użyj:
winget install -e --id Microsoft.AzureCLI
```

---

## Krok 2: Struktura projektu

Stwórz następującą strukturę katalogów:

```
Lab5/
├── api/                          # Azure Functions (backend)
│   ├── AddPerson/                # Funkcja: dodawanie osoby
│   ├── GetPersons/               # Funkcja: pobieranie wszystkich osób
│   ├── GetAvailableSlots/        # Funkcja: pobieranie dostępnych terminów
│   ├── BookAppointment/          # Funkcja: rezerwacja spotkania
│   ├── GetAppointments/          # Funkcja: pobieranie rezerwacji
│   ├── host.json                 # Konfiguracja hosta Functions
│   ├── local.settings.json       # Ustawienia lokalne (NIE commituj!)
│   └── package.json              # Zależności Node.js
│
└── frontend/                     # Frontend (HTML/CSS/JS)
    ├── index.html                # Strona główna
    ├── styles.css                # Stylizacja
    └── app.js                    # Logika frontendu
```

---

## Krok 3: Konfiguracja Azure Functions

### 3.1 Stwórz projekt Functions (Node.js)
```powershell
# Przejdź do folderu Lab5
cd "C:\Users\norbe\Desktop\Magisterka\Semestr 2\Programowanie w Chmurze Obliczeniowej\Lab5"

# Stwórz folder api
mkdir api
cd api

# Inicjalizuj projekt Functions (wybierz Node.js + JavaScript)
func init . --javascript

# Zainstaluj SDK Cosmos DB
npm install @azure/cosmos
```

### 3.2 Skonfiguruj local.settings.json
Edytuj plik `api/local.settings.json`:

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

### 3.3 Skonfiguruj host.json
Edytuj `api/host.json`:

```json
{
  "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "maxTelemetryItemsPerSecond": 20
      }
    }
  },
  "extensionBundle": {
    "id": "Microsoft.Azure.Functions.ExtensionBundle",
    "version": "[4.*, 5.0.0)"
  }
}
```

---

## Krok 4: Implementacja API

### 4.1 Stwórz funkcję: AddPerson
```powershell
cd api
func new --name AddPerson --template "HTTP trigger" --authlevel "anonymous"
```

Edytuj `api/AddPerson/index.js`:

```javascript
const { CosmosClient } = require("@azure/cosmos");

module.exports = async function (context, req) {
    const connectionString = process.env.CosmosDBConnection;
    const client = new CosmosClient(connectionString);

    const database = client.database("MeetingsDB");
    const container = database.container("Persons");

    try {
        const person = {
            id: req.body.id || `person_${Date.now()}`,
            name: req.body.name,
            email: req.body.email,
            availability: req.body.availability || {
                monday: { start: "08:00", end: "16:00", enabled: true },
                tuesday: { start: "08:00", end: "16:00", enabled: true },
                wednesday: { start: "08:00", end: "16:00", enabled: true },
                thursday: { start: "08:00", end: "16:00", enabled: true },
                friday: { start: "08:00", end: "16:00", enabled: true },
                saturday: { enabled: false },
                sunday: { enabled: false }
            }
        };

        const { resource } = await container.items.create(person);

        context.res = {
            status: 201,
            body: resource,
            headers: { 'Content-Type': 'application/json' }
        };
    } catch (error) {
        context.res = {
            status: 500,
            body: { error: error.message }
        };
    }
};
```

Edytuj `api/AddPerson/function.json`:

```json
{
  "bindings": [
    {
      "authLevel": "anonymous",
      "type": "httpTrigger",
      "direction": "in",
      "name": "req",
      "methods": ["post"],
      "route": "persons"
    },
    {
      "type": "http",
      "direction": "out",
      "name": "res"
    }
  ]
}
```

### 4.2 Stwórz funkcję: GetPersons
```powershell
func new --name GetPersons --template "HTTP trigger" --authlevel "anonymous"
```

Edytuj `api/GetPersons/index.js`:

```javascript
const { CosmosClient } = require("@azure/cosmos");

module.exports = async function (context, req) {
    const connectionString = process.env.CosmosDBConnection;
    const client = new CosmosClient(connectionString);

    const database = client.database("MeetingsDB");
    const container = database.container("Persons");

    try {
        const { resources } = await container.items
            .query("SELECT * FROM c")
            .fetchAll();

        context.res = {
            status: 200,
            body: resources,
            headers: { 'Content-Type': 'application/json' }
        };
    } catch (error) {
        context.res = {
            status: 500,
            body: { error: error.message }
        };
    }
};
```

Edytuj `api/GetPersons/function.json`:

```json
{
  "bindings": [
    {
      "authLevel": "anonymous",
      "type": "httpTrigger",
      "direction": "in",
      "name": "req",
      "methods": ["get"],
      "route": "persons"
    },
    {
      "type": "http",
      "direction": "out",
      "name": "res"
    }
  ]
}
```

### 4.3 Stwórz funkcję: GetAvailableSlots
```powershell
func new --name GetAvailableSlots --template "HTTP trigger" --authlevel "anonymous"
```

Edytuj `api/GetAvailableSlots/index.js`:

```javascript
const { CosmosClient } = require("@azure/cosmos");

function generateTimeSlots(date, startTime, endTime, bookedSlots) {
    const slots = [];
    const [startHour, startMin] = startTime.split(':').map(Number);
    const [endHour, endMin] = endTime.split(':').map(Number);

    let currentHour = startHour;
    let currentMin = startMin;

    while (currentHour < endHour || (currentHour === endHour && currentMin < endMin)) {
        const timeSlot = `${String(currentHour).padStart(2, '0')}:${String(currentMin).padStart(2, '0')}`;
        const slotDateTime = `${date}T${timeSlot}`;

        if (!bookedSlots.includes(slotDateTime)) {
            slots.push(timeSlot);
        }

        currentMin += 30;
        if (currentMin >= 60) {
            currentMin = 0;
            currentHour++;
        }
    }

    return slots;
}

function getDayName(dateString) {
    const days = ['sunday', 'monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday'];
    const date = new Date(dateString);
    return days[date.getDay()];
}

module.exports = async function (context, req) {
    const connectionString = process.env.CosmosDBConnection;
    const client = new CosmosClient(connectionString);

    const database = client.database("MeetingsDB");
    const personsContainer = database.container("Persons");
    const appointmentsContainer = database.container("Appointments");

    const personId = req.query.personId;
    const date = req.query.date; // Format: YYYY-MM-DD

    if (!personId || !date) {
        context.res = {
            status: 400,
            body: { error: "personId and date are required" }
        };
        return;
    }

    try {
        // Pobierz osobę
        const { resource: person } = await personsContainer.item(personId, personId).read();

        if (!person) {
            context.res = {
                status: 404,
                body: { error: "Person not found" }
            };
            return;
        }

        // Sprawdź dostępność dla danego dnia
        const dayName = getDayName(date);
        const dayAvailability = person.availability[dayName];

        if (!dayAvailability || !dayAvailability.enabled) {
            context.res = {
                status: 200,
                body: { date, personId, availableSlots: [] }
            };
            return;
        }

        // Pobierz zarezerwowane sloty
        const { resources: appointments } = await appointmentsContainer.items
            .query({
                query: "SELECT * FROM c WHERE c.personId = @personId AND c.date = @date",
                parameters: [
                    { name: "@personId", value: personId },
                    { name: "@date", value: date }
                ]
            })
            .fetchAll();

        const bookedSlots = appointments.map(a => `${a.date}T${a.time}`);

        // Generuj dostępne sloty
        const availableSlots = generateTimeSlots(
            date,
            dayAvailability.start,
            dayAvailability.end,
            bookedSlots
        );

        context.res = {
            status: 200,
            body: {
                date,
                personId,
                personName: person.name,
                availableSlots
            },
            headers: { 'Content-Type': 'application/json' }
        };
    } catch (error) {
        context.res = {
            status: 500,
            body: { error: error.message }
        };
    }
};
```

Edytuj `api/GetAvailableSlots/function.json`:

```json
{
  "bindings": [
    {
      "authLevel": "anonymous",
      "type": "httpTrigger",
      "direction": "in",
      "name": "req",
      "methods": ["get"],
      "route": "slots"
    },
    {
      "type": "http",
      "direction": "out",
      "name": "res"
    }
  ]
}
```

### 4.4 Stwórz funkcję: BookAppointment
```powershell
func new --name BookAppointment --template "HTTP trigger" --authlevel "anonymous"
```

Edytuj `api/BookAppointment/index.js`:

```javascript
const { CosmosClient } = require("@azure/cosmos");

module.exports = async function (context, req) {
    const connectionString = process.env.CosmosDBConnection;
    const client = new CosmosClient(connectionString);

    const database = client.database("MeetingsDB");
    const appointmentsContainer = database.container("Appointments");

    try {
        const appointment = {
            id: `appointment_${Date.now()}`,
            personId: req.body.personId,
            date: req.body.date,
            time: req.body.time,
            clientName: req.body.clientName,
            clientEmail: req.body.clientEmail,
            description: req.body.description || "",
            createdAt: new Date().toISOString()
        };

        // Sprawdź, czy slot nie jest już zajęty
        const { resources: existing } = await appointmentsContainer.items
            .query({
                query: "SELECT * FROM c WHERE c.personId = @personId AND c.date = @date AND c.time = @time",
                parameters: [
                    { name: "@personId", value: appointment.personId },
                    { name: "@date", value: appointment.date },
                    { name: "@time", value: appointment.time }
                ]
            })
            .fetchAll();

        if (existing.length > 0) {
            context.res = {
                status: 409,
                body: { error: "This time slot is already booked" }
            };
            return;
        }

        const { resource } = await appointmentsContainer.items.create(appointment);

        context.res = {
            status: 201,
            body: resource,
            headers: { 'Content-Type': 'application/json' }
        };
    } catch (error) {
        context.res = {
            status: 500,
            body: { error: error.message }
        };
    }
};
```

Edytuj `api/BookAppointment/function.json`:

```json
{
  "bindings": [
    {
      "authLevel": "anonymous",
      "type": "httpTrigger",
      "direction": "in",
      "name": "req",
      "methods": ["post"],
      "route": "appointments"
    },
    {
      "type": "http",
      "direction": "out",
      "name": "res"
    }
  ]
}
```

### 4.5 Stwórz funkcję: GetAppointments
```powershell
func new --name GetAppointments --template "HTTP trigger" --authlevel "anonymous"
```

Edytuj `api/GetAppointments/index.js`:

```javascript
const { CosmosClient } = require("@azure/cosmos");

module.exports = async function (context, req) {
    const connectionString = process.env.CosmosDBConnection;
    const client = new CosmosClient(connectionString);

    const database = client.database("MeetingsDB");
    const appointmentsContainer = database.container("Appointments");

    try {
        const personId = req.query.personId;

        let query = "SELECT * FROM c ORDER BY c.date, c.time";
        let parameters = [];

        if (personId) {
            query = "SELECT * FROM c WHERE c.personId = @personId ORDER BY c.date, c.time";
            parameters = [{ name: "@personId", value: personId }];
        }

        const { resources } = await appointmentsContainer.items
            .query({ query, parameters })
            .fetchAll();

        context.res = {
            status: 200,
            body: resources,
            headers: { 'Content-Type': 'application/json' }
        };
    } catch (error) {
        context.res = {
            status: 500,
            body: { error: error.message }
        };
    }
};
```

Edytuj `api/GetAppointments/function.json`:

```json
{
  "bindings": [
    {
      "authLevel": "anonymous",
      "type": "httpTrigger",
      "direction": "in",
      "name": "req",
      "methods": ["get"],
      "route": "appointments"
    },
    {
      "type": "http",
      "direction": "out",
      "name": "res"
    }
  ]
}
```

---

## Krok 5: Implementacja frontendu

### 5.1 Stwórz strukturę frontendu
```powershell
cd ..
mkdir frontend
cd frontend
```

### 5.2 Stwórz index.html

```html
<!DOCTYPE html>
<html lang="pl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>System umawiania spotkań</title>
    <link rel="stylesheet" href="styles.css">
</head>
<body>
    <div class="container">
        <h1>📅 System umawiania spotkań</h1>

        <!-- Sekcja 1: Dodawanie osoby -->
        <section class="card">
            <h2>➕ Dodaj osobę</h2>
            <form id="addPersonForm">
                <input type="text" id="personName" placeholder="Imię i nazwisko" required>
                <input type="email" id="personEmail" placeholder="Email" required>

                <h3>Dostępność:</h3>
                <div class="availability-grid">
                    <div class="day-config" data-day="monday">
                        <label><input type="checkbox" checked> Poniedziałek</label>
                        <input type="time" value="08:00" class="start-time">
                        <span>-</span>
                        <input type="time" value="16:00" class="end-time">
                    </div>
                    <div class="day-config" data-day="tuesday">
                        <label><input type="checkbox" checked> Wtorek</label>
                        <input type="time" value="08:00" class="start-time">
                        <span>-</span>
                        <input type="time" value="16:00" class="end-time">
                    </div>
                    <div class="day-config" data-day="wednesday">
                        <label><input type="checkbox" checked> Środa</label>
                        <input type="time" value="08:00" class="start-time">
                        <span>-</span>
                        <input type="time" value="16:00" class="end-time">
                    </div>
                    <div class="day-config" data-day="thursday">
                        <label><input type="checkbox" checked> Czwartek</label>
                        <input type="time" value="08:00" class="start-time">
                        <span>-</span>
                        <input type="time" value="16:00" class="end-time">
                    </div>
                    <div class="day-config" data-day="friday">
                        <label><input type="checkbox" checked> Piątek</label>
                        <input type="time" value="08:00" class="start-time">
                        <span>-</span>
                        <input type="time" value="16:00" class="end-time">
                    </div>
                    <div class="day-config" data-day="saturday">
                        <label><input type="checkbox"> Sobota</label>
                        <input type="time" value="09:00" class="start-time">
                        <span>-</span>
                        <input type="time" value="14:00" class="end-time">
                    </div>
                    <div class="day-config" data-day="sunday">
                        <label><input type="checkbox"> Niedziela</label>
                        <input type="time" value="10:00" class="start-time">
                        <span>-</span>
                        <input type="time" value="14:00" class="end-time">
                    </div>
                </div>

                <button type="submit">Dodaj osobę</button>
            </form>
        </section>

        <!-- Sekcja 2: Rezerwacja spotkania -->
        <section class="card">
            <h2>📆 Zarezerwuj spotkanie</h2>
            <form id="bookAppointmentForm">
                <select id="selectPerson" required>
                    <option value="">Wybierz osobę...</option>
                </select>

                <input type="date" id="appointmentDate" required>

                <div id="availableSlots" class="slots-grid"></div>

                <input type="text" id="clientName" placeholder="Twoje imię i nazwisko" required>
                <input type="email" id="clientEmail" placeholder="Twój email" required>
                <textarea id="appointmentDescription" placeholder="Opis spotkania (opcjonalnie)"></textarea>

                <button type="submit" id="bookButton" disabled>Zarezerwuj</button>
            </form>
        </section>

        <!-- Sekcja 3: Lista spotkań -->
        <section class="card">
            <h2>📋 Zarezerwowane spotkania</h2>
            <div id="appointmentsList"></div>
        </section>

        <!-- Komunikaty -->
        <div id="message" class="message"></div>
    </div>

    <script src="app.js"></script>
</body>
</html>
```

### 5.3 Stwórz styles.css

```css
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    min-height: 100vh;
    padding: 20px;
}

.container {
    max-width: 1200px;
    margin: 0 auto;
}

h1 {
    text-align: center;
    color: white;
    margin-bottom: 30px;
    font-size: 2.5em;
    text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
}

.card {
    background: white;
    border-radius: 15px;
    padding: 30px;
    margin-bottom: 30px;
    box-shadow: 0 10px 30px rgba(0,0,0,0.2);
}

h2 {
    color: #667eea;
    margin-bottom: 20px;
    font-size: 1.8em;
}

h3 {
    color: #764ba2;
    margin: 20px 0 10px 0;
    font-size: 1.2em;
}

form {
    display: flex;
    flex-direction: column;
    gap: 15px;
}

input[type="text"],
input[type="email"],
input[type="date"],
input[type="time"],
select,
textarea {
    padding: 12px;
    border: 2px solid #e0e0e0;
    border-radius: 8px;
    font-size: 1em;
    transition: border-color 0.3s;
}

input:focus,
select:focus,
textarea:focus {
    outline: none;
    border-color: #667eea;
}

textarea {
    min-height: 100px;
    resize: vertical;
}

button {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 15px;
    border: none;
    border-radius: 8px;
    font-size: 1.1em;
    font-weight: bold;
    cursor: pointer;
    transition: transform 0.2s, box-shadow 0.2s;
}

button:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
}

button:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.availability-grid {
    display: grid;
    gap: 15px;
}

.day-config {
    display: grid;
    grid-template-columns: 150px 80px 20px 80px;
    align-items: center;
    gap: 10px;
    padding: 10px;
    background: #f8f9fa;
    border-radius: 8px;
}

.day-config label {
    display: flex;
    align-items: center;
    gap: 8px;
    cursor: pointer;
}

.day-config input[type="checkbox"] {
    width: 20px;
    height: 20px;
    cursor: pointer;
}

.slots-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
    gap: 10px;
    margin: 20px 0;
}

.slot-button {
    padding: 12px;
    background: #f0f4ff;
    border: 2px solid #667eea;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.2s;
    font-size: 1em;
}

.slot-button:hover {
    background: #667eea;
    color: white;
    transform: scale(1.05);
}

.slot-button.selected {
    background: #764ba2;
    color: white;
    border-color: #764ba2;
}

.appointment-item {
    background: #f8f9fa;
    padding: 15px;
    border-radius: 8px;
    margin-bottom: 10px;
    border-left: 4px solid #667eea;
}

.appointment-item h4 {
    color: #764ba2;
    margin-bottom: 8px;
}

.appointment-item p {
    color: #666;
    margin: 5px 0;
}

.message {
    position: fixed;
    top: 20px;
    right: 20px;
    padding: 15px 25px;
    border-radius: 8px;
    box-shadow: 0 5px 15px rgba(0,0,0,0.2);
    display: none;
    max-width: 400px;
    z-index: 1000;
    animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
    from {
        transform: translateX(400px);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}

.message.success {
    background: #4caf50;
    color: white;
}

.message.error {
    background: #f44336;
    color: white;
}

.message.show {
    display: block;
}

@media (max-width: 768px) {
    .day-config {
        grid-template-columns: 1fr;
    }

    .slots-grid {
        grid-template-columns: repeat(auto-fill, minmax(80px, 1fr));
    }

    h1 {
        font-size: 1.8em;
    }
}
```

### 5.4 Stwórz app.js

```javascript
// Konfiguracja API
const API_BASE_URL = 'http://localhost:7071/api';

// Stan aplikacji
let selectedSlot = null;
let persons = [];
let appointments = [];

// Inicjalizacja
document.addEventListener('DOMContentLoaded', () => {
    loadPersons();
    loadAppointments();
    setupEventListeners();
});

// Obsługa zdarzeń
function setupEventListeners() {
    document.getElementById('addPersonForm').addEventListener('submit', handleAddPerson);
    document.getElementById('bookAppointmentForm').addEventListener('submit', handleBookAppointment);
    document.getElementById('selectPerson').addEventListener('change', handlePersonSelect);
    document.getElementById('appointmentDate').addEventListener('change', handleDateSelect);

    // Obsługa checkboxów dostępności
    document.querySelectorAll('.day-config input[type="checkbox"]').forEach(checkbox => {
        checkbox.addEventListener('change', (e) => {
            const dayConfig = e.target.closest('.day-config');
            const timeInputs = dayConfig.querySelectorAll('input[type="time"]');
            timeInputs.forEach(input => input.disabled = !e.target.checked);
        });
    });
}

// Pobierz listę osób
async function loadPersons() {
    try {
        const response = await fetch(`${API_BASE_URL}/persons`);
        persons = await response.json();

        const select = document.getElementById('selectPerson');
        select.innerHTML = '<option value="">Wybierz osobę...</option>';

        persons.forEach(person => {
            const option = document.createElement('option');
            option.value = person.id;
            option.textContent = `${person.name} (${person.email})`;
            select.appendChild(option);
        });
    } catch (error) {
        showMessage('Błąd przy pobieraniu listy osób: ' + error.message, 'error');
    }
}

// Pobierz listę spotkań
async function loadAppointments() {
    try {
        const response = await fetch(`${API_BASE_URL}/appointments`);
        appointments = await response.json();
        displayAppointments();
    } catch (error) {
        showMessage('Błąd przy pobieraniu spotkań: ' + error.message, 'error');
    }
}

// Wyświetl spotkania
function displayAppointments() {
    const container = document.getElementById('appointmentsList');

    if (appointments.length === 0) {
        container.innerHTML = '<p style="text-align: center; color: #999;">Brak zarezerwowanych spotkań</p>';
        return;
    }

    container.innerHTML = appointments.map(apt => {
        const person = persons.find(p => p.id === apt.personId);
        const personName = person ? person.name : apt.personId;

        return `
            <div class="appointment-item">
                <h4>📅 ${apt.date} o ${apt.time}</h4>
                <p><strong>Z:</strong> ${personName}</p>
                <p><strong>Klient:</strong> ${apt.clientName} (${apt.clientEmail})</p>
                ${apt.description ? `<p><strong>Opis:</strong> ${apt.description}</p>` : ''}
            </div>
        `;
    }).join('');
}

// Dodaj osobę
async function handleAddPerson(e) {
    e.preventDefault();

    const name = document.getElementById('personName').value;
    const email = document.getElementById('personEmail').value;

    // Zbierz dostępność
    const availability = {};
    document.querySelectorAll('.day-config').forEach(dayConfig => {
        const day = dayConfig.dataset.day;
        const checkbox = dayConfig.querySelector('input[type="checkbox"]');
        const startTime = dayConfig.querySelector('.start-time').value;
        const endTime = dayConfig.querySelector('.end-time').value;

        availability[day] = {
            enabled: checkbox.checked,
            start: startTime,
            end: endTime
        };
    });

    try {
        const response = await fetch(`${API_BASE_URL}/persons`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name, email, availability })
        });

        if (response.ok) {
            showMessage('Osoba została dodana pomyślnie!', 'success');
            document.getElementById('addPersonForm').reset();
            loadPersons();
        } else {
            throw new Error('Błąd przy dodawaniu osoby');
        }
    } catch (error) {
        showMessage('Błąd: ' + error.message, 'error');
    }
}

// Wybór osoby
function handlePersonSelect() {
    const date = document.getElementById('appointmentDate').value;
    if (date) {
        loadAvailableSlots();
    }
}

// Wybór daty
function handleDateSelect() {
    const personId = document.getElementById('selectPerson').value;
    if (personId) {
        loadAvailableSlots();
    }
}

// Pobierz dostępne sloty
async function loadAvailableSlots() {
    const personId = document.getElementById('selectPerson').value;
    const date = document.getElementById('appointmentDate').value;

    if (!personId || !date) return;

    try {
        const response = await fetch(`${API_BASE_URL}/slots?personId=${personId}&date=${date}`);
        const data = await response.json();

        const container = document.getElementById('availableSlots');

        if (data.availableSlots.length === 0) {
            container.innerHTML = '<p style="text-align: center; color: #999;">Brak dostępnych terminów w tym dniu</p>';
            return;
        }

        container.innerHTML = data.availableSlots.map(slot =>
            `<button type="button" class="slot-button" data-time="${slot}">${slot}</button>`
        ).join('');

        // Obsługa kliknięć w sloty
        container.querySelectorAll('.slot-button').forEach(button => {
            button.addEventListener('click', (e) => {
                container.querySelectorAll('.slot-button').forEach(btn =>
                    btn.classList.remove('selected')
                );
                e.target.classList.add('selected');
                selectedSlot = e.target.dataset.time;
                document.getElementById('bookButton').disabled = false;
            });
        });

    } catch (error) {
        showMessage('Błąd przy pobieraniu dostępnych terminów: ' + error.message, 'error');
    }
}

// Rezerwacja spotkania
async function handleBookAppointment(e) {
    e.preventDefault();

    if (!selectedSlot) {
        showMessage('Wybierz godzinę spotkania', 'error');
        return;
    }

    const appointment = {
        personId: document.getElementById('selectPerson').value,
        date: document.getElementById('appointmentDate').value,
        time: selectedSlot,
        clientName: document.getElementById('clientName').value,
        clientEmail: document.getElementById('clientEmail').value,
        description: document.getElementById('appointmentDescription').value
    };

    try {
        const response = await fetch(`${API_BASE_URL}/appointments`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(appointment)
        });

        if (response.ok) {
            showMessage('Spotkanie zarezerwowane pomyślnie!', 'success');
            document.getElementById('bookAppointmentForm').reset();
            document.getElementById('availableSlots').innerHTML = '';
            document.getElementById('bookButton').disabled = true;
            selectedSlot = null;
            loadAppointments();
        } else if (response.status === 409) {
            showMessage('Ten termin został już zarezerwowany. Wybierz inny.', 'error');
            loadAvailableSlots();
        } else {
            throw new Error('Błąd przy rezerwacji');
        }
    } catch (error) {
        showMessage('Błąd: ' + error.message, 'error');
    }
}

// Wyświetl komunikat
function showMessage(text, type) {
    const messageEl = document.getElementById('message');
    messageEl.textContent = text;
    messageEl.className = `message ${type} show`;

    setTimeout(() => {
        messageEl.classList.remove('show');
    }, 5000);
}
```

---

## Krok 6: Testowanie

### 6.1 Uruchom Azure Functions lokalnie
```powershell
cd api
func start
```

Funkcje powinny być dostępne na: `http://localhost:7071`

### 6.2 Uruchom frontend lokalnie
Otwórz nowy terminal PowerShell:

```powershell
cd frontend

# Prosty serwer HTTP (Python)
python -m http.server 8000

# LUB użyj Live Server w VS Code (rozszerzenie)
```

Frontend będzie dostępny na: `http://localhost:8000`

### 6.3 Testowanie funkcjonalności

1. **Dodaj osobę:**
   - Otwórz frontend w przeglądarce
   - Wypełnij formularz dodawania osoby
   - Skonfiguruj dostępność (np. Pn-Pt 8:00-16:00)
   - Kliknij "Dodaj osobę"

2. **Sprawdź dostępne terminy:**
   - Wybierz osobę z listy
   - Wybierz datę
   - Sprawdź wyświetlone dostępne sloty

3. **Zarezerwuj spotkanie:**
   - Wybierz slot czasowy
   - Wypełnij dane klienta
   - Kliknij "Zarezerwuj"

4. **Sprawdź listę spotkań:**
   - Przewiń do sekcji "Zarezerwowane spotkania"
   - Sprawdź czy Twoje spotkanie się wyświetla

---

## Krok 7: Wdrożenie na Azure

### 7.1 Przygotuj Cosmos DB

W Azure Portal:
1. Przejdź do swojej bazy Cosmos DB
2. Stwórz bazę danych: `MeetingsDB`
3. Stwórz kontenery:
   - **Persons** (Partition key: `/id`)
   - **Appointments** (Partition key: `/personId`)

### 7.2 Wdróż Azure Functions

```powershell
cd api

# Zaloguj się do Azure
az login

# Stwórz Function App (zamień YOUR_UNIQUE_NAME)
az functionapp create `
  --resource-group YOUR_RESOURCE_GROUP `
  --consumption-plan-location westeurope `
  --runtime node `
  --runtime-version 18 `
  --functions-version 4 `
  --name meetings-api-YOUR_UNIQUE_NAME `
  --storage-account YOUR_STORAGE_ACCOUNT

# Ustaw connection string Cosmos DB
az functionapp config appsettings set `
  --name meetings-api-YOUR_UNIQUE_NAME `
  --resource-group YOUR_RESOURCE_GROUP `
  --settings "CosmosDBConnection=AccountEndpoint=https://TWOJA-COSMOS-DB.documents.azure.com:443/;AccountKey=TWOJ-ACCOUNT-KEY-TUTAJ=="

# Wdróż kod
func azure functionapp publish meetings-api-YOUR_UNIQUE_NAME
```

### 7.3 Wdróż Frontend

**Opcja A: Azure Static Web Apps**
```powershell
cd frontend

# Zainstaluj CLI
npm install -g @azure/static-web-apps-cli

# Wdróż
swa deploy --app-location . --deployment-token YOUR_DEPLOYMENT_TOKEN
```

**Opcja B: Azure Storage Static Website**
```powershell
# Stwórz Storage Account
az storage account create `
  --name meetingsfrontendXXX `
  --resource-group YOUR_RESOURCE_GROUP `
  --location westeurope `
  --sku Standard_LRS

# Włącz static website
az storage blob service-properties update `
  --account-name meetingsfrontendXXX `
  --static-website `
  --index-document index.html

# Upload plików
az storage blob upload-batch `
  --account-name meetingsfrontendXXX `
  --source . `
  --destination '$web'
```

### 7.4 Zaktualizuj URL API w frontendzie

W `app.js` zmień:
```javascript
const API_BASE_URL = 'https://meetings-api-YOUR_UNIQUE_NAME.azurewebsites.net/api';
```

---

## 📚 Dodatkowe funkcjonalności (rozszerzenia)

### 1. Usuwanie spotkań
Dodaj funkcję DELETE do `api/DeleteAppointment/index.js`

### 2. Edycja dostępności osoby
Dodaj funkcję PATCH do aktualizacji dostępności

### 3. Powiadomienia email
Zintegruj z SendGrid lub Azure Communication Services

### 4. Autoryzacja
Dodaj Azure AD B2C do uwierzytelniania użytkowników

---

## 🐛 Rozwiązywanie problemów

### Problem: Błąd CORS
**Rozwiązanie:** Dodaj w `local.settings.json`:
```json
"Host": {
  "CORS": "*"
}
```

### Problem: Baza danych nie odpowiada
**Rozwiązanie:** Sprawdź:
- Connection string w `local.settings.json`
- Czy baza i kontenery zostały utworzone w Cosmos DB
- Czy masz dostęp do internetu

### Problem: Frontend nie łączy się z API
**Rozwiązanie:**
- Sprawdź czy Azure Functions działa (`func start`)
- Sprawdź URL API w `app.js`
- Sprawdź konsolę przeglądarki (F12)

---

## ✅ Kryteria oceny

**Wersja na 4 (bazowa):**
- ✅ Dodawanie osób
- ✅ Pobieranie listy osób
- ✅ Pobieranie dostępnych terminów
- ✅ Rezerwacja spotkań
- ✅ Wyświetlanie zarezerwowanych spotkań

**Wersja na 5:**
- ✅ Wszystko z wersji na 4
- ✅ Konfigurowalna dostępność dla każdej osoby
- ✅ Różne dni i godziny pracy
- ✅ Frontend z możliwością ustawiania dostępności

---

## 📦 Struktura finalna

```
Lab5/
├── api/
│   ├── AddPerson/
│   ├── GetPersons/
│   ├── GetAvailableSlots/
│   ├── BookAppointment/
│   ├── GetAppointments/
│   ├── host.json
│   ├── local.settings.json
│   └── package.json
├── frontend/
│   ├── index.html
│   ├── styles.css
│   └── app.js
└── INSTRUKCJA.md
```

---

## 🎓 Czego się nauczysz?

1. ✅ Tworzenie serverless API w Azure Functions
2. ✅ Praca z Azure Cosmos DB (NoSQL)
3. ✅ Integracja frontendu z backend API
4. ✅ Obsługa requestów HTTP (GET, POST)
5. ✅ Zarządzanie stanem aplikacji w JavaScript
6. ✅ Wdrażanie aplikacji na Azure

---

**Powodzenia! 🚀**
