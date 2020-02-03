using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SiddhiCli.Services
{
    public class SiddhiAppsService : ISiddhiAppsService
    {
        private readonly ILogger<SiddhiAppsService> _logger;
        private readonly ISiddhiApiClient _siddhiApiClient;

        public SiddhiAppsService(ISiddhiApiClient siddhiApiClient, ILogger<SiddhiAppsService> logger)
        {
            _siddhiApiClient = siddhiApiClient;
            _logger = logger;
        }

        public string GetMeta(string appName)
        {
            var files = GetSiddhiFiles();
            if (files.Any())
            {
                foreach (var file in files)
                {
                    var siddhiAppName = GetSiddhiAppName(file);
                    if (siddhiAppName != appName) continue;

                    var tables = GetTablesFromSiddhiApp(file);
                    if (!tables.Any()) return "None";

                    var sb = new StringBuilder();
                    sb.AppendLine("Tables:");
                    foreach (var table in tables) sb.AppendLine($" - {table}");

                    return sb.ToString();
                }

                return $"ERROR: Cannot find Siddhi app : {appName}";
            }

            return "ERROR: There are no Siddhi apps in current folder";
        }

        public string GetState(string appName, string tableName)
        {
            var jsonResult = _siddhiApiClient.ExecQuery(appName, tableName);
            var queryResult = JsonConvert.DeserializeObject<OnDemandQueryResult>(jsonResult);
            var sb = new StringBuilder();
            foreach (var record in queryResult.Records)
            {
                var sep = string.Empty;
                foreach (var item in record)
                {
                    sb.Append($"{sep}{item}");
                    sep = ",";
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string Uninstall(string appName)
        {
            var result = _siddhiApiClient.Delete(appName);
            return result;
        }

        public string UninstallAll()
        {
            var applications = GetDeployedApplications();
            if (applications.Any())
            {
                var sp = new StringBuilder();
                foreach (var siddhiAppName in applications)
                {
                    var result = _siddhiApiClient.Delete(siddhiAppName);
                    sp.AppendLine($"{siddhiAppName}: {result}");
                }

                return sp.ToString();
            }

            return "There are no Siddhi apps deployed";
        }

        public string InstallAllInCurrentFolder()
        {
            var files = GetSiddhiFiles();
            if (files.Any())
            {
                var sp = new StringBuilder();
                foreach (var file in files)
                {
                    var siddhiAppName = GetSiddhiAppName(file);
                    var deployResult = _siddhiApiClient.Deploy(file);
                    sp.AppendLine($"{siddhiAppName}: {deployResult}");
                }

                return sp.ToString();
            }

            return "ERROR: There are no Siddhi apps in current folder";
        }

        public string Install(string appName)
        {
            var files = GetSiddhiFiles();
            if (files.Any())
            {
                foreach (var file in files)
                {
                    var siddhiAppName = GetSiddhiAppName(file);
                    if (siddhiAppName == appName) return _siddhiApiClient.Deploy(file);
                }

                return $"ERROR: Cannot find Siddhi app : {appName}";
            }

            return "ERROR: There are no Siddhi apps in current folder";
        }

        public string GetDeployed()
        {
            var applications = GetDeployedApplications();
            if (applications.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("Deployed applications:");
                foreach (var app in applications) sb.AppendLine($" - {app}");

                return sb.ToString();
            }

            return "No deployed applications found.";
        }

        public string List()
        {
            var files = GetSiddhiFiles();

            var sb = new StringBuilder();
            sb.AppendLine("Local Siddhi applications:");
            if (files.Any())
                foreach (var file in files)
                {
                    var siddhiAppName = GetSiddhiAppName(file);
                    if (!string.IsNullOrEmpty(siddhiAppName)) sb.AppendLine($" - {siddhiAppName}");
                }
            else
                sb.AppendLine(" None");

            return sb.ToString();
        }

        private string[] GetTablesFromSiddhiApp(string file)
        {
            var text = File.ReadAllText(file);
            var matches = Regex.Matches(text, @"define *table *(\w+) *\(");

            if (!matches.Any()) return new string[] { };

            var result = new List<string>();
            foreach (Match match in matches) result.Add(match.Groups[1].Value);
            return result.ToArray();
        }

        private string[] GetDeployedApplications()
        {
            var result = _siddhiApiClient.GetActiveApps();
            var applications = JsonConvert.DeserializeObject<string[]>(result);
            return applications;
        }

        private static string GetSiddhiAppName(string file)
        {
            using var inputFile = new StreamReader(file);
            var appNameLine = inputFile.ReadLine();
            if (string.IsNullOrEmpty(appNameLine)) return string.Empty;
            /*
            * The Siddhi app name suppose to follow the pattern:
            * @App:name('application_name')
            * SO that take the app name with regex
            */
            var match = Regex.Match(appNameLine, @"^@App:name\(['""](\w+)['""]\)");
            if (match.Success) return match.Groups[1].Value;

            return string.Empty;
        }

        private static string[] GetSiddhiFiles()
        {
            var workingFolder = Directory.GetCurrentDirectory();
            var files = Directory.GetFiles(workingFolder, "*.siddhi");
            return files;
        }

        private class OnDemandQueryResult
        {
            public string[][] Records { get; set; }
        }
    }
}