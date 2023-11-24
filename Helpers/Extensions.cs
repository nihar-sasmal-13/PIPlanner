using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Media;

namespace PIPlanner.Helpers
{
    static class Extensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> coll, bool deepCopy = false)
        {
            return new ObservableCollection<T>(coll.Select(item => (deepCopy && item is ICloneable) ? (T)(item as ICloneable).Clone() : item));
        }

        public static Brush ToColor(this string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFrom(color);
        }

        public static ObservableCollection<DataTable> ToObservableCollection(this DataTableCollection tableCollection)
        {
            ObservableCollection<DataTable> tables = new ObservableCollection<DataTable>();
            for (int i = 0; i < tableCollection.Count; i++)
                tables.Add(tableCollection[i]);
            return tables;
        }

        public static ObservableCollection<DataColumn> ToObservableCollection(this DataColumnCollection columnCollection)
        {
            ObservableCollection<DataColumn> cols = new ObservableCollection<DataColumn>();
            for (int i = 0; i < columnCollection.Count; i++)
                cols.Add(columnCollection[i]);
            return cols;
        }

        public static T GetData<T>(this DataRow dataRow, DataColumn column, T defaultValue)
        {
            if (column == null)
                return defaultValue;
            object value = dataRow[column.ColumnName];
            if (value == null || value is DBNull)
                return defaultValue;
            else return (T)Convert.ChangeType(value, typeof(T));
        }

        public static string GetData(this DataRow dataRow, DataColumn column, bool split = false, string separator = ",")
        {
            if (column == null)
                return "";
            object value = dataRow[column.ColumnName];
            if (value == null || value is DBNull)
                return "";
            string str = value.ToString();
            if (!split || !str.Contains(separator))
                return str;
            else
                return str.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        public static string GetValue(this List<KeyValue> keyValues, string key)
        {
            return keyValues.FirstOrDefault(kv => kv.Key == key)?.Value;
        }
    }
}
