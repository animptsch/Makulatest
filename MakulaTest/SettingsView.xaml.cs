using MakulaTest.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace MakulaTest
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
                        
            
        }

        private SettingsViewModel _vm;

        public SettingsViewModel ViewModel
        {
            get { return _vm; }
            set 
            { 
                _vm = value;
                this.DataContext = _vm;
            }
        }

    }
}
