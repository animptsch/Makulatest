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
    public partial class Settings : Window
    {
        public Settings(SettingsViewModel viewModel)
        {
            InitializeComponent();

            // place the new window in the middle of the screen
            wndSettings.Left = SystemParameters.PrimaryScreenWidth / 2 - wndSettings.Width;
            wndSettings.Top = SystemParameters.PrimaryScreenHeight / 2 - wndSettings.Height / 2;

            _vm = viewModel;
            this.DataContext = _vm;
        }

        private SettingsViewModel _vm;

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            _vm.SaveSettings();
            this.Close();
        }
        

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
