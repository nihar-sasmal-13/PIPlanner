using PIPlanner.DataModel;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PIPlanner.Converters
{
    class CRItemsSummation : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ReadOnlyObservableCollection<Object> items = value as ReadOnlyObservableCollection<Object>;
            if (items != null)
                return items.Sum(i => (i as ChangeRequest)?.SPs);
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
