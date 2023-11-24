using PIPlanner.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PIPlanner.Views
{
    /// <summary>
    /// Interaction logic for TeamSprintView.xaml
    /// </summary>
    public partial class TeamSprintView : UserControl, INotifyPropertyChanged
    {
        public int? SprintId { get; set; }
        public int TeamId { get; set; }
        public bool FeatureView { get; set; }
        internal Func<string, ChangeRequest> DCRFinderFunc { get; set; }

        private int _available;
        public int Available
        {
            get => _available;
            set
            {
                _available = value;
                OnPropertyChanged("Available");
            }
        }

        private Brush _progressColor;
        public Brush ProgressColor
        {
            get => _progressColor;
            set
            {
                if (_progressColor != value)
                {
                    _progressColor = value;
                    OnPropertyChanged("ProgressColor");
                }
            }
        }

        public int Allocated
        {
            get
            {
                int allocated = 0;
                if (_panel?.Children != null)
                {
                    foreach (var item in _panel.Children)
                    {
                        var cr = (item as ChangeRequestView)?.DataContext as ChangeRequest;
                        if (cr != null)
                            allocated += cr.SPs;
                    }
                }

                if (Available == 0 && allocated > 0)
                    ProgressColor = Brushes.Red;
                else
                {
                    if (Math.Abs(Available - allocated) <= Available / 10)
                        ProgressColor = Brushes.Green;
                    else ProgressColor = Brushes.Red;
                }
                return allocated;
            }
        }

        public Action RefreshAction { get; set; }

        public TeamSprintView()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            base.OnDrop(e);
            if (e != null && e.Data != null && e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                if (e.Data.GetData(DataFormats.StringFormat) == null) return;

                var CRId = e.Data.GetData(DataFormats.StringFormat) as string;
                bool duplicate = false;
                foreach (var item in _panel.Children)
                {
                    var cr = (item as ChangeRequestView)?.DataContext as ChangeRequest;
                    if (cr != null && cr.Id == CRId)
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (!duplicate)
                {
                    var CR = DCRFinderFunc(CRId); //MemoryCache<string, ChangeRequestViewModel>.Get(CRId);
                    if (CR != null)
                    {
                        if (CR.Status == DCRStatus.Rejected)
                        {
                            MessageBox.Show("Rejected DCRs cannot be assigned to Sprints. Please Approve the DCR first and then try assignment",
                                "Invalid Operation", MessageBoxButton.OK);
                            return;
                        }

                        CR.BackgroundColor = this.Background;
                        CR.SprintId = SprintId;
                        CR.TeamId = TeamId;
                        _panel.Children.Add(new ChangeRequestView
                        {
                            DataContext = CR,
                            FullMode = false,
                            FeatureMode = FeatureView,
                            RefreshAction = RefreshAction
                        });
                        OnPropertyChanged("Allocated");
                        RefreshAction?.Invoke();
                    }
                }
            }
        }

        internal void EnsureItemValidity(IEnumerable<ChangeRequest> crs, bool fullMode)
        {
            var existingCRs = new List<ChangeRequest>();
            for (int index = 0; index < _panel.Children.Count;)
            {
                ChangeRequestView view = (_panel.Children[index] as ChangeRequestView);
                if (view != null)
                {
                    view.FullMode = fullMode;
                    view.FeatureMode = FeatureView;
                }
                var cr = view?.DataContext as ChangeRequest;
                if (cr != null && (cr.SprintId != SprintId || cr.TeamId != TeamId))
                    _panel.Children.RemoveAt(index);
                else
                {
                    existingCRs.Add(cr);
                    index++;
                }
            }
            if (crs != null)
            {
                var toAdd = crs.Except(existingCRs);
                if (toAdd != null)
                {
                    foreach (var item in toAdd)
                    {
                        _panel.Children.Add(new ChangeRequestView
                        {
                            DataContext = item,
                            FullMode = fullMode,
                            RefreshAction = RefreshAction,
                        });
                        item.BackgroundColor = this.Background;
                    }
                }
            }

            OnPropertyChanged("Allocated");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null) return;
            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to unallocate all the CRs from this Sprint assigned to this team?", "Confirm", MessageBoxButton.YesNo) ==
                MessageBoxResult.Yes)
            {
                for (int index = 0; index < _panel.Children.Count; index++)
                {
                    var cr = (_panel.Children[index] as ChangeRequestView)?.DataContext as ChangeRequest;
                    if (cr != null)
                    {
                        cr.SprintId = cr.TeamId = 0;
                        cr.BackgroundColor = Brushes.Transparent;
                    }
                }
                _panel.Children.Clear();
                OnPropertyChanged("Allocated");
            }
        }
    }
}
