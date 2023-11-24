using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace PIPlanner.Converters
{
    class FunctionalAreaMneumonic : IValueConverter
    {
        static Dictionary<string, string> _mneumonics = new Dictionary<string, string>
        {
            { "", "NA" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string fa = value as string;
            if (string.IsNullOrEmpty(fa)) return _mneumonics[""];
            else if (!_mneumonics.ContainsKey(fa))
                _mneumonics.Add(fa, FunctionalAreaHelper.MakeMneumonic(fa));
            return _mneumonics[fa];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
