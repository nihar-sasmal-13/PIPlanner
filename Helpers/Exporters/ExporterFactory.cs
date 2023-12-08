using PIPlanner.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPlanner.Helpers.Exporters
{
    interface IExporter
    {
        void Export(PlanViewModel plan, string outFilePath);
    }

    enum ExportTypes
    {
        HTML,
        CSV
    }

    static class ExporterFactory
    {
        public static IExporter GetExporter(ExportTypes exportType)
        {
            return exportType == ExportTypes.HTML ? 
                new HTMLExporter() : 
                new CSVExporter();
        }

        public static Tuple<string, string> GetExtensionForExportType(ExportTypes exportType)
        {
            return exportType == ExportTypes.HTML ?
                new Tuple<string, string>(".html", "Html (.html)|*.html") :
                new Tuple<string, string>(".csv", "Comma Separated Values (.csv)|*.csv");
        }
    }
}
