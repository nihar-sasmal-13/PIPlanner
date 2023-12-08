using PIPlanner.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIPlanner.DataModel
{
    internal enum ContentState
    {
        Planned = 0,
        CarryForwarded = 1,
        Opportunity = 2,
        Completed = 3,
        Dropped = 4
    }

    internal class SprintContent : PropertyNotifier
    {
        private int _id;
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private ContentState _state;
        public ContentState State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        private string _dcrId;
        [ForeignKey(nameof(DCRId))]
        public string DCRId
        {
            get => _dcrId;
            set => SetProperty(ref _dcrId, value);
        }

        public ChangeRequest ChangeRequest { get; set; }

        private int _sprintId;
        [ForeignKey(nameof(SprintId))]
        public int SprintId
        {
            get => _sprintId;
            set => SetProperty(ref _sprintId, value);
        }

        public Sprint Sprint { get; set; }

        private int _remainingSPs;
        public int RemainingSPs
        {
            get => _remainingSPs;
            set => SetProperty(ref _remainingSPs, value);
        }

        private string? _planningComments;
        public string? PlanningComments
        {
            get => _planningComments;
            set => SetProperty(ref _planningComments, value);
        }
    }
}
