using PIPlanner.DataModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PIPlanner.Views
{
    /// <summary>
    /// Interaction logic for ChangeRequestView.xaml
    /// </summary>
    public partial class ChangeRequestView : UserControl
    {
        public Action RefreshAction { get; set; }

        public bool FullMode
        {
            get => _fullMode.IsChecked.GetValueOrDefault();
            set => _fullMode.IsChecked = value;
        }

        public bool FeatureMode
        {
            get => _featureMode.IsChecked.GetValueOrDefault();
            set => _featureMode.IsChecked = value;
        }

        public ChangeRequestView()
        {
            InitializeComponent();
            FullMode = true;
            FeatureMode = true;
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Package the data.
                DataObject data = new DataObject();
                data.SetData(DataFormats.StringFormat, (DataContext as ChangeRequest).Id);

                // Inititate the drag-and-drop operation.
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void unassignMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var cr = DataContext as ChangeRequest;
            cr.SprintId = cr.TeamId = null;
            cr.BackgroundColor = Brushes.Transparent;
            RefreshAction?.Invoke();
        }

        private void rejectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var cr = DataContext as ChangeRequest;
            if (cr.Status == DCRStatus.Approved)
            {
                if (MessageBox.Show($"Are you sure to REJECT DCR# {cr.Id}? Rejecting the DCR will unassign it from the sprint as well.",
                    "Confirmation",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    cr.Status = DCRStatus.Rejected;
                    cr.SprintId = cr.TeamId = null;
                    cr.BackgroundColor = Brushes.Transparent;
                }
            }
            else if (cr.Status == DCRStatus.Rejected)
            {
                if (MessageBox.Show("Are you sure to APPROVE this DCR?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    cr.Status = DCRStatus.Approved;
                }
            }
            RefreshAction?.Invoke();
        }

        private void _container_MouseLeave(object sender, MouseEventArgs e)
        {
            this.FontSize = 12;
            this.Foreground = Brushes.Black;
        }

        private void _container_MouseEnter(object sender, MouseEventArgs e)
        {
            this.FontSize = 16;
            this.Foreground = Brushes.DarkRed;
        }

        private void editItem_Click(object sender, RoutedEventArgs e)
        {
            //DCREditableList dlg = new DCREditableList
            //{
            //    DataContext = MemoryCache<string, ChangeRequestViewModel>.GetAll().ToObservableCollection()
            //};
            //dlg.ShowDialog();
        }
    }
}
