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

    const cosmosClient = getCosmosClient();
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

    context.log(`Person added: ${person.id}`);

    return {
      status: 201,
      body: person
    };
  } catch (error) {
    context.log(`Error in AddPerson: ${error.message}`);
    return {
      status: 500,
      body: { error: `Failed to add person: ${error.message}` }
    };
  }
};
