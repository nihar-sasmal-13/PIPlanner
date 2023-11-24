using PIPlanner.Helpers;
using System;
using System.Collections.ObjectModel;

namespace PIPlanner.DataModel
{
    internal enum SprintStatus
    {
        PlannedOrInProgress = 0,
        CompletedAsPlanned = 1,
        PartiallySucceeded = 2,
        CompleteFailure = 3
    }

    internal class SprintSummary : PropertyNotifier
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        //private int _sprintId;
        //public int SprintId
        //{
        //    get => _sprintId;
        //    set => SetProperty(ref _sprintId, value);
        //}

        //public Sprint Sprint { get; set; }

        private SprintStatus _status;
        public SprintStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private int _totalAvailableEffort = 1;
        public int TotalAvailableEffort
        {
            get => _totalAvailableEffort;
            set => SetProperty(ref _totalAvailableEffort, value <= 0 ? 1 : value, () => OnPropertyChanged("Properties"));
        }

        private int _defectEffort;
        public int DefectEffort
        {
            get => _defectEffort;
            set => SetProperty(ref _defectEffort, value, () => OnPropertyChanged("Properties"));
        }

        private int _carryForwardEffort;
        public int CarryForwardEffort
        {
            get => _carryForwardEffort;
            set => SetProperty(ref _carryForwardEffort, value, () => OnPropertyChanged("Properties"));
        }

        private int _newContentEffort;
        public int NewContentEffort
        {
            get => _newContentEffort;
            set =>SetProperty(ref _newContentEffort, value, () => OnPropertyChanged("Properties"));
        }

        private int _remainingEffort;
        public int RemainingEffort
        {
            get => _remainingEffort;
            set => SetProperty(ref _remainingEffort, value, () => OnPropertyChanged("Properties"));
        }

        public ObservableDictionary<string, Tuple<int, int>> Properties
        {
            get
            {
                var properties = new ObservableDictionary<string, Tuple<int, int>>();
                properties.Add("Total Available Effort", new Tuple<int, int>(TotalAvailableEffort, 100));
                properties.Add("Defect Effort", new Tuple<int, int>(DefectEffort, DefectEffort * 100 / TotalAvailableEffort));
                properties.Add("Carry Forward Effort", new Tuple<int, int>(CarryForwardEffort, CarryForwardEffort * 100 / TotalAvailableEffort));
                properties.Add("New Content Effort", new Tuple<int, int>(NewContentEffort, NewContentEffort * 100 / TotalAvailableEffort));
                properties.Add("Remaining Effort", new Tuple<int, int>(RemainingEffort, RemainingEffort * 100 / TotalAvailableEffort));
                return properties;
            }
        }
    }
}
