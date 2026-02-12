const { CosmosClient } = require("@azure/cosmos");

let cosmosClient = null;

function getCosmosClient() {
  if (!cosmosClient) {
    const connectionString = process.env.CosmosDbConnectionString;

    if (!connectionString) {
      throw new Error("CosmosDbConnectionString is not configured");
    }

    cosmosClient = new CosmosClient(connectionString);
  }

  return cosmosClient;
}

module.exports = { getCosmosClient };
