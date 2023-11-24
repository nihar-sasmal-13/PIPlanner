using ExcelDataReader;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace PIPlanner.Helpers
{
    static class ExcelReader
    {
        static ExcelReader()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static DataTableCollection ReadExcelFile(string filePath)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    return result.Tables;
                }
            }
        }

        public static List<string> GetColumnNames(string filePath, string tableName)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    for (int i = 0; i < result.Tables.Count; i++)
                    {
                        if (result.Tables[i].TableName == tableName)
                        {
                            DataRow topRow = result.Tables[i].Rows[0];
                            return topRow.ItemArray.Select(item => item.ToString()).ToList();
                        }
                    }
                }
            }
            return new List<string>();
        }
    }
}
