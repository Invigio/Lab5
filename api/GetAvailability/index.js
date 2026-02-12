module.exports = async function (context, req) {
  try {
    const personId = context.bindingData.id;
    const date = req.query.date;

    if (!personId || !date) {
      return {
        status: 400,
        body: { error: "Missing personId or date parameter" }
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
    const requestDate = new Date(date);
    const dayName = getDayName(requestDate.getDay());
    const daySchedule = person.workHours[dayName];

    if (!daySchedule || !daySchedule.enabled) {
      return {
        status: 200,
        body: { slots: [], message: "No work hours for this day" }
      };
    }

    const slots = generateTimeSlots(daySchedule.start, daySchedule.end);
    const availableSlots = filterBookedSlots(slots, person.bookedSlots, date);

    return {
      status: 200,
      body: {
        personId,
        date,
        slots: availableSlots
      }
    };
  } catch (error) {
    context.log(`Error in GetAvailability: ${error.message}`);
    return {
      status: 500,
      body: { error: `Failed to get availability: ${error.message}` }
    };
  }
};

function getDayName(dayIndex) {
  const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
  return days[dayIndex];
}

function generateTimeSlots(startTime, endTime) {
  const slots = [];
  const [startHour, startMin] = startTime.split(":").map(Number);
  const [endHour, endMin] = endTime.split(":").map(Number);

  let currentHour = startHour;
  let currentMin = startMin;
  const endTotalMin = endHour * 60 + endMin;

  while (currentHour * 60 + currentMin < endTotalMin) {
    const timeStr = `${String(currentHour).padStart(2, "0")}:${String(currentMin).padStart(2, "0")}`;
    slots.push(timeStr);
    currentMin += 30;
    if (currentMin >= 60) {
      currentMin -= 60;
      currentHour += 1;
    }
  }

  return slots;
}

function filterBookedSlots(availableSlots, bookedSlots, date) {
  const bookedTimes = bookedSlots
    .filter(slot => slot.date === date)
    .map(slot => slot.time);

  return availableSlots.filter(slot => !bookedTimes.includes(slot));
}
