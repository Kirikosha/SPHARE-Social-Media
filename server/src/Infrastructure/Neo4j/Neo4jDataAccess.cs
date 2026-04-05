namespace Infrastructure.Neo4j;

using global::Neo4j.Driver;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Neo4jDataAccess : INeo4jDataAccess
{
    private readonly IDriver _driver;
    private readonly string _database;

    public Neo4jDataAccess(IDriver driver, IOptions<Neo4jSettings> settings)
    {
        _driver = driver;
        _database = settings.Value.Neo4jDatabase ?? "neo4j";
    }

    public async Task ClearDatabaseAsync()
    {
        const string query = "MATCH (n) DETACH DELETE n";

        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));
        await session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(query);
        });
    }

    public async Task<List<string>> ExecuteReadListAsync(string query, string returnObjectKey, IDictionary<string, object>? parameters = null)
    {
        var resultList = new List<string>();

        try
        {
            await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(query, parameters);
                var records = await cursor.ToListAsync();
                return records;
            });

            foreach (var record in result)
            {
                resultList.Add(record[returnObjectKey].ToString());
            }
        }
        catch (Exception ex)
        {
            throw new Neo4jException("During reading executing subscription query error happened", ex);
        }

        return resultList;
    }

    public async Task ExecuteWriteAsync(string query, IDictionary<string, object>? parameters = null)
    {
        try
        {
            await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(query, parameters);
            });
        }
        catch (Exception ex)
        {
            throw new Neo4jException("During writing executing subscription query error happened", ex);
        }
    }
}

