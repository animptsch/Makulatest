using MakulaTest.Model;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MakulaTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            
        }
        
        
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DiagnoseControl.StartDiagnosis();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                DiagnoseControl.MarkPoint();
            }

            if (e.Key == Key.Enter)
            {
                DiagnoseControl.StopDiagnosis();
            }
        }
        
        private void BtnAnalyse_Click(object sender, RoutedEventArgs e)
        {
            MyAnalyse.Visibility = Visibility.Visible;
            DiagnoseControl.Visibility = Visibility.Collapsed;            
        }
    }
}
