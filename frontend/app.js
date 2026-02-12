// Konfiguracja API
// Dla Azure (produkcja) - używaj względnej ścieżki
const API_BASE_URL = '/api';

// Dla lokalnego testowania odkomentuj linię poniżej:
// const API_BASE_URL = 'http://localhost:7071/api';

// Stan aplikacji
let currentPersons = [];
let selectedSlot = null;
let selectedPersonId = null;

// Mapowanie dni tygodnia
const DAYS_MAP = {
    'monday': 'Monday',
    'tuesday': 'Tuesday',
    'wednesday': 'Wednesday',
    'thursday': 'Thursday',
    'friday': 'Friday',
    'saturday': 'Saturday',
    'sunday': 'Sunday'
};

const DAYS_PL = {
    'Monday': 'Poniedziałek',
    'Tuesday': 'Wtorek',
    'Wednesday': 'Środa',
    'Thursday': 'Czwartek',
    'Friday': 'Piątek',
    'Saturday': 'Sobota',
    'Sunday': 'Niedziela'
};

// Inicjalizacja po załadowaniu DOM
document.addEventListener('DOMContentLoaded', () => {
    initializeApp();
    setupEventListeners();
});

// Inicjalizacja aplikacji
function initializeApp() {
    loadPersons();
    setMinDate();
}

// Ustawienie minimalnej daty na dziś
function setMinDate() {
    const today = new Date().toISOString().split('T')[0];
    document.getElementById('bookingDate').min = today;
}

// Konfiguracja event listenerów
function setupEventListeners() {
    // Formularz dodawania osoby
    document.getElementById('addPersonForm').addEventListener('submit', handleAddPerson);

    // Formularz rezerwacji
    document.getElementById('bookingForm').addEventListener('submit', handleBookAppointment);
    document.getElementById('selectPerson').addEventListener('change', handlePersonSelect);
    document.getElementById('bookingDate').addEventListener('change', handleDateSelect);

    // Filtr spotkań
    document.getElementById('filterPerson').addEventListener('change', loadAppointments);

    // Checkboxy dni tygodnia - włącz/wyłącz pola czasu
    Object.keys(DAYS_MAP).forEach(day => {
        const checkbox = document.getElementById(day);
        const startInput = document.getElementById(`${day}-start`);
        const endInput = document.getElementById(`${day}-end`);

        checkbox.addEventListener('change', () => {
            startInput.disabled = !checkbox.checked;
            endInput.disabled = !checkbox.checked;
        });
    });
}

// ========== OSOBY ==========

// Dodawanie osoby
async function handleAddPerson(e) {
    e.preventDefault();

    const name = document.getElementById('personName').value;
    const email = document.getElementById('personEmail').value;

    // Zbierz godziny pracy
    const workHours = {};
    for (const [dayId, dayName] of Object.entries(DAYS_MAP)) {
        const checkbox = document.getElementById(dayId);
        const start = document.getElementById(`${dayId}-start`).value;
        const end = document.getElementById(`${dayId}-end`).value;

        workHours[dayName] = {
            start: start,
            end: end,
            enabled: checkbox.checked
        };
    }

    const personData = {
        name: name,
        email: email,
        workHours: workHours
    };

    try {
        const response = await fetch(`${API_BASE_URL}/persons`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(personData)
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || 'Błąd podczas dodawania osoby');
        }

        const result = await response.json();
        showToast('Osoba dodana pomyślnie!', 'success');

        // Reset formularza
        document.getElementById('addPersonForm').reset();

        // Zaznacz ponownie dni robocze
        ['monday', 'tuesday', 'wednesday', 'thursday', 'friday'].forEach(day => {
            document.getElementById(day).checked = true;
        });

        // Odśwież listę osób
        await loadPersons();
    } catch (error) {
        showToast(`Błąd: ${error.message}`, 'error');
        console.error('Error adding person:', error);
    }
}

// Pobieranie listy osób
async function loadPersons() {
    try {
        const response = await fetch(`${API_BASE_URL}/persons`);

        if (!response.ok) {
            throw new Error('Błąd podczas pobierania osób');
        }

        currentPersons = await response.json();
        displayPersons();
        updatePersonSelects();
    } catch (error) {
        document.getElementById('personsList').innerHTML =
            '<p class="empty-state">Nie udało się załadować osób</p>';
        console.error('Error loading persons:', error);
    }
}

// Wyświetlanie listy osób
function displayPersons() {
    const container = document.getElementById('personsList');

    if (currentPersons.length === 0) {
        container.innerHTML = '<p class="empty-state">Brak osób w systemie. Dodaj pierwszą osobę powyżej.</p>';
        return;
    }

    container.innerHTML = currentPersons.map(person => {
        const workDays = Object.entries(person.workHours)
            .filter(([_, day]) => day.enabled)
            .map(([dayName, day]) => {
                const dayPL = DAYS_PL[dayName];
                return `<span class="work-day" title="${day.start} - ${day.end}">${dayPL}</span>`;
            })
            .join('');

        const appointmentsCount = person.bookedSlots ? person.bookedSlots.length : 0;

        return `
            <div class="person-card">
                <h4>${person.name}</h4>
                <p><strong>Email:</strong> ${person.email}</p>
                <p><strong>Zarezerwowanych spotkań:</strong> ${appointmentsCount}</p>
                <div class="work-hours-summary">
                    <strong>Dni przyjęć:</strong><br>
                    ${workDays || '<span style="color: #999;">Brak dostępnych dni</span>'}
                </div>
            </div>
        `;
    }).join('');
}

// Aktualizacja selectów z osobami
function updatePersonSelects() {
    const bookingSelect = document.getElementById('selectPerson');
    const filterSelect = document.getElementById('filterPerson');

    const options = currentPersons.map(person =>
        `<option value="${person.id}">${person.name} (${person.email})</option>`
    ).join('');

    bookingSelect.innerHTML = '<option value="">-- Wybierz osobę --</option>' + options;
    filterSelect.innerHTML = '<option value="">Wszystkie spotkania</option>' + options;
}

// ========== DOSTĘPNOŚĆ ==========

// Wybór osoby w formularzu rezerwacji
function handlePersonSelect() {
    selectedPersonId = document.getElementById('selectPerson').value;
    const date = document.getElementById('bookingDate').value;

    if (selectedPersonId && date) {
        loadAvailableSlots(selectedPersonId, date);
    } else {
        hideSlots();
    }
}

// Wybór daty w formularzu rezerwacji
function handleDateSelect() {
    const date = document.getElementById('bookingDate').value;

    if (selectedPersonId && date) {
        loadAvailableSlots(selectedPersonId, date);
    } else {
        hideSlots();
    }
}

// Pobieranie dostępnych slotów
async function loadAvailableSlots(personId, date) {
    try {
        const response = await fetch(`${API_BASE_URL}/availability/${personId}?date=${date}`);

        if (!response.ok) {
            throw new Error('Błąd podczas pobierania dostępności');
        }

        const data = await response.json();
        displayAvailableSlots(data);
    } catch (error) {
        showToast(`Błąd: ${error.message}`, 'error');
        hideSlots();
        console.error('Error loading slots:', error);
    }
}

// Wyświetlanie dostępnych slotów
function displayAvailableSlots(data) {
    const container = document.getElementById('availableSlots');
    const slotsContainer = document.getElementById('slotsContainer');

    if (data.availableSlots.length === 0) {
        container.innerHTML = '<p class="empty-state">Brak dostępnych terminów w tym dniu</p>';
        slotsContainer.style.display = 'block';
        document.getElementById('bookButton').disabled = true;
        return;
    }

    container.innerHTML = data.availableSlots.map(slot =>
        `<button type="button" class="slot-btn" data-time="${slot}">${slot}</button>`
    ).join('');

    slotsContainer.style.display = 'block';

    // Event listenery dla slotów
    container.querySelectorAll('.slot-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            // Usuń zaznaczenie z innych
            container.querySelectorAll('.slot-btn').forEach(b => b.classList.remove('selected'));

            // Zaznacz wybrany
            btn.classList.add('selected');
            selectedSlot = btn.dataset.time;
            document.getElementById('bookButton').disabled = false;
        });
    });
}

// Ukrywanie slotów
function hideSlots() {
    document.getElementById('slotsContainer').style.display = 'none';
    document.getElementById('availableSlots').innerHTML = '';
    document.getElementById('bookButton').disabled = true;
    selectedSlot = null;
}

// ========== REZERWACJA ==========

// Rezerwacja spotkania
async function handleBookAppointment(e) {
    e.preventDefault();

    if (!selectedSlot) {
        showToast('Wybierz godzinę spotkania', 'warning');
        return;
    }

    const bookingData = {
        personId: selectedPersonId,
        date: document.getElementById('bookingDate').value,
        time: selectedSlot,
        clientName: document.getElementById('clientName').value,
        clientEmail: document.getElementById('clientEmail').value,
        description: document.getElementById('description').value || ''
    };

    try {
        const response = await fetch(`${API_BASE_URL}/book`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(bookingData)
        });

        if (!response.ok) {
            const error = await response.json();
            if (response.status === 409) {
                showToast('Ten termin został już zarezerwowany', 'warning');
                // Odśwież sloty
                loadAvailableSlots(selectedPersonId, bookingData.date);
            } else {
                throw new Error(error.error || 'Błąd podczas rezerwacji');
            }
            return;
        }

        showToast('Spotkanie zarezerwowane pomyślnie!', 'success');

        // Reset formularza
        document.getElementById('bookingForm').reset();
        hideSlots();
        selectedPersonId = null;
        selectedSlot = null;

        // Odśwież listę spotkań
        await loadAppointments();
        await loadPersons(); // Odśwież też liczniki spotkań
    } catch (error) {
        showToast(`Błąd: ${error.message}`, 'error');
        console.error('Error booking appointment:', error);
    }
}

// ========== SPOTKANIA ==========

// Pobieranie spotkań
async function loadAppointments() {
    const filterPersonId = document.getElementById('filterPerson').value;
    const container = document.getElementById('appointmentsList');

    try {
        let url = `${API_BASE_URL}/persons`;
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error('Błąd podczas pobierania spotkań');
        }

        const persons = await response.json();

        // Zbierz wszystkie spotkania
        let allAppointments = [];
        persons.forEach(person => {
            if (person.bookedSlots && person.bookedSlots.length > 0) {
                person.bookedSlots.forEach(slot => {
                    allAppointments.push({
                        ...slot,
                        personName: person.name,
                        personId: person.id
                    });
                });
            }
        });

        // Filtruj jeśli wybrano osobę
        if (filterPersonId) {
            allAppointments = allAppointments.filter(apt => apt.personId === filterPersonId);
        }

        // Sortuj po dacie i czasie
        allAppointments.sort((a, b) => {
            const dateCompare = a.date.localeCompare(b.date);
            if (dateCompare !== 0) return dateCompare;
            return a.time.localeCompare(b.time);
        });

        displayAppointments(allAppointments);
    } catch (error) {
        container.innerHTML = '<p class="empty-state">Nie udało się załadować spotkań</p>';
        console.error('Error loading appointments:', error);
    }
}

// Wyświetlanie spotkań
function displayAppointments(appointments) {
    const container = document.getElementById('appointmentsList');

    if (appointments.length === 0) {
        container.innerHTML = '<p class="empty-state">Brak zarezerwowanych spotkań</p>';
        return;
    }

    container.innerHTML = appointments.map(apt => {
        const dateObj = new Date(apt.date);
        const formattedDate = dateObj.toLocaleDateString('pl-PL', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });

        return `
            <div class="appointment-card">
                <h4>📅 ${formattedDate}</h4>
                <p><strong>Godzina:</strong> ${apt.time}</p>
                <p><strong>Z:</strong> ${apt.personName}</p>
                <p><strong>Klient:</strong> ${apt.clientName}</p>
                <p><strong>Email:</strong> ${apt.clientEmail}</p>
                ${apt.description ? `<p><strong>Opis:</strong> ${apt.description}</p>` : ''}
                <p style="font-size: 0.85rem; color: #999; margin-top: 10px;">
                    Zarezerwowano: ${new Date(apt.bookedAt).toLocaleString('pl-PL')}
                </p>
            </div>
        `;
    }).join('');
}

// ========== TOAST NOTIFICATIONS ==========

function showToast(message, type = 'success') {
    const toast = document.getElementById('toast');
    toast.textContent = message;
    toast.className = `toast ${type} show`;

    setTimeout(() => {
        toast.classList.remove('show');
    }, 5000);
}

// Auto-ładowanie spotkań co 30 sekund
setInterval(() => {
    loadAppointments();
}, 30000);

// Początkowe załadowanie spotkań
loadAppointments();
