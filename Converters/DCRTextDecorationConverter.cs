using PIPlanner.DataModel;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PIPlanner.Converters
{
    class DCRTextDecorationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           DCRStatus dcr = (DCRStatus)value;
            if (dcr == DCRStatus.Approved)
                return null;
            return TextDecorations.Strikethrough;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
