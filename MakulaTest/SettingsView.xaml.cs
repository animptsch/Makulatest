using MakulaTest.Model;
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
                        
            _vm = new SettingsViewModel();
            this.DataContext = _vm;
        }

        private SettingsViewModel _vm;

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {          
            _vm.SaveSettings();          
        }
        

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {            
        }
    }
}
