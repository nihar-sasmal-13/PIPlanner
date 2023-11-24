using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPlanner.DataModel
{
    internal class Sprint : PropertyNotifier
    {
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

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private int? _sprintSummaryId;
        [ForeignKey(nameof(SprintSummaryId))]
        public int? SprintSummaryId
        {
            get => _sprintSummaryId;
            set => SetProperty(ref _sprintSummaryId, value);
        }

        public SprintSummary? SprintSummary { get; set; }

        public void FireSprintUpdated()
        {
            OnPropertyChanged(nameof(SprintSummary));
            OnPropertyChanged(nameof(SprintSummaryId));
        }
    }
}
