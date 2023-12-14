using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Newtonsoft.Json;
using PIPlanner.DataModel;
using PIPlanner.Helpers;
using PIPlanner.Helpers.Exporters;
using PIPlanner.Plugins;
using PIPlanner.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PIPlanner.ViewModels
{
    internal class MainViewModel : PropertyNotifier
    {
        private string _filter;

        private PlanViewModel _plan;
        public PlanViewModel Plan
        {
            get => _plan;
            private set => SetProperty(ref _plan, value);
        }

        private ICollectionView _filteredChangeRequests = null;
        public ICollectionView FilteredChangeRequests
        {
            get
            {
                if (Plan == null)
                    return _filteredChangeRequests;
                if (_filteredChangeRequests == null)
                {
                    _filteredChangeRequests = CollectionViewSource.GetDefaultView(Plan.ChangeRequests);
                    if (_filteredChangeRequests != null)
                    {
                        _filteredChangeRequests.Filter = filterCRs;
                        _filteredChangeRequests.GroupDescriptions.Add(new PropertyGroupDescription("FunctionalArea"));
                    }
                }
                return _filteredChangeRequests;
            }
        }

        public ObservableCollection<string> FunctionalAreas
        {
            get => Plan?.ChangeRequests?
                .Select(cr => cr.FunctionalArea)
                .Distinct()
                .ToObservableCollection();
        }

        private bool _showAllCRs = true;
        public bool ShowAllCRs
        {
            get => _showAllCRs;
            set => SetProperty(ref _showAllCRs, value, () => FilteredChangeRequests.Refresh());
        }

        private bool _showFeaturesOnly = false;
        public bool ShowFeaturesOnly
        {
            get => _showFeaturesOnly;
            set => SetProperty(ref _showFeaturesOnly, value, () => FilteredChangeRequests.Refresh());
        }

        private bool _showCRsOnly = false;
        public bool ShowCRsOnly
        {
            get => _showCRsOnly;
            set => SetProperty(ref _showCRsOnly, value, () => FilteredChangeRequests.Refresh());
        }

        private string _selectedFeatureFilter;
        public string SelectedFeatureFilter
        {
            get => _selectedFeatureFilter;
            set => SetProperty(ref _selectedFeatureFilter, value, () => FilteredChangeRequests.Refresh());
        }

        private double _gridWidth = 10;
        public double GridWidth
        {
            get => _gridWidth;
            set => SetProperty(ref _gridWidth, double.IsInfinity(value) ? 10 : value);
        }

        private double _gridHeight = 10;
        public double GridHeight
        {
            get => _gridHeight;
            set => SetProperty(ref _gridHeight, double.IsInfinity(value) ? 10 : value);
        }

        private ScrumViewModel _scrum;
        public ScrumViewModel Scrum
        {
            get => _scrum;
            private set => SetProperty(ref _scrum, value);
        }

        private PIStatisticsViewModel _piStatistics;
        public PIStatisticsViewModel PIStatistics
        {
            get => _piStatistics;
            private set => SetProperty(ref _piStatistics, value);
        }

        public ICommand NewPlanCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand EditContentCommand { get; private set; }
        public ICommand ImportPlanCommand { get; private set; }
        public ICommand FilterChangedCommand { get; private set; }
        public ICommand RefreshPlanCommand { get; private set; }
        public ICommand Export2HTMLCommand { get; private set; }
        public ICommand Export2CSVCommand { get; private set; }

        public MainViewModel()
        {
            NewPlanCommand = new Command(param => createOrEditPlan(), true);
            LoadCommand = new Command(async (param) => await loadAsync(param), true);
            SaveCommand = new Command(async (param) => await savePlanAsync(), true);
            EditCommand = new Command(param => createOrEditPlan(), true);
            EditContentCommand = new Command(param => editPlanContent(), true);
            ImportPlanCommand = new Command(param => importPlan());
            FilterChangedCommand = new Command((param) => filterChangedAsync(param), true);
            RefreshPlanCommand = new Command(async (param) => await refreshPlanAsync(param));
            Export2HTMLCommand = new Command(param => exportPlan(ExportTypes.HTML), true);
            Export2CSVCommand = new Command(param => exportPlan(ExportTypes.CSV), true);

            ChangeTracker.Initialize(() => RefreshPlanCommand.Execute(null));
        }

        private async Task loadAsync(object param)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Database (.db)|*.db";
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                loadPlan(dlg.FileName);
            }
        }

        private void loadPlan(string filePath)
        {
            Plan = new PlanViewModel(filePath);
            Scrum = new ScrumViewModel(Plan);
            PIStatistics = new PIStatisticsViewModel { Plan = Plan };
            OnPropertyChanged(nameof(FilteredChangeRequests));
            OnPropertyChanged(nameof(FunctionalAreas));
        }

        private async Task savePlanAsync()
        {
            Plan.PlanMetadata.LastModifiedAt = DateTime.Now;
            Plan.Save();
        }

        private void createOrEditPlan()
        {
            NewPIPlanBuilder builder = null;
            if (Plan == null)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".db";
                dlg.Filter = "Database (.db)|*.db";
                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    Plan = new PlanViewModel(dlg.FileName);
                    builder = new NewPIPlanBuilder
                    {
                        Title = "New PIPlan Builder",
                        ViewModel = new NewPlanBuilderViewModel
                        {
                            ShowInformationTab = true,
                            Plan = Plan
                        }
                    };
                }
            }
            else
            {
                builder = new NewPIPlanBuilder
                {
                    Title = "Edit PIPlan",
                    ViewModel = new NewPlanBuilderViewModel
                    {
                        ShowInformationTab = true,
                        Plan = Plan
                    }
                };
            }

            if (builder != null)
            {
                builder.ShowDialog();
                Scrum = new ScrumViewModel(Plan);
                PIStatistics = new PIStatisticsViewModel { Plan = Plan };
                OnPropertyChanged(nameof(Plan));
                OnPropertyChanged(nameof(FilteredChangeRequests));
                OnPropertyChanged(nameof(FunctionalAreas));
            }
        }

        private void editPlanContent()
        {
            NewPIPlanBuilder builder = new NewPIPlanBuilder
            {
                Title = "Edit PIPlan Content",
                ViewModel = new NewPlanBuilderViewModel
                {
                    ShowInformationTab = false,
                    ShowContentTab = true,
                    Plan = Plan
                }
            };
            builder.ShowDialog();
            Scrum = new ScrumViewModel(Plan);
            PIStatistics = new PIStatisticsViewModel { Plan = Plan };
            OnPropertyChanged(nameof(FilteredChangeRequests));
        }

        private void importPlan()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                string filePath = dlg.FileName;
                if (filePath.EndsWith(".json"))
                {
                    string dbFilePath = getTargetFilePath();
                    if (!dbFilePath.EndsWith(".db")) dbFilePath += ".db";

                    var options = new DbContextOptionsBuilder<PIPlannerDbContext>()
                        .UseSqlite($"Data Source={dbFilePath}")
                        .Options;
                    var context = new PIPlannerDbContext(options);

                    LegacyPIPlannerJson json = new LegacyPIPlannerJson();
                    json.FetchData(new List<KeyValue> { new KeyValue("filepath", filePath) });
                    var planInfo = json.Plan;
                    var changeRequests = json.ChangeRequests;
                    var teams = json.Teams;
                    var sprints = json.Sprints;

                    var sprintTeams = new List<SprintTeam>();
                    foreach (var sprint in sprints)
                    {
                        foreach (var team in teams)
                        {
                            sprintTeams.Add(new SprintTeam
                            {
                                Id = sprint.Id * 100 + team.Id,
                                SprintId = sprint.Id,
                                TeamId = team.Id,
                            });
                        }
                    }

                    context.Plans.Add(planInfo);
                    context.ChangeRequests.AddRange(changeRequests);
                    context.Teams.AddRange(teams);
                    context.Sprints.AddRange(sprints);
                    context.SprintTeams.AddRange(sprintTeams);

                    context.SaveChanges();
                    loadPlan(dbFilePath);
                }
            }
        }

        private void exportPlan(ExportTypes exportType)
        {
            if (Plan.HasUnsavedChanges)
            {
                var result = MessageBox.Show("There are unsaved changes to the plan. " +
                    Environment.NewLine +
                    "Click 'Yes' to save the plan before exporting. " +
                    Environment.NewLine +
                    "Click 'No' to continue exporting without saving.",
                    "Unsaved Changes in Plan",
                    MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Cancel)
                    return;
                else if (result == MessageBoxResult.Yes)
                    Plan.Save();
            }

            var fileExtn = ExporterFactory.GetExtensionForExportType(exportType);
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = fileExtn.Item1;
            dlg.Filter = fileExtn.Item2;
            dlg.FileName = Plan.PlanMetadata.Name;
            if (dlg.ShowDialog().GetValueOrDefault())
                ExporterFactory.GetExporter(exportType).Export(Plan, dlg.FileName);
        }

        private async Task refreshPlanAsync(object param)
        {
            Plan.NotifyPlanChanged();
        }

        private string getTargetFilePath()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog().GetValueOrDefault())
                return dlg.FileName;
            return Path.GetRandomFileName();
        }

        private void filterChangedAsync(object param)
        {
            if (param != null)
                _filter = (string)param;
            FilteredChangeRequests.Refresh();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if the item is supposed to be visible</returns>
        private bool filterCRs(object item)
        {
            var cr = item as ChangeRequest;
            if (cr == null) 
                return true;

            if (ShowFeaturesOnly && !cr.IsFeature)
                return false;
            if (ShowCRsOnly && cr.IsFeature)
                return false;
            if (!string.IsNullOrEmpty(SelectedFeatureFilter))
            {
                if (string.IsNullOrEmpty(cr.FunctionalArea))
                    return true;
                if (!cr.FunctionalArea.Contains(SelectedFeatureFilter))
                    return false;
            }
            if (!string.IsNullOrEmpty(_filter))
            {
                if (cr.Id.ToString().Contains(_filter))
                    return true;
                else if (string.IsNullOrEmpty(cr.Summary))
                    return true;
                else if (cr.Summary.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                else if (!string.IsNullOrEmpty(cr.FunctionalArea) &&
                        (cr.FunctionalArea.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0))
                    return true;
                else
                    return false;
            }
            return true;
        }
    }
}
