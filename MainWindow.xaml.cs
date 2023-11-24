using PIPlanner.DataModel;
using PIPlanner.Helpers;
using PIPlanner.ViewModels;
using PIPlanner.Views;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PIPlanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel;

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            DataContext = ViewModel;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Plan")
                Application.Current.Dispatcher.Invoke(() =>
                {
                    prepareFeatureGUI();
                    prepareGUI();
                });
            else if (e.PropertyName == "GridWidth" || e.PropertyName == "GridHeight")
                Application.Current.Dispatcher.Invoke(() => gridDimensionsHaveChanged());
        }

        private void gridDimensionsHaveChanged()
        {
            for (int i = 1; i < _piplanGrid.RowDefinitions.Count; i++)
                _piplanGrid.RowDefinitions[i].Height = new GridLength(ViewModel.GridHeight);
            for (int i = 1; i < _piplanGrid.ColumnDefinitions.Count; i++)
                _piplanGrid.ColumnDefinitions[i].Width = new GridLength(ViewModel.GridWidth);

            for (int i = 1; i < _featurePlanGrid.RowDefinitions.Count; i++)
                _featurePlanGrid.RowDefinitions[i].Height = new GridLength(ViewModel.GridHeight);
        }

        private void prepareFeatureGUI()
        {
            _featurePlanGrid.Children.Clear();
            _featurePlanGrid.RowDefinitions.Clear();
            _featurePlanGrid.ColumnDefinitions.Clear();

            _featurePlanGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _featurePlanGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _featurePlanGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            //define the grid we right numbers of rows and columns. Rows => teams. Columns => 1.
            var gridLength = new GridLength(1, GridUnitType.Star);
            //for (int i = 0; i < ViewModel.Plan.Sprints.Count; i++)
            //    _featurePlanGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength, MinWidth = 20 });
            for (int i = 0; i < ViewModel.Plan.Teams.Count; i++)
                _featurePlanGrid.RowDefinitions.Add(new RowDefinition { Height = gridLength, MinHeight = 20 });

            TextBlock release = new TextBlock
            {
                Text = ViewModel.Plan.PlanMetadata.Name,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            release.SetValue(Grid.RowProperty, 0);
            release.SetValue(Grid.ColumnProperty, 1);
            _featurePlanGrid.Children.Add(release);

            //Create team buttons
            for (int i = 0; i < ViewModel.Plan.Teams.Count; i++)
            {
                var team = ViewModel.Plan.Teams[i];
                Button element = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = team.TeamColor.ToColor(),
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(2),
                    Command = new Command((param) => showTeamDetails(team), true)
                };
                Binding binding = new Binding($"Plan.Teams[{i}].Name");
                var textBlock = new TextBlock { LayoutTransform = new RotateTransform { Angle = 270 } };
                textBlock.SetBinding(TextBlock.TextProperty, binding);
                element.Content = textBlock;

                element.SetValue(Grid.ColumnProperty, 0);
                element.SetValue(Grid.RowProperty, i + 1);
                _featurePlanGrid.Children.Add(element);

                var teamAvailability = team.Velocity * ViewModel.Plan.Sprints.Count;
                TeamSprintView panel = new TeamSprintView
                {
                    SprintId = null,    //null is required for feature grid
                    TeamId = team.Id,
                    FeatureView = true,
                    Background = team.TeamColor.ToColor(),
                    Available = teamAvailability,
                    RefreshAction = refreshPanels,
                    DCRFinderFunc = new Func<string, ChangeRequest>((crid) => ViewModel.Plan.ChangeRequests.FirstOrDefault(cr => cr.Id == crid))
                };

                panel.SetValue(Grid.ColumnProperty, 1);
                panel.SetValue(Grid.RowProperty, i + 1);
                _featurePlanGrid.Children.Add(panel);
            }
        }

        private void prepareGUI()
        {
            _piplanGrid.Children.Clear();
            _piplanGrid.RowDefinitions.Clear();
            _piplanGrid.ColumnDefinitions.Clear();

            _piplanGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _piplanGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            //define the grid we right numbers of rows and columns. Rows => teams. Columns => Sprints.
            var gridLength = new GridLength(1, GridUnitType.Star);
            for (int i = 0; i < ViewModel.Plan.Sprints.Count; i++)
                _piplanGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength, MinWidth = 20 });
            for (int i = 0; i < ViewModel.Plan.Teams.Count; i++)
                _piplanGrid.RowDefinitions.Add(new RowDefinition { Height = gridLength, MinHeight = 20 });

            //Create sprint buttons
            for (int i = 0; i < ViewModel.Plan.Sprints.Count; i++)
            {
                var sprint = ViewModel.Plan.Sprints[i];
                Button element = new Button
                {
                    Content = $"{sprint.Name}{Environment.NewLine}" +
                        $"{sprint.StartDate.ToString("MMM-dd-yy")} : " +
                        $"{sprint.EndDate.ToString("MMM-dd-yy")}",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = Brushes.Transparent,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(2),
                    Command = new Command((param) => showSprintPlan(sprint), true)
                };
                element.SetValue(Grid.ColumnProperty, i + 1);
                element.SetValue(Grid.RowProperty, 0);
                _piplanGrid.Children.Add(element);
            }

            //Create team buttons
            for (int i = 0; i < ViewModel.Plan.Teams.Count; i++)
            {
                var team = ViewModel.Plan.Teams[i];
                Button element = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = team.TeamColor.ToColor(),
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(2),
                    Command = new Command((param) => showTeamDetails(team), true)
                };
                Binding binding = new Binding($"Plan.Teams[{i}].Name");
                var textBlock = new TextBlock { LayoutTransform = new RotateTransform { Angle = 270 } };
                textBlock.SetBinding(TextBlock.TextProperty, binding);
                element.Content = textBlock;

                element.SetValue(Grid.ColumnProperty, 0);
                element.SetValue(Grid.RowProperty, i + 1);
                _piplanGrid.Children.Add(element);
            }

            if (ViewModel.Plan.Sprints != null && ViewModel.Plan.Sprints.Count > 0)
            {
                int sprintIdOffset = ViewModel.Plan.Sprints[0].Id - 1;
                //create sprint+team panels to hold CRs            
                if (ViewModel.Plan.SprintTeams != null)
                {
                    foreach (var sprintTeam in ViewModel.Plan.SprintTeams)
                    {
                        TeamSprintView panel = new TeamSprintView
                        {
                            SprintId = sprintTeam.SprintId,
                            TeamId = sprintTeam.TeamId,
                            FeatureView = false,
                            Background = sprintTeam.Team.TeamColor.ToColor(),
                            Available = sprintTeam.PIAvailability,
                            RefreshAction = refreshPanels,
                            DCRFinderFunc = new Func<string, ChangeRequest>((dcrId) => ViewModel.Plan.ChangeRequests.FirstOrDefault(dcr => dcr.Id == dcrId))
                        };
                        panel.SetValue(Grid.ColumnProperty, sprintTeam.SprintId - sprintIdOffset);
                        panel.SetValue(Grid.RowProperty, sprintTeam.TeamId);
                        _piplanGrid.Children.Add(panel);
                    }
                }
            }

            fit2WindowButton_Click(this, null);
            refreshPanels();
        }

        private void refreshPanels()
        {
            if (ViewModel?.Plan?.ChangeRequests == null)
                return;

            foreach (var cr in ViewModel.Plan.ChangeRequests)
            {
                if (cr.SprintId == 0 && cr.TeamId == 0)
                    cr.BackgroundColor = Brushes.Transparent;

                //if (cr.TeamId != 0)
                //    cr.TeamName = ViewModel.Plan.Teams.FirstOrDefault(t => t.Id == cr.TeamId)?.TeamName;
            }

            foreach (var panel in _piplanGrid.Children)
            {
                TeamSprintView sprintView = panel as TeamSprintView;
                if (sprintView != null)
                {
                    var sprintTeam = ViewModel.Plan.SprintTeams
                        .First(st => (st.TeamId == sprintView.TeamId) && (st.SprintId == sprintView.SprintId));
                    sprintView.Available = sprintTeam.SprintAvailability;
                    sprintView.Background = sprintTeam.Team.TeamColor.ToColor();

                    var crs = ViewModel.Plan.ChangeRequests.Where(cr => cr.SprintId == sprintView.SprintId && cr.TeamId == sprintView.TeamId);
                    sprintView.EnsureItemValidity(crs, _showItemDetails.IsChecked.GetValueOrDefault());
                }
            }

            foreach (var panel in _featurePlanGrid.Children)
            {
                TeamSprintView sprintView = panel as TeamSprintView;
                if (sprintView != null)
                {
                    sprintView.Available = ViewModel.Plan.SprintTeams
                        .Where(st => st.TeamId == sprintView.TeamId)
                        .Sum(st => st.SprintAvailability);

                    var crs = ViewModel.Plan.ChangeRequests.Where(cr => cr.SprintId == null && cr.TeamId == sprintView.TeamId);
                    sprintView.EnsureItemValidity(crs, _showItemDetails.IsChecked.GetValueOrDefault());
                }
            }
            ViewModel.Plan.UpdatePlanUnsavedStatus();
        }

        private void fit2WindowButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GridWidth = (this.ActualWidth * 0.75) / (_piplanGrid.ColumnDefinitions.Count - 1);
            ViewModel.GridHeight = (this.ActualHeight * 0.75) / (_piplanGrid.RowDefinitions.Count - 1);
        }

        private void _showItemDetails_Checked(object sender, RoutedEventArgs e)
        {
            _showItemDetails.Content = _showItemDetails.IsChecked.GetValueOrDefault() ?
                "Hide Details" :
                "Show Details";
            refreshPanels();
        }

        private void pushButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void pullButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            new AboutView().Show();
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.FilterChangedCommand.Execute((sender as TextBox).Text);
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            _featureFilter.SelectedIndex = -1;
        }

        private void _dependenciesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void showTeam_Click(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;            
            showTeamDetails(btn?.Tag as Team);
        }

        private void showSprintPlan(Sprint sprint)
        {
            _tabControl.SelectedIndex = 3;
            ViewModel.Scrum.SelectedSprint = sprint;
        }

        private void showTeamDetails(Team team)
        {
            if (team == null) return;

            TeamView dlg = new TeamView(team, ViewModel.Plan);
            dlg.ShowDialog();

            //assume team availability has changed.. update sprint panels
            ViewModel.RefreshPlanCommand.Execute(null);
            refreshPanels();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (ViewModel != null && ViewModel.Plan != null)
            {
                if (ViewModel.Plan.HasUnsavedChanges)
                {
                    var result = MessageBox.Show("There are unsaved changes to the plan.\n" +
                        "Click Yes to save and close.\n" +
                        "Click No to close without saving.\n" +
                        "Click Cancel to prevent closing of the application.",
                        "Close Application",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Cancel)
                        e.Cancel = true;
                    else
                    {
                        if (result == MessageBoxResult.Yes)
                            ViewModel.SaveCommand.Execute(null);
                        e.Cancel = false;
                    }
                }

                if (!e.Cancel) ViewModel.Plan.Close();
            }
            else
                e.Cancel = false;
            
        }
    }
}
