namespace Infrastructure.Neo4j;
public interface INeo4jDataAccess
{
    Task<List<string>> ExecuteReadListAsync(string query, string returnObjectKey, IDictionary<string, object>? parameters = null);
    Task ExecuteWriteAsync(string query, IDictionary<string, object>? parameters = null);
    Task ClearDatabaseAsync();

}