namespace Infrastructure.Neo4j;
using System;

public class Neo4jSettings
{
    public Uri Neo4jConnection { get; set; }
    public string Neo4juser { get; set; }
    public string Neo4jPassword { get; set; }
    public string Neo4jDatabase { get; set; }
}
