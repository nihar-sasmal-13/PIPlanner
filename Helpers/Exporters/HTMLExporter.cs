using PIPlanner.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PIPlanner.Helpers.Exporters
{
    class HTMLExporter : IExporter
    {
        private static string _styles = @"
table, th, td {
  border: 1px solid black;
  border-collapse: collapse;
}
table {
  width: 100%
}
";

        public void Export(PlanViewModel plan, string outFilePath)
        {
            using (StreamWriter writer = new StreamWriter(outFilePath))
            {
                writer.WriteLine($"<html><head><title>{plan.PlanMetadata.Name}</title><style>{_styles}</style></head><body>");
                writer.WriteLine($"<br/><p><i>This report is generated using <b>PI Planner v{Assembly.GetExecutingAssembly().GetName().Version}</b> on <b>{DateTime.Now}</b></i></p><br/>");

                //write DCRs
                writer.WriteLine("<table>");
                //header
                writer.WriteLine("<tr><td>DCR #</td><td>Summary</td><td>SPs</td></tr>");
                //content
                foreach (var group in plan.ChangeRequests.GroupBy(cr => cr.FunctionalArea))
                {
                    writer.WriteLine($"<tr><td colspan='4'><b>{group.Key}</b></td></tr>");
                    foreach (var dcr in group)
                        writer.WriteLine($"<tr><td>{dcr.Id}</td><td>{dcr.Summary}</td><td>{dcr.SPs}</td></tr>");
                }
                writer.WriteLine("</table><br/><br/><br/><br/><br/>");

                //write plan
                writer.WriteLine("<table>");
                //header
                writer.Write("<tr style=\"font-size: 22px; text-align: center;\"><td></td>");
                foreach (var sprint in plan.Sprints)
                    writer.Write($"<td>{sprint.Name}</td>");
                writer.Write("</tr>");

                //content
                foreach (var team in plan.Teams)
                {
                    Color color = ((SolidColorBrush)team.TeamColor.ToColor()).Color;
                    writer.Write($"<tr style=\"background: #{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")};\">" +
                        $"<td style=\"font-size: 22px;\"><b>{team.Name}</b></td>");
                    foreach (var sprint in plan.Sprints)
                    {
                        writer.Write($"<td style=\"text-align: center;\">");
                        var crs = plan.ChangeRequests.Where(cr => cr.TeamId == team.Id && cr.SprintId == sprint.Id);
                        if (crs != null && crs.Any())
                        {
                            writer.Write("<ul>");
                            foreach (var cr in crs)
                                writer.Write($"<li><b>{cr.Id}</b> <i>SPs</i> <b>{cr.SPs}</b></li>");
                            writer.Write("</ul>");
                        }
                        writer.Write("</td>");
                    }
                    writer.WriteLine("</tr>");
                }
                writer.WriteLine("</table><br/><br/><br/><br/><br/>");
                writer.WriteLine("</body></html>");
            }
        }
    }
}
