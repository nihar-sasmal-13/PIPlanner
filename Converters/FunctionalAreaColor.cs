using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace PIPlanner.Converters
{
    class FunctionalAreaColor : IValueConverter
    {
        static List<Brush> _availableBrushes = new List<Brush>
        {
            Brushes.Black,
            Brushes.Red,
            new SolidColorBrush(Color.FromRgb(0xDC, 0xFF, 0x8C)),
            new SolidColorBrush(Color.FromRgb(0xB9, 0xFF, 0xFF)),
            new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0xFF)),
            new SolidColorBrush(Color.FromRgb(0xFF, 0x9D, 0x5F)),
            new SolidColorBrush(Color.FromRgb(0xEA, 0xEA, 0xEA)),
            Brushes.Gray,
            Brushes.Chocolate,
            Brushes.Pink,
            Brushes.SteelBlue,
            Brushes.Lime,
            Brushes.Yellow,            
            Brushes.BurlyWood,
            Brushes.Magenta,
            Brushes.Aquamarine,
            Brushes.RoyalBlue,
            Brushes.DeepSkyBlue,
            Brushes.Orange,
        };
        static Dictionary<string, Brush> _cachedBrushes = new Dictionary<string, Brush>
        {
            { "__External Dependencies__", Brushes.Black },
            { "", Brushes.Red }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string fa = value as string;
            if (string.IsNullOrEmpty(fa)) return _cachedBrushes[""];
            else if (!_cachedBrushes.ContainsKey(fa))
                _cachedBrushes.Add(fa, _availableBrushes[(_cachedBrushes.Count % (_availableBrushes.Count - 2)) + 2]);
            return _cachedBrushes[fa];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
