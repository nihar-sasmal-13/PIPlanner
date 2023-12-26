using PIPlanner.DataModel;
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
    class SprintStatusToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SprintStatus status = (SprintStatus)value;
            Brush brush = Brushes.Transparent;
            switch(status)
            {
                case SprintStatus.Planned:
                    {
                        brush = Brushes.LightCyan;
                        break;
                    }
                case SprintStatus.InProgress:
                    {
                        brush = Brushes.LightBlue;
                        break;
                    }
                case SprintStatus.CompletedAsPlanned:
                    {
                        brush = Brushes.LightGreen;
                        break;
                    }
                case SprintStatus.PartiallySucceeded:
                    {
                        brush = Brushes.YellowGreen;
                        break;
                    }
                case SprintStatus.CompleteFailure:
                    {
                        brush = Brushes.OrangeRed;
                        break;
                    }
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
