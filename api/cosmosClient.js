const { CosmosClient } = require("@azure/cosmos");

let cosmosClient = null;

function getCosmosClient() {
  if (!cosmosClient) {
    // Try multiple possible environment variable names
    const connectionString =
      process.env.CosmosDbConnectionString ||
      process.env.COSMOS_DB_CONNECTION_STRING ||
      process.env.CosmosConnectionString;

    if (!connectionString) {
      const availableEnvVars = Object.keys(process.env)
        .filter(key => key.toLowerCase().includes('cosmos') || key.toLowerCase().includes('connection'))
        .join(", ");

      throw new Error(
        `CosmosDbConnectionString not configured. ` +
        `Available env vars with 'cosmos' or 'connection': ${availableEnvVars || 'NONE'}`
      );
    }

    cosmosClient = new CosmosClient(connectionString);
  }

  return cosmosClient;
}

module.exports = { getCosmosClient };
