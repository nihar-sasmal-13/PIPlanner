using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore.Query.Internal;
using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PIPlanner.ViewModels
{
    class PIStatisticsViewModel : PropertyNotifier
    {
        private PlanViewModel _plan;
        public PlanViewModel Plan
        {
            get => _plan;
            set => SetProperty(ref _plan, value, () =>
            {
                _plan.PropertyChanged += (s, e) => App.Current.Dispatcher.Invoke(() => RefreshStats());
                App.Current.Dispatcher.Invoke(() => RefreshStats());
            });
        }

        private SeriesCollection _burndown;
        public SeriesCollection Burndown
        {
            get => _burndown;
            private set => SetProperty(ref _burndown, value);
        }

        private AxesCollection _burndownXAxis;
        public AxesCollection BurndownXAxis
        {
            get => _burndownXAxis;
            private set => SetProperty(ref _burndownXAxis, value);
        }

        private AxesCollection _burndownYAxis;
        public AxesCollection BurndownYAxis
        {
            get => _burndownYAxis;
            private set => SetProperty(ref _burndownYAxis, value);
        }

        private SeriesCollection _sprintDistribution;
        public SeriesCollection SprintDistribution
        {
            get => _sprintDistribution;
            private set => SetProperty(ref _sprintDistribution, value);
        }

        private AxesCollection _sprintXAxis;
        public AxesCollection SprintXAxis
        {
            get => _sprintXAxis;
            private set => SetProperty(ref _sprintXAxis, value);
        }

        private AxesCollection _sprintYAxis;
        public AxesCollection SprintYAxis
        {
            get => _sprintYAxis;
            private set => SetProperty(ref _sprintYAxis, value);
        }

        private SeriesCollection _dcrsByState;
        public SeriesCollection DCRStates
        {
            get => _dcrsByState;
            private set => SetProperty(ref _dcrsByState, value);
        }

        private SeriesCollection _teamCarryForwards;
        public SeriesCollection TeamCarryForwards
        {
            get => _teamCarryForwards;
            private set => SetProperty(ref _teamCarryForwards, value);
        }

        private AxesCollection _teamNames;
        public AxesCollection TeamNames
        {
            get => _teamNames;
            private set => SetProperty(ref _teamNames, value);
        }

        private AxesCollection _teamYAxis;
        public AxesCollection TeamYAxis
        {
            get => _teamYAxis;
            private set => SetProperty(ref _teamYAxis, value);
        }

        private Dictionary<DataModel.ContentState, Brush> _stateColors;

        public PIStatisticsViewModel()
        {
            _stateColors = new Dictionary<DataModel.ContentState, Brush> 
            {
                { DataModel.ContentState.Opportunity, Brushes.MediumPurple },
                { DataModel.ContentState.Dropped, Brushes.DarkGray },
                { DataModel.ContentState.CarryForwarded, Brushes.Pink },
                { DataModel.ContentState.Completed, Brushes.LightGreen },
                { DataModel.ContentState.Planned, Brushes.LightBlue },
            };
        }

        public void RefreshStats()
        {
            createBurndown();
            createSprintDistribution();
            createDCRStateDistribution();
            createCarryForwards();            
        }

        private void createSprintDistribution()
        {
            SprintDistribution = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Total Available Effort",
                    Values = new ChartValues<int>(Plan.Sprints.Select(sp => sp.SprintSummary.TotalAvailableEffort)),
                    DataLabels = true,                    
                },
                new StackedAreaSeries
                {
                    Title = "New Content Effort",
                    Values = new ChartValues<int>(Plan.Sprints.Select(sp => sp.SprintSummary.NewContentEffort)),
                    DataLabels = true,
                    Fill = Brushes.LightGreen
                },
                new StackedAreaSeries
                {
                    Title = "Defect Effort",
                    Values = new ChartValues<int>(Plan.Sprints.Select(sp => sp.SprintSummary.DefectEffort)),
                    DataLabels = true,
                    Fill = Brushes.OrangeRed
                },
                new StackedAreaSeries
                {
                    Title = "Other Effort",
                    Values = new ChartValues<int>(Plan.Sprints.Select(sp => sp.SprintSummary.OtherEffort)),
                    DataLabels = true,
                    Fill = Brushes.LightGray
                },
                new StackedAreaSeries
                {
                    Title = "CarryForward Effort",
                    Values = new ChartValues<int>(Plan.Sprints.Select(sp => sp.SprintSummary.CarryForwardEffort)),
                    DataLabels = true,
                    Fill = Brushes.Pink
                },
            };

            SprintXAxis = new AxesCollection
            {
                new Axis
                {
                    Labels = Plan.Sprints.Select(sp => sp.Name).ToList(),
                    Separator = new Separator { Step = 1 }
                }
            };

            SprintYAxis = new AxesCollection
            {
                new Axis
                {
                    LabelFormatter = value => value.ToString(),
                    MinValue = 0,
                    ShowLabels = true
                }
            };
        }

        private void createBurndown()
        {
            List<int> sprintCumulativeSPs = new List<int>();
            List<int> sprintCompletedSPs = new List<int>();
            foreach (var sprint in Plan.Sprints)
            {
                sprintCumulativeSPs.Add(Plan.ChangeRequests
                    .Where(cr => cr.SprintId >= sprint.Id)
                    .Sum(cr => cr.SPs));
                sprintCompletedSPs.Add(Plan.ChangeRequests
                    .Where(cr => cr.SprintId >= sprint.Id)
                    .Where(cr => Plan.SprintContents.FirstOrDefault(sc => sc.DCRId == cr.Id)?.State == DataModel.ContentState.Completed)
                    .Sum(cr => cr.SPs));
            }
            Burndown = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Ideal Burndown",
                    Values = new ChartValues<int>(sprintCumulativeSPs),
                    Foreground = Brushes.LightBlue
                },
                new LineSeries
                {
                    Title = "Actual Burndown",
                    DataLabels = true,
                    Values = new ChartValues<int>(sprintCompletedSPs),
                    Foreground = Brushes.Pink
                }
            };

            BurndownXAxis = new AxesCollection
            {
                new Axis
                {
                    Labels = Plan.Sprints.Select(sp => sp.Name).ToList(),
                    Separator = new Separator { Step = 1 }
                }
            };

            BurndownYAxis = new AxesCollection
            {
                new Axis
                {
                    LabelFormatter = value => value.ToString(),
                    MinValue = 0,
                    ShowLabels = true
                }
            };
        }

        private void createDCRStateDistribution()
        {
            var pies = Plan.SprintContents
                .GroupBy(sc => sc.State)
                .Select(grp => new PieSeries
                {
                    Title = grp.Key.ToString(),
                    DataLabels = true,
                    Values = new ChartValues<int> { grp.Count() },
                    PushOut = grp.Key == DataModel.ContentState.Completed ? 15 : 0,
                    Fill = _stateColors[grp.Key]
                });
            DCRStates = new SeriesCollection();
            DCRStates.AddRange(pies);
        }

        private void createCarryForwards()
        {
            List<int> completedCRs = new List<int>();
            List<int> inProgressCRs = new List<int>();
            List<int> carryForwardedCRs = new List<int>();
            foreach (var team in Plan.Teams)
            {                
                completedCRs.Add(0 - Plan.SprintContents
                    .Where(sc => sc.ChangeRequest.TeamId == team.Id && sc.State == DataModel.ContentState.Completed)
                    .Count());
                carryForwardedCRs.Add(Plan.SprintContents
                    .Where(sc => sc.ChangeRequest.TeamId == team.Id && sc.State == DataModel.ContentState.CarryForwarded)
                    .Count());
                inProgressCRs.Add(Plan.SprintContents
                    .Where(sc => sc.ChangeRequest.TeamId == team.Id && sc.State == DataModel.ContentState.Planned)
                    .Count());
            }

            TeamNames = new AxesCollection
            {
                new Axis
                {
                    Labels = Plan.Teams.Select(t => t.Name).ToList(),
                    ShowLabels = true,
                    Separator = new Separator { Step = 1 }
                }
            };

            TeamYAxis = new AxesCollection
            {
                new Axis
                {
                    LabelFormatter = value => value.ToString(),                    
                    ShowLabels = true,
                    Separator = new Separator { Step = 1 }
                }
            };

            TeamCarryForwards = new SeriesCollection
            {
                new StackedColumnSeries
                {
                    Title = "Completed",
                    Values = new ChartValues<int>(completedCRs),
                    DataLabels = true,
                    Fill = _stateColors[DataModel.ContentState.Completed],
                    MaxColumnWidth = double.PositiveInfinity,
                },
                new StackedColumnSeries
                {
                    Title = "InProgress",
                    Values = new ChartValues<int>(inProgressCRs),
                    DataLabels = true,
                    Fill = _stateColors[DataModel.ContentState.Planned],
                    MaxColumnWidth = double.PositiveInfinity,
                },
                new StackedColumnSeries
                {
                    Title = "CarryForwarded",
                    Values = new ChartValues<int>(carryForwardedCRs),
                    DataLabels = true,
                    Fill = _stateColors[DataModel.ContentState.CarryForwarded],
                    MaxColumnWidth = double.PositiveInfinity,
                },                
            };
        }
    }
}
