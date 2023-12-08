using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Win32;
using PIPlanner.DataModel;
using PIPlanner.Helpers;
using PIPlanner.Plugins;
using PIPlanner.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
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
    /// Interaction logic for NewPIPlanBuilder.xaml
    /// </summary>
    internal partial class NewPIPlanBuilder : Window
    {
        public NewPlanBuilderViewModel ViewModel 
        {
            get => DataContext as NewPlanBuilderViewModel;
            set => DataContext = value;
        }

        public NewPIPlanBuilder()
        {
            InitializeComponent();            
        }

        private void _teamCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            string enteredCount = _teamCount.Text;
            int count = 0;
            if (int.TryParse(enteredCount, out count))
            {
                if (count > 0)
                    ViewModel.UpdateTeamCount(count);
            }
        }

        private void _sprintCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            string enteredCount = _sprintCount.Text;
            int count = 0;
            if (int.TryParse(enteredCount, out count))
            {
                if (count > 0)
                    ViewModel.UpdateSprintCount(count);
            }
        }

        private void _firstSprintId_TextChanged(object sender, TextChangedEventArgs e)
        {
            string enteredId = _firstSprintId.Text;
            int firstSprintId = 0;
            if (int.TryParse(enteredId, out firstSprintId))
                ViewModel.UpdateSprintId(firstSprintId);
        }

        private void _firstSprintStartDate_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? selectedDate = _firstSprintStartDate.SelectedDate;
            if (selectedDate != null)
                ViewModel.UpdateSprintDate(selectedDate.Value);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateSprintTeams();
            ViewModel.Save();
            Close();
        }

        private void importTeamsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog().GetValueOrDefault())
                ViewModel.ImportTeams(dlg.FileName);
        }

        private void importFromWindchill_Click(object sender, RoutedEventArgs e)
        {
            _excelDataImportSection.Visibility = Visibility.Collapsed;
            ViewModel.ImportContentFromWindchill();
        }

        private void importFromExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                _excelDataImportSection.Visibility = Visibility.Visible;
                ViewModel.ImportContentFromExcel(dlg.FileName);
            }
        }

        private void DataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            var guid = Guid.NewGuid().ToString("D");
            e.NewItem = new ChangeRequest
            {
                Id = guid.Substring(0, guid.IndexOf('-')),
                Project = string.Empty,
                Release = string.Empty,
                FunctionalArea = string.Empty,
            };
        }
    }
}
