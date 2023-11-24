using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using PIPlanner.DataModel;
using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static PIPlanner.Plugins.IDataSource;

namespace PIPlanner.Plugins
{
    internal class LegacyPIPlannerJson : PluginBase
    {
        public Plan Plan { get; private set; }

        public override void FetchData(List<KeyValue>? inputs, ContentToFetch fetch = ContentToFetch.Everything)
        {
            if (inputs == null || !inputs.Any(item => item.Key == "filepath")) return;

            using (StreamReader reader = new StreamReader(inputs.First(i => i.Key == "filepath").Value))
            {
                dynamic json = JsonConvert.DeserializeObject(reader.ReadToEnd());

                //Read plan metadata information
                int id = 1;
                string name = json.Name;
                string created = json.CreatedAt;
                string modified = json.LastModifiedAt;
                Plan = new Plan
                {
                    Id = id,
                    Name = name,
                    CreatedAt = json.CreatedAt,
                    LastModifiedAt = json.LastModifiedAt //DateTime.ParseExact(modified, "yyyy-MM-ddThh:mm:ss", CultureInfo.InvariantCulture)
                };

                //Read Change Requests
                ChangeRequests = new List<ChangeRequest>();
                foreach (var cr in json.ChangeRequests)
                {
                    string project = cr.Project;
                    string release = cr.Release;
                    string fc = cr.FunctionalArea;
                    string summary = cr.Summary;
                    ChangeRequests.Add(new ChangeRequest
                    {
                        Id = cr.ID,
                        Project = project ?? "Unknown",
                        Release = release ?? "Unknown",
                        FunctionalArea = fc ?? "Unknown",
                        SPs = cr.SPs,
                        Status = cr.Status,
                        Summary = summary ?? "Unknown",
                        SprintId = cr.SprintId <= 0 ? null : cr.SprintId,
                        TeamId = cr.TeamId <= 0 ? null : cr.TeamId
                    });
                }

                //Read Sprint information
                Sprints = new List<Sprint>();
                foreach (var sprint in json.Sprints)
                {
                    string sprintName = sprint.SprintName;
                    Sprints.Add(new Sprint
                    {
                        Id = sprint.SprintId,
                        Name = sprintName ?? "Unknown",
                        StartDate = sprint.StartDate,
                        EndDate = sprint.EndDate,
                    });
                }

                //Read Team information
                Teams = new List<Team>();
                foreach (var team in json.Teams)
                {
                    string teamName = team.TeamName;
                    Teams.Add(new Team
                    {
                        Id = team.Id,
                        Name = teamName ?? "Unknown",
                        TeamColor = team.TeamColor.ToString(),
                        Velocity = team.Velocity,
                    });
                }
            }
        }
    }
}
