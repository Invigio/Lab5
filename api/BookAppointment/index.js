module.exports = async function (context, req) {
  try {
    const { personId, date, time, clientName, clientEmail } = req.body;

    if (!personId || !date || !time || !clientName || !clientEmail) {
      return {
        status: 400,
        body: { error: "Missing required fields" }
      };
    }

    const cosmosClient = context.bindings.cosmosClient;
    const database = cosmosClient.database("SpotkaniaDB");
    const container = database.container("Persons");

    const { resources } = await container.items
      .query(`SELECT * FROM c WHERE c.id = "${personId}"`)
      .fetchAll();

    if (resources.length === 0) {
      return {
        status: 404,
        body: { error: "Person not found" }
      };
    }

    const person = resources[0];

    // Check if slot is already booked
    const isBooked = person.bookedSlots.some(
      slot => slot.date === date && slot.time === time
    );

    if (isBooked) {
      return {
        status: 409,
        body: { error: "Slot already booked" }
      };
    }

    // Validate work hours
    const requestDate = new Date(date);
    const dayName = getDayName(requestDate.getDay());
    const daySchedule = person.workHours[dayName];

    if (!daySchedule || !daySchedule.enabled) {
      return {
        status: 400,
        body: { error: "No work hours for this day" }
      };
    }

    if (!isTimeInRange(time, daySchedule.start, daySchedule.end)) {
      return {
        status: 400,
        body: { error: "Time is outside work hours" }
      };
    }

    // Add booking
    const newSlot = {
      date,
      time,
      clientName,
      clientEmail,
      bookedAt: new Date().toISOString()
    };

    person.bookedSlots.push(newSlot);

    // Update person in database
    await container.item(personId, personId).replace(person);

    return {
      status: 201,
      body: {
        message: "Appointment booked successfully",
        booking: newSlot
      }
    };
  } catch (error) {
    context.log(`Error in BookAppointment: ${error.message}`);
    return {
      status: 500,
      body: { error: `Failed to book appointment: ${error.message}` }
    };
  }
};

function getDayName(dayIndex) {
  const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
  return days[dayIndex];
}

function isTimeInRange(time, startTime, endTime) {
  const [h, m] = time.split(":").map(Number);
  const timeMinutes = h * 60 + m;

  const [sh, sm] = startTime.split(":").map(Number);
  const startMinutes = sh * 60 + sm;

  const [eh, em] = endTime.split(":").map(Number);
  const endMinutes = eh * 60 + em;

  return timeMinutes >= startMinutes && timeMinutes < endMinutes;
}
