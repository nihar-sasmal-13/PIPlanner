using PIPlanner.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace PIPlanner.Plugins
{
    /// <summary>
    /// Interaction logic for UserInputDialog.xaml
    /// </summary>
    public partial class UserInputDialog : Window
    {
        public List<KeyValue> UserInputs { get; set; }

        public UserInputDialog()
        {
            InitializeComponent();
            UserInputs = new List<KeyValue>();
            DataContext = this;
        }

        public UserInputDialog(List<KeyValue> keysWithDefaults)
        {
            InitializeComponent();
            UserInputs = keysWithDefaults;
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
