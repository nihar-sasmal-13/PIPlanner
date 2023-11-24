using PIPlanner.DataModel;
using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
                summarize(_selectedSprint);
                ChangeTracker.Notify();
                _selectedSprint.FireSprintUpdated();
                OnPropertyChanged("SelectedSprintTeams");
                OnPropertyChanged("SelectedSprintContent");
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
                    .ToObservableCollection();
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
                newContent.ForEach(c => content.Add(c));                    
                return content;
            }
        }

        public ScrumViewModel(PlanViewModel planViewModel)
        {
            _plan = planViewModel;
        }

        private void summarize(Sprint sprint)
        {
            if(_selectedSprint.SprintSummary == null)
            {
                _selectedSprint.SprintSummary = _plan.SprintSummaries.FirstOrDefault(ss => ss.Id == _selectedSprint.Id);
                if (_selectedSprint.SprintSummary == null)
                {
                    _selectedSprint.SprintSummary = new SprintSummary
                    {
                        Id = sprint.Id,
                        Status = SprintStatus.PlannedOrInProgress,
                    };
                    _plan.SprintSummaries.Add(_selectedSprint.SprintSummary);
                }
            }

            int totalAvailableEffort = _plan.SprintTeams.Sum(st => st.SprintAvailability);
            int defectEffort = _plan.SprintTeams.Sum(st => st.DefectBandwidth);
            int newContentEffort = SelectedSprintContent
                .Where(cr => cr.State == ContentState.Planned || cr.State == ContentState.Opportunity)
                .Sum(cr => cr.ChangeRequest.SPs);
            int remainingEffort = SelectedSprintContent
                .Where(cr => cr.State != ContentState.Completed)
                .Sum(cr => cr.RemainingSPs);

            _selectedSprint.SprintSummary.TotalAvailableEffort = totalAvailableEffort;
            _selectedSprint.SprintSummary.DefectEffort = defectEffort;
            _selectedSprint.SprintSummary.NewContentEffort = newContentEffort;
            _selectedSprint.SprintSummary.RemainingEffort = remainingEffort;
        }
    }
}
