using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using PIPlanner.DataModel;
using PIPlanner.Helpers;
using PIPlanner.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PIPlanner.ViewModels
{
    internal class PlanViewModel : PropertyNotifier
    {
        private PIPlannerDbContext _context;

        public Plan PlanMetadata { get; private set; }
        public ObservableCollection<ChangeRequest> ChangeRequests { get; private set; }
        public ObservableCollection<Team> Teams { get; private set; }
        public ObservableCollection<Sprint> Sprints { get; private set; }
        public ObservableCollection<SprintTeam> SprintTeams { get; private set; }
        public ObservableCollection<TeamMember> TeamMembers { get; private set; }
        public ObservableCollection<SprintTeamMember> SprintTeamMembers { get; private set; }
        public ObservableCollection<SprintSummary> SprintSummaries { get; private set; }
        public ObservableCollection<SprintContent> SprintContents { get; private set; }

        private ObservableCollection<ChangeRequest> _features;
        public ObservableCollection<ChangeRequest> Features
        {
            get
            {
                if (_features == null && ChangeRequests != null && ChangeRequests.Any())
                {
                    _features = ChangeRequests
                        .Where(cr => cr.IsFeature)
                        .ToObservableCollection();
                }
                return _features;
            }
        }

        public int TotalDCRSPs
        {
            get => ChangeRequests.Sum(cr => cr.Status == DCRStatus.Approved ? cr.SPs : 0);
        }

        public int TotalBandwidth
        {
            get => Math.Max(SprintTeams.Sum(st => st.PIAvailability), 
                SprintTeams.Sum(st => st.SprintAvailability));
        }

        public int TotalAllocated
        {
            get => ChangeRequests.Where(cr => cr.SprintId > 0).Sum(cr => cr.SPs);
        }

        public int TotalFeatureAllocated
        {
            get => ChangeRequests.Where(cr => cr.SprintId == null && cr.TeamId != null).Sum(cr => cr.SPs);
        }        

        public bool HasUnsavedChanges
        {
            get
            {
                _context.ChangeTracker.DetectChanges();
                return _context.ChangeTracker.HasChanges();
            }
        }

        public PlanViewModel(string planFilePath)
        {
            var options = new DbContextOptionsBuilder<PIPlannerDbContext>()
                        .UseSqlite($"Data Source={planFilePath}")
                        .Options;
            _context = new PIPlannerDbContext(options);
            _context.Plans.Load();
            _context.ChangeRequests.Load();
            _context.Teams.Load();
            _context.Sprints.Load();
            _context.SprintTeams.Load();
            _context.TeamMembers.Load();
            _context.SprintTeamMembers.Load();
            _context.SprintSummaries.Load();
            _context.SprintContents.Load();

            PlanMetadata = _context.Plans.Local.FirstOrDefault();            
            ChangeRequests = _context.ChangeRequests.Local.ToObservableCollection();
            Sprints = _context.Sprints.Local.ToObservableCollection();
            Teams = _context.Teams.Local.ToObservableCollection();
            SprintTeams = _context.SprintTeams.Local.ToObservableCollection();       
            TeamMembers = _context.TeamMembers.Local.ToObservableCollection();
            SprintTeamMembers = _context.SprintTeamMembers.Local.ToObservableCollection();
            SprintSummaries = _context.SprintSummaries.Local.ToObservableCollection();
            SprintContents = _context.SprintContents.Local.ToObservableCollection();

            if (PlanMetadata == null)
            {
                PlanMetadata = new Plan
                {
                    Id = 0,
                    Name = "New Plan",
                    CreatedAt = DateTime.Now,
                    LastModifiedAt = DateTime.Now,
                };
                _context.Plans.Local.Add(PlanMetadata);
            }
        }

        public void MergeChangeRequests(IEnumerable<ChangeRequest> changeRequests)
        {
            foreach (ChangeRequest changeRequest in changeRequests)
            {
                var foundCR = ChangeRequests.FirstOrDefault(cr => cr.Id == changeRequest.Id);
                if (foundCR != null)
                {
                    foundCR.Summary = changeRequest.Summary;
                    foundCR.SPs = changeRequest.SPs;
                    foundCR.FunctionalArea = changeRequest.FunctionalArea;
                }
                else
                    ChangeRequests.Add(changeRequest);
            }
        }

        public void Save()
        {
            try
            {
                _context.SaveChanges();
                OnPropertyChanged("HasUnsavedChanges");
            }
            catch (Exception ex)
            {
                var entries = _context.ChangeTracker.Entries();
                MessageBox.Show($"ERROR: Could not save the changes to database. Check the correctness of the changes." +
                    $"\n\n{ex.Message}\n{ex.InnerException?.Message}",
                    "Error in saving data", MessageBoxButton.OK);
            }
        }

        public void Close()
        {
            _context.Dispose();
            _context = null;
        }

        public void NotifyPlanChanged()
        {
            foreach (var st in SprintTeams)
                st.NotifySprintTeamChanged();

            OnPropertyChanged("TotalDCRSPs");
            OnPropertyChanged("TotalBandwidth");
            OnPropertyChanged("TotalAllocated");
            OnPropertyChanged("TotalFeatureAllocated");      
            
            OnPropertyChanged("HasUnsavedChanges");
        }

        public void UpdatePlanUnsavedStatus()
        {
            OnPropertyChanged("HasUnsavedChanges");
        }
    }
}
