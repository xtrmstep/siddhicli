namespace SiddhiCli.Services
{
    public interface ISiddhiAppsService
    {
        string GetMeta(string appName);
        string GetState(string appName, string tableName);
        string Uninstall(string appName);
        string Install(string appName);
        string GetDeployed();
        string List();
        string InstallAllInCurrentFolder();
        string UninstallAll();
    }
}