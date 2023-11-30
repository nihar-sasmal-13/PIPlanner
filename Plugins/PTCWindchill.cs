using PIPlanner.DataModel;
using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static PIPlanner.Plugins.IDataSource;

namespace PIPlanner.Plugins
{
    internal class PTCWindchill : PluginBase
    {
        private const string HOSTNAME = "Hostname";
        private const string PORT = "Port";
        private const string USERNAME = "Username";
        private const string PASSWORD = "Password";
        private const string RELEASES = "Release Ids (comma separated)";        

        public override void FetchData(List<KeyValue>? inputs, ContentToFetch fetch = ContentToFetch.Everything)
        {
            inputs = getInputsFromUser(new List<KeyValue> {
                new KeyValue(HOSTNAME, ""),
                new KeyValue(PORT, ""),
                new KeyValue(USERNAME, ""),
                new KeyValue(PASSWORD, ""),
                new KeyValue(RELEASES, "")
                });

            string connection = $"--hostname={inputs.GetValue(HOSTNAME)} " +
                $"--port={inputs.GetValue(PORT)} " +
                $"--user={inputs.GetValue(USERNAME)} " +
                $"--password={inputs.GetValue(PASSWORD)}";

            var releases = inputs.GetValue(RELEASES)
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var releaseFilters = releases.Select(r => $"(field[Planned Release]={r})");

            //if ((fetch & ContentToFetch.CRsOnly) != 0)
            if (fetch.HasFlag(ContentToFetch.CRsOnly))
            {
                string query = $"((field[Type]=Change Request) and ({string.Join(" or ", releaseFilters)}))";
                List<string> crFields = new List<string> { "ID", "Summary", "Project", "Custom Integer", "Functional Classification", "Planned Release", "Sprint", "Assigned Team" };
                var crs = executeQuery(crFields, query, connection);
                ChangeRequests = crs.Select(cr => new ChangeRequest
                {
                    Id = cr[0],
                    Summary = cr[1],
                    Project = cr[2],
                    SPs = string.IsNullOrEmpty(cr[3]) ? 0 : int.Parse(cr[3]),
                    FunctionalArea = cr[4],
                    Release = cr[5],
                }).ToList();
            }
        }

        private List<List<string>> executeQuery(List<string> fields, string query, string connection)
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = "im",
                    Arguments = $"issues --fields=\"{string.Join(",", fields)}\" --fieldsDelim=\"|\" --queryDefinition=\"{query}\" {connection}"
                }
            };
            List<List<string>> output = new List<List<string>>();
            string error = "";
            proc.OutputDataReceived += (sender, args) =>
            {
                if (args != null && args.Data != null)
                    output.Add(args.Data.Split(new string[] { "|" }, StringSplitOptions.None).ToList());
            };
            proc.ErrorDataReceived += (sender, args) => error += args.Data;
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            if (!string.IsNullOrEmpty(error))
                Console.WriteLine(error);
            return output;
        }
    }
}
