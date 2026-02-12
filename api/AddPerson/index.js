const { getCosmosClient } = require("../cosmosClient");

module.exports = async function (context, req) {
  try {
    const { name, email, workHours } = req.body;

    if (!name || !email || !workHours) {
      return {
        status: 400,
        body: { error: "Missing required fields: name, email, workHours" }
      };
    }

    context.log("AddPerson: Attempting to get Cosmos client...");
    const cosmosClient = getCosmosClient();
    context.log("AddPerson: Cosmos client obtained, connecting to database...");

    const database = cosmosClient.database("SpotkaniaDB");
    const container = database.container("Persons");

    const person = {
      id: Date.now().toString(),
      name,
      email,
      workHours,
      bookedSlots: [],
      createdAt: new Date().toISOString()
    };

    await container.items.create(person);

    context.log(`Person added successfully: ${person.id}`);

    return {
      status: 201,
      body: person
    };
  } catch (error) {
    context.log(`AddPerson ERROR: ${error.message}`);
    context.log(`Stack: ${error.stack}`);
    return {
      status: 500,
      body: {
        error: `Failed to add person: ${error.message}`,
        details: error.stack
      }
    };
  }
};
