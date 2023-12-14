using PIPlanner.DataModel;
using PIPlanner.ViewModels;
using System.IO;
using System.Linq;

namespace PIPlanner.Helpers.Exporters
{
    class CSVExporter : IExporter
    {
        public void Export(PlanViewModel plan, string outFilePath)
        {
            using (StreamWriter writer = new StreamWriter(outFilePath))
            {
                writer.WriteLine("Sprint,ID,Summary,SPs,Team,");

                //write the planned CRs first
                plan.ChangeRequests
                    .Where(cr => cr.SprintId != null)
                    .OrderBy(cr => cr.SprintId)
                    .ToList()
                    .ForEach(cr => writer.WriteLine($"{cr.SprintId},{cr.Id},\"{cr.Summary}\",{cr.SPs},{cr.Team?.Name},"));

                //now write the unplanned CRs
                plan.ChangeRequests
                    .Where(cr => cr.SprintId == null)
                    .OrderBy(cr => cr.FunctionalArea)
                    .ToList()
                    .ForEach(cr => writer.WriteLine($"Unplanned,{cr.Id},\"{cr.Summary}\",{cr.SPs},{cr.Team?.Name},"));
            }
        }

        public void Export(ScrumViewModel scrum, string outFilePath)
        {
            using (StreamWriter writer = new StreamWriter(outFilePath))
            {
                //Basic information
                writer.WriteLine(scrum.SelectedSprint.Name);
                writer.WriteLine();
                writer.WriteLine($"Start Date,{scrum.SelectedSprint.StartDate.ToString("dd-MMM-yyyy")}");
                writer.WriteLine($"End Date,{scrum.SelectedSprint.EndDate.ToString("dd-MMM-yyyy")}");
                writer.WriteLine($"Status,{scrum.SelectedSprint.SprintSummary?.Status}");
                writer.WriteLine();

                //Summaries
                writer.WriteLine("Team,SprintAvailability,Assigned,Defects,MiscBandwidth,RemainingEffort,");
                foreach (var sprintTeam in scrum.SelectedSprintTeams)
                {
                    writer.WriteLine($"{sprintTeam.Team.Name},{sprintTeam.SprintAvailability}," +
                        $"{sprintTeam.Assigned},{sprintTeam.DefectBandwidth},{sprintTeam.MiscBandwidth},{sprintTeam.RemainingEffort},");
                }

                writer.WriteLine();
                writer.WriteLine("Sprint Summary,");
                writer.WriteLine("Key,Value,");
                foreach (var prop in scrum.SelectedSprint.SprintSummary.Properties)
                {
                    writer.WriteLine($"{prop.Key},{prop.Value},");
                }
                writer.WriteLine();

                //Content
                writer.WriteLine("State,DCRId,Summary,SPs,RemainingSPs,PlanningComments,");
                foreach (var content in scrum.SelectedSprintContent)
                {
                    writer.WriteLine($"{content.State},{content.DCRId},\"{content.ChangeRequest.Summary}\"," +
                        $"{content.ChangeRequest.SPs},{content.RemainingSPs},\"{content.PlanningComments}\",");
                }
            }
        }
    }
}
