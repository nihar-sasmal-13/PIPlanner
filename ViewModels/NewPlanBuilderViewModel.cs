using Microsoft.VisualBasic;
using PIPlanner.DataModel;
using PIPlanner.Helpers;
using PIPlanner.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PIPlanner.ViewModels
{
    class NewPlanBuilderViewModel : PropertyNotifier
    {
        private bool _showInformationTab = true;
        public bool ShowInformationTab
        {
            get => _showInformationTab;
            set => SetProperty(ref _showInformationTab, value);
        }

        private bool _showContentTab = false;
        public bool ShowContentTab
        {
            get => _showContentTab;
            set => SetProperty(ref _showContentTab, value);
        }

        private PlanViewModel _plan;
        public PlanViewModel Plan
        {
            get => _plan;
            set => SetProperty(ref _plan, value);
        }

        private string _excelFilePath;
        public string ExcelFilePath
        {
            get => _excelFilePath;
            private set => SetProperty(ref _excelFilePath, value);
        }

        private ObservableCollection<DataTable> _availableTables;
        public ObservableCollection<DataTable> AvailableTables
        {
            get => _availableTables;
            private set => SetProperty(ref _availableTables, value);
        }

        private DataTable _selectedTable;
        public DataTable SelectedTable
        {
            get => _selectedTable;
            set => SetProperty(ref _selectedTable, value, () =>
            {
                DCRFields = value.Columns.ToObservableCollection();
                performSmartSelection();
            });
        }

        private ObservableCollection<DataColumn> _dcrFields;
        public ObservableCollection<DataColumn> DCRFields
        {
            get => _dcrFields;
            set => SetProperty(ref _dcrFields, value);
        }

        private DataColumn _idFieldName;
        public DataColumn IdFieldName
        {
            get => _idFieldName;
            set => SetProperty(ref _idFieldName, value, () => loadContent());
        }

        private DataColumn _summaryFieldName;
        public DataColumn SummaryFieldName
        {
            get => _summaryFieldName;
            set => SetProperty(ref _summaryFieldName, value, () => loadContent());
        }

        private DataColumn _projectFieldName;
        public DataColumn ProjectFieldName
        {
            get => _projectFieldName;
            set => SetProperty(ref _projectFieldName, value, () => loadContent());
        }

        private DataColumn _releaseFieldName;
        public DataColumn ReleaseFieldName
        {
            get => _releaseFieldName;
            set => SetProperty(ref _releaseFieldName, value, () => loadContent());
        }

        private DataColumn _spsFieldName;
        public DataColumn SPsFieldName
        {
            get => _spsFieldName;
            set => SetProperty(ref _spsFieldName, value, () => loadContent());
        }

        private DataColumn _functionalFieldName;
        public DataColumn FunctionalFieldName
        {
            get => _functionalFieldName;
            set => SetProperty(ref _functionalFieldName, value, () => loadContent());
        }

        public void UpdateTeamCount(int count)
        {
            while (count > Plan.Teams.Count)
            {
                Plan.Teams.Add(new Team
                {
                    Id = Plan.Teams.Count + 1,
                    Name = $"Team# {Plan.Teams.Count + 1}",
                    Velocity = 0,
                    TeamColor = Team.TeamColors[Plan.Teams.Count].ToString(),
                });
            }
            while (count < Plan.Teams.Count)
            {
                Plan.Teams.RemoveAt(Plan.Teams.Count - 1);
            }
        }

        public void UpdateSprintCount(int count)
        {
            while (count > Plan.Sprints.Count)
            {
                Plan.Sprints.Add(new Sprint
                {
                    Id = Plan.Sprints.Count + 1,
                    Name = $"Sprint# {Plan.Sprints.Count + 1}",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(10),
                });
            }
            while (count < Plan.Sprints.Count)
            {
                Plan.Sprints.RemoveAt(Plan.Sprints.Count - 1);
            }
        }

        public void UpdateSprintId(int firstSprintId)
        {
            for (int id = firstSprintId, i = 0; i < Plan.Sprints.Count; id++, i++)
            {
                Plan.Sprints[i].Id = id;
                Plan.Sprints[i].Name = $"Sprint# {id}";
            }
        }

        public void UpdateSprintDate(DateTime firstSprintStartDate)
        {
            for (int i = 0; i < Plan.Sprints.Count; i++)
            {
                Plan.Sprints[i].StartDate = firstSprintStartDate.AddDays(i * 14);
                Plan.Sprints[i].EndDate = Plan.Sprints[i].StartDate.AddDays(13);
            }
        }

        public void UpdateSprintTeams()
        {
            //Plan.SprintTeams.Clear();
            foreach (var sprint in Plan.Sprints)
            {
                foreach (var team in Plan.Teams)
                {
                    var sprintTeamId = sprint.Id * 100 + team.Id;
                    var existingSprintTeam = Plan.SprintTeams.FirstOrDefault(st => st.Id == sprintTeamId);
                    if (existingSprintTeam == null)
                    {
                        Plan.SprintTeams.Add(new SprintTeam
                        {
                            Id = sprintTeamId,
                            SprintId = sprint.Id,
                            TeamId = team.Id,
                            PIAvailability = team.Velocity,
                            SprintAvailability = team.Velocity,
                        });
                    }                    
                }
            }
        }

        public void ImportTeams(string filePath)
        {
            PlanViewModel tempPlan = new PlanViewModel(filePath);
            Plan.Teams.Clear();
            tempPlan.Teams.ToList().ForEach(t => Plan.Teams.Add(t));
            tempPlan.Close();
        }

        public void ImportContentFromWindchill()
        {
            IDataSource windchill = new PTCWindchill();
            windchill.FetchData(null, IDataSource.ContentToFetch.CRsOnly);
            Plan.ChangeRequests.Clear();
            windchill.ChangeRequests.ForEach(cr => Plan.ChangeRequests.Add(cr));
        }

        public void ImportContentFromExcel(string filePath)
        {
            ExcelFilePath = filePath;
            DataTableCollection tables = null;
            try
            {
                tables = ExcelReader.ReadExcelFile(filePath);
            }
            catch
            {
                if (MessageBox.Show("Reading file failed. It might be open in other application. Please close and try again. Retry now?",
                    "Error in reading file", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    tables = ExcelReader.ReadExcelFile(filePath);
            }
            if (tables == null || tables.Count == 0)
                MessageBox.Show("Nothing found in the excel sheet. Please check and retry.", "Import Error", MessageBoxButton.OK);
            else if (tables.Count == 1)
            {
                AvailableTables = tables.ToObservableCollection();
                SelectedTable = AvailableTables.First();
                performSmartSelection();
            }
            else
                AvailableTables = tables.ToObservableCollection();
        }

        public void Save()
        {
            Plan.Save();
        }

        private void performSmartSelection()
        {
            if (SelectedTable == null) return;
            if (DCRFields.All(f => f.ColumnName.StartsWith("Column")))
            {
                List<string> colNames = ExcelReader.GetColumnNames(ExcelFilePath, SelectedTable.TableName);
                for (int i = 0; i < DCRFields.Count; i++)
                    DCRFields[i].ColumnName = $"{colNames[i]} ({DCRFields[i].ColumnName})";
                var temp = DCRFields;
                DCRFields = new ObservableCollection<DataColumn>();
                DCRFields = temp;
            }

            foreach (var field in DCRFields)
            {
                if (field.ColumnName.StartsWith("ID"))
                    IdFieldName = field;
                else if (field.ColumnName.StartsWith("Summary"))
                    SummaryFieldName = field;
                else if (field.ColumnName.StartsWith("Project"))
                    ProjectFieldName = field;
                else if (field.ColumnName.Contains("Release"))
                    ReleaseFieldName = field;
                else if (field.ColumnName.StartsWith("Custom Integer"))
                    SPsFieldName = field;
                else if (field.ColumnName.StartsWith("Story Point"))
                    SPsFieldName = field;
                else if (field.ColumnName.StartsWith("Functional Classification"))
                    FunctionalFieldName = field;
            }

            loadContent();
        }

        private void loadContent()
        {
            try
            {
                Plan.ChangeRequests.Clear();
                int startIndex = 0;
                if (DCRFields.All(f => f.ColumnName.Contains("("))) startIndex = 1;
                for (int index = startIndex; index < SelectedTable.Rows.Count; index++)
                {
                    try
                    {
                        string crId = SelectedTable.Rows[index].GetData<string>(IdFieldName, "");
                        string summary = SelectedTable.Rows[index].GetData<string>(SummaryFieldName, "");
                        string project = SelectedTable.Rows[index].GetData<string>(ProjectFieldName, "");
                        string release = SelectedTable.Rows[index].GetData<string>(ReleaseFieldName, "");
                        int sps = SelectedTable.Rows[index].GetData<int>(SPsFieldName, 0);
                        string fa = SelectedTable.Rows[index].GetData(FunctionalFieldName, true, ",");
                        ChangeRequest cr = new ChangeRequest
                        {
                            DBId = crId,
                            Id = crId,
                            Summary = summary,
                            Project = project,
                            Release = release,
                            SPs = sps,
                            FunctionalArea = fa,
                        };
                        Plan.ChangeRequests.Add(cr);
                    }
                    catch
                    {
                        //ignore all errors here
                    }
                }
            }
            catch
            {
                //ignore all errors
            }
        }
    }
}
