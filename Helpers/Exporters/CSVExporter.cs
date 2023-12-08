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
                    .ForEach(cr => writer.WriteLine($"{cr.SprintId},{cr.Id},{cr.Summary},{cr.SPs},{cr.Team?.Name},"));

                //now write the unplanned CRs
                plan.ChangeRequests
                    .Where(cr => cr.SprintId == null)
                    .OrderBy(cr => cr.FunctionalArea)
                    .ToList()
                    .ForEach(cr => writer.WriteLine($"Unplanned,{cr.Id},{cr.Summary},{cr.SPs},{cr.Team?.Name},"));
            }
        }
    }
}
