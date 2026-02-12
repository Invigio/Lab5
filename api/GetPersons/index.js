module.exports = async function (context, req) {
  try {
    const cosmosClient = context.bindings.cosmosClient;
    const database = cosmosClient.database("SpotkaniaDB");
    const container = database.container("Persons");

    const { resources } = await container.items.readAll().fetchAll();

    return {
      status: 200,
      body: resources
    };
  } catch (error) {
    context.log(`Error in GetPersons: ${error.message}`);
    return {
      status: 500,
      body: { error: `Failed to get persons: ${error.message}` }
    };
  }
};
