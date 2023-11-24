using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PIPlanner.DataModel
{
    internal class Team : PropertyNotifier
    {
        [NotMapped]
        public static List<Brush> TeamColors
        {
            get => new List<Brush>
            {
                Brushes.LightBlue,
                Brushes.LightCoral,
                Brushes.LightGreen,
                Brushes.LightSlateGray,
                Brushes.LightYellow,
                Brushes.LightCyan,
                Brushes.LightPink
            };
        }

        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _teamColor;
        public string TeamColor
        {
            get => _teamColor;
            set => SetProperty(ref _teamColor, value);
        }

        private int _velocity;
        public int Velocity
        {
            get => _velocity;
            set => SetProperty(ref _velocity, value);
        }
    }
}
