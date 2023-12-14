using PIPlanner.DataModel;
using PIPlanner.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PIPlanner.ViewModels
{
    internal class ScrumViewModel : PropertyNotifier
    {
        private PlanViewModel _plan;

        private Sprint _selectedSprint;
        public Sprint SelectedSprint
        {
            get => _selectedSprint;
            set => SetProperty(ref _selectedSprint, value, () =>
            {
                if (value != null)
                {
                    //summarizeSelectedSprint(_selectedSprint);
                    summarize(_selectedSprint);
                    ChangeTracker.Notify();
                    _selectedSprint.FireSprintUpdated();
                    OnPropertyChanged("SelectedSprintTeams");
                    OnPropertyChanged("SelectedSprintContent");
                }
            });
        }

        public ObservableCollection<SprintTeam> SelectedSprintTeams
        {
            get
            {
                if (_plan == null || _selectedSprint == null) return new ObservableCollection<SprintTeam>();
                var sprintTeams = _plan.SprintTeams.Where(st => st.SprintId == SelectedSprint.Id).ToList();
                sprintTeams.ForEach(st => st.PropertyChanged += (sender, e) => summarize(_selectedSprint));
                return sprintTeams.ToObservableCollection();
            }
        }

        public ObservableCollection<SprintContent> SelectedSprintContent
        {
            get
            {
                if (SelectedSprint == null)
                    return new ObservableCollection<SprintContent>();

                //get already added content
                var content = _plan.SprintContents
                    .Where(sc => sc.SprintId == SelectedSprint.Id)
                    .ToList();
                var newContent = _plan.ChangeRequests
                    .Where(cr => (cr.SprintId == SelectedSprint.Id) && !content.Any(c => c.DCRId == cr.Id))
                    .ToList()
                    .Select(cr => new SprintContent
                    {
                        DCRId = cr.Id,
                        SprintId = SelectedSprint.Id,
                        RemainingSPs = cr.SPs,
                        State = ContentState.Planned,
                        ChangeRequest = cr
                    })
                    .ToList();
                newContent.ForEach(c => _plan.SprintContents.Add(c));
                content = _plan.SprintContents
                    .Where(sc => sc.SprintId == SelectedSprint.Id)
                    .ToList();
                content.ForEach(c => c.PropertyChanged += (s, e) => Update());
                return content.ToObservableCollection();
            }
        }

        public ScrumViewModel(PlanViewModel planViewModel)
        {
            _plan = planViewModel;
            updateSprintContents();
        }

        public void Update()
        {
            updateSprintContents();

            //reset selected sprint to auto-update all fields
            var selectedSprint = SelectedSprint;
            SelectedSprint = null;
            SelectedSprint = selectedSprint;
        }

        private void updateSprintContents()
        {
            foreach (var sprint in _plan.Sprints)
            {
                var newContent = _plan.ChangeRequests
                    .Where(cr => (cr.SprintId == sprint.Id) && !_plan.SprintContents.Any(c => c.DCRId == cr.Id))
                    .Select(cr => new SprintContent
                    {
                        DCRId = cr.Id,
                        SprintId = sprint.Id,
                        RemainingSPs = cr.SPs,
                        State = ContentState.Planned,
                        ChangeRequest = cr,
                        PlanningComments = string.Empty
                    })
                    .ToList();
                newContent.ForEach(c => _plan.SprintContents.Add(c));
                summarize(sprint);
            }
            ChangeTracker.Notify();
        }

        private void summarize(Sprint sprint)
        {
            if(sprint == null)
                return;

            if (sprint.SprintSummary == null)
            {
                sprint.SprintSummary = _plan.SprintSummaries.FirstOrDefault(ss => ss.Id == sprint.Id);
                if (sprint.SprintSummary == null)
                {
                    sprint.SprintSummary = new SprintSummary
                    {
                        Id = sprint.Id,
                        Status = getSprintStatus(sprint),
                    };
                    _plan.SprintSummaries.Add(sprint.SprintSummary);
                }
            }
            else
            {
                sprint.SprintSummary.Status = getSprintStatus(sprint);
            }

            int totalAvailableEffort = _plan.SprintTeams
                .Where(st => st.SprintId == sprint.Id)
                .Sum(st => st.SprintAvailability);
            int defectEffort = _plan.SprintTeams
                .Where(st => st.SprintId == sprint.Id)
                .Sum(st => st.DefectBandwidth);
            int newContentEffort = _plan.SprintContents
                .Where(sc => sc.SprintId == sprint.Id)
                .Where(cr => cr.State == ContentState.Planned || cr.State == ContentState.Opportunity)
                .Sum(cr => cr.ChangeRequest.SPs);
            int otherEffort = _plan.SprintTeams
                .Where(st => st.SprintId == sprint.Id)
                .Sum(st => st.MiscBandwidth);

            sprint.SprintSummary.TotalAvailableEffort = totalAvailableEffort;
            sprint.SprintSummary.DefectEffort = defectEffort;
            sprint.SprintSummary.NewContentEffort = newContentEffort;
            sprint.SprintSummary.OtherEffort = otherEffort;

            _plan.SprintTeams
                .Where(st => st.SprintId == sprint.Id)
                .ToList()
                .ForEach(st => st.Assigned = (_plan.SprintContents
                    .Where(sc => sc.SprintId == sprint.Id && sc.ChangeRequest.TeamId == st.TeamId)
                    .Sum(sc => sc.ChangeRequest.SPs))
                    );
        }

        private SprintStatus getSprintStatus(Sprint sprint)
        {
            if (sprint == null)
                return SprintStatus.PlannedOrInProgress;
            if (sprint.EndDate > DateTime.Today)
                return SprintStatus.PlannedOrInProgress;
            else
            {
                var sprintContent = _plan.SprintContents.Where(sc => sc.SprintId == sprint.Id);
                if (sprintContent == null || !sprintContent.Any())
                    return SprintStatus.CompletedAsPlanned;
                if (sprintContent.All(sc => sc.State == ContentState.Completed || sc.State == ContentState.Dropped))
                    return SprintStatus.CompletedAsPlanned;
                else if (sprintContent.All(sc => sc.State == ContentState.CarryForwarded || sc.State == ContentState.Planned))
                    return SprintStatus.CompleteFailure;
                else if (sprintContent.Any(sc => sc.State == ContentState.Completed))
                    return SprintStatus.PartiallySucceeded;
            }
            return SprintStatus.PlannedOrInProgress;
        }
    }
}
