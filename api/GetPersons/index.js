const { getCosmosClient } = require("../cosmosClient");

module.exports = async function (context, req) {
  try {
    context.log("GetPersons: Attempting to get Cosmos client...");
    const cosmosClient = getCosmosClient();
    context.log("GetPersons: Cosmos client obtained");

    const database = cosmosClient.database("SpotkaniaDB");
    const container = database.container("Persons");

    const { resources } = await container.items.readAll().fetchAll();

    context.log(`GetPersons: Retrieved ${resources.length} persons`);

    return {
      status: 200,
      body: resources
    };
  } catch (error) {
    context.log(`GetPersons ERROR: ${error.message}`);
    context.log(`Stack: ${error.stack}`);
    return {
      status: 500,
      body: {
        error: `Failed to get persons: ${error.message}`,
        details: error.stack
      }
    };
  }
};
