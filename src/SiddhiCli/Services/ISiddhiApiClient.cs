namespace SiddhiCli.Services
{
    public interface ISiddhiApiClient
    {
        string ExecQuery(string appName, string tableName);
        string GetActiveApps();
        string Delete(string appName);
        string Deploy(string file);
    }
}