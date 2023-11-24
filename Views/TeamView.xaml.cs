using PIPlanner.DataModel;
using PIPlanner.Helpers;
using PIPlanner.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PIPlanner.Views
{
    /// <summary>
    /// Interaction logic for TeamView.xaml
    /// </summary>
    internal partial class TeamView : Window, INotifyPropertyChanged
    {
        private Team _team;
        public Team Team
        {
            get => _team;
            set => SetProperty(ref _team, value);
        }

        private PlanViewModel _plan;
        public PlanViewModel Plan
        {
            get => _plan;
            set => SetProperty(ref _plan, value);
        }

        private ObservableCollection<SprintTeam> _sprintTeams;
        public ObservableCollection<SprintTeam> SprintTeams
        {
            get => _sprintTeams;
            set => SetProperty(ref _sprintTeams, value);
        }

        private ObservableCollection<TeamMember> _teamMembers;
        public ObservableCollection<TeamMember> TeamMembers
        {
            get => _teamMembers;
            set => SetProperty(ref _teamMembers, value);
        }

        private ObservableCollection<Tuple<string, ObservableCollection<SprintTeamMember>>> _allSprintTeamMembers;
        public ObservableCollection<Tuple<string, ObservableCollection<SprintTeamMember>>> AllSprintTeamMembers
        {
            get => _allSprintTeamMembers;
            set => SetProperty(ref _allSprintTeamMembers, value);
        }

        public TeamView()
        {
            InitializeComponent();
        }

        internal TeamView(Team team, PlanViewModel plan)
        {
            InitializeComponent();
            Team = team;
            Plan = plan;
            SprintTeams = Plan.SprintTeams.Where(st => st.TeamId == team.Id).ToObservableCollection();
            TeamMembers = Plan.TeamMembers.Where(tm => tm.TeamId == _team.Id).ToObservableCollection();

            team.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Velocity")
                {
                    foreach (var sprintTeam in SprintTeams)
                        sprintTeam.PIAvailability = team.Velocity;
                }
            };

            DataContext = this;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Plan.Save();            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newValue = (sender as TextBox).Text;
            int teamSize = 0;
            if (DataContext == null || string.IsNullOrEmpty(newValue) || !int.TryParse(newValue, out teamSize))
                return;

            if (teamSize <= 0) 
                return;

            //remove all the team members for THIS team
            _plan.TeamMembers
                .Where(tm => tm.TeamId == _team.Id)
                .ToList()
                .ForEach(tm => _plan.TeamMembers.Remove(tm));

            //add new team members for this team
            var newTeamMembers = Enumerable.Range(0, teamSize)
                .Select(i => new TeamMember
                {
                    Id = _team.Id * 100 + i,
                    TeamId = _team.Id,
                    Name = $"Person# {i + 1}",
                    DailyHours = 8,
                    DefaultDrag = 0.2f,
                })
                .ToList();
            newTeamMembers.ForEach(tm => _plan.TeamMembers.Add(tm));
            TeamMembers = newTeamMembers.ToObservableCollection();

            AllSprintTeamMembers = new ObservableCollection<Tuple<string, ObservableCollection<SprintTeamMember>>>();
            foreach (var sprintTeam in _plan.SprintTeams.Where(st => st.TeamId == _team.Id))
            {
                //remove team members from every sprintTeam corresponding to this team
                _plan.SprintTeamMembers
                    .Where(stm => stm.SprintTeamId == sprintTeam.Id)
                    .ToList()
                    .ForEach(stm => _plan.SprintTeamMembers.Remove(stm));

                var sprintMembers = newTeamMembers.Select(tm => new SprintTeamMember
                {
                    Id = sprintTeam.Id * 100 + tm.Id,
                    SprintTeamId = sprintTeam.Id,
                    TeamMemberId = tm.Id,
                    Drag = 0.2f,
                    WorkingDays = 10
                })
                .ToList();
                sprintMembers.ForEach(stm => _plan.SprintTeamMembers.Add(stm));
                AllSprintTeamMembers.Add(new Tuple<string, ObservableCollection<SprintTeamMember>>(sprintTeam.Sprint.Name,
                    sprintMembers.ToObservableCollection()));
                var totalAvailabilityHours = sprintMembers.Sum(stm => stm.WorkingDays * (1 - stm.Drag));
                sprintTeam.SprintAvailability = Convert.ToInt32(totalAvailabilityHours);
            }
        }

        protected bool SetProperty<T>(ref T backingStore, T value, Action? onChanged = null, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null) return;
            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
