using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPlanner.DataModel
{
    internal class TeamMember : PropertyNotifier
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private int _teamId;
        [ForeignKey(nameof(TeamId))]
        public int TeamId
        {
            get => _teamId;
            set => SetProperty(ref _teamId, value);
        }

        public Team Team { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private float _dailyHours;
        public float DailyHours
        {
            get => _dailyHours;
            set => SetProperty(ref _dailyHours, value);
        }

        private float _defaultDrag;
        public float DefaultDrag
        {
            get => _defaultDrag; 
            set => SetProperty(ref _defaultDrag, value);
        }
    }
}
