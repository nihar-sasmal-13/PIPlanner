using PIPlanner.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIPlanner.DataModel
{
    internal class SprintTeam : PropertyNotifier
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private int _sprintId;
        [ForeignKey(nameof(SprintId))]
        public int SprintId
        {
            get => _sprintId;
            set => SetProperty(ref _sprintId, value);
        }

        private int _teamId;
        [ForeignKey(nameof(TeamId))]
        public int TeamId
        {
            get => _teamId;
            set => SetProperty(ref _teamId, value);
        }

        private int _piAvailability;
        public int PIAvailability
        {
            get => _piAvailability;
            set => SetProperty(ref _piAvailability, value);
        }

        private int _sprintAvailability;
        public int SprintAvailability
        {
            get => _sprintAvailability;
            set => SetProperty(ref _sprintAvailability, value, () => OnPropertyChanged("RemainingEffort"));
        }

        private int _assigned;
        public int Assigned
        {
            get => _assigned;
            set => SetProperty(ref _assigned, value, () => OnPropertyChanged("RemainingEffort"));
        }

        private int _defectBandwidth;
        public int DefectBandwidth
        {
            get => _defectBandwidth;
            set => SetProperty(ref _defectBandwidth, value, () => OnPropertyChanged("RemainingEffort"));
        }

        private int _miscBandwidth;
        public int MiscBandwidth
        {
            get => _miscBandwidth;
            set => SetProperty(ref _miscBandwidth, value, () => OnPropertyChanged("RemainingEffort"));
        }

        [NotMapped]        
        public int RemainingEffort
        {
            get => SprintAvailability - Assigned - DefectBandwidth - MiscBandwidth;
        }

        public Sprint Sprint { get; set; }
        public Team Team { get; set; }

        public void NotifySprintTeamChanged()
        {
            OnPropertyChanged("SprintAvailability");
        }
    }
}
