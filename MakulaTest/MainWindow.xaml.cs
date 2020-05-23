using System.Windows;
using System.Windows.Input;

using MakulaTest.Model;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Media;

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

            string baseDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");

            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }

            _windowsSettingsFile = System.IO.Path.Combine(baseDir, "windowsSettings.xml");
        }
               
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            LoadSettings();
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

            if (e.Key == Key.Left && MyAnalyse.Visibility == Visibility.Visible)
            {
              MyAnalyse.GoBackInTime();
            }

            if (e.Key == Key.Right && MyAnalyse.Visibility == Visibility.Visible)
            {
              MyAnalyse.GoForwardInTime();
            }

    }

    private void BtnAnalyse_Click(object sender, RoutedEventArgs e)
        {
            MyAnalyse.Visibility = Visibility.Visible;
            DiagnoseControl.Visibility = Visibility.Collapsed;
            MyAnalyse.Start();
        }

        private void BtnSettingSize_Click(object sender, RoutedEventArgs e)
        {
            SaveWindowSettings();
            MessageBox.Show("Windowsdaten wurden gespeichert.");
        }

        private void BtnDiagnose_Click(object sender, RoutedEventArgs e)
        {
            MyAnalyse.Visibility = Visibility.Collapsed;
            DiagnoseControl.Visibility = Visibility.Visible;
        }

        private void BtnStartMacularDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            btnStartMacularDiagnosis.IsEnabled = false;
            DiagnoseControl.StartDiagnosis();
        }
        
        public void SaveWindowSettings()
        {
            SerializedWindowsState rect = new SerializedWindowsState()
            {
                Left = Application.Current.MainWindow.Left,
                Top = Application.Current.MainWindow.Top,
                Width = Application.Current.MainWindow.Width,
                Height = Application.Current.MainWindow.Height,
                IsMaximized = Application.Current.MainWindow.WindowState == WindowState.Maximized
            };

            var xmlserializer = new XmlSerializer(typeof(SerializedWindowsState));

            if (File.Exists(_windowsSettingsFile))
            {
                File.Delete(_windowsSettingsFile);
            }

            using (var fileStream = File.Open(_windowsSettingsFile, FileMode.Create))
            {
                using (var xWriter = XmlWriter.Create(fileStream))
                {
                    xmlserializer.Serialize(xWriter, rect);
                }
            }
        }

        private string _windowsSettingsFile;

        public void LoadSettings()
        {
            SerializedWindowsState rect = null;

            var xmlserializer = new XmlSerializer(typeof(SerializedWindowsState));

            if (File.Exists(_windowsSettingsFile))
            {
                using (var fileStream = File.OpenRead(_windowsSettingsFile))
                {
                    try
                    {
                        rect = xmlserializer.Deserialize(fileStream) as SerializedWindowsState;
                    }
                    catch
                    {
                        rect = null;
                    }

                }
            }

            if (rect != null)
            {
                Application.Current.MainWindow.Height = rect.Height;
                Application.Current.MainWindow.Left = rect.Left;
                Application.Current.MainWindow.Top = rect.Top;
                Application.Current.MainWindow.Width = rect.Width;

            }

        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsViewModel vm = new SettingsViewModel();
            Settings settings = new Settings(vm);

            bool? result = settings.ShowDialog();
            if (result == true)
            {
                DiagnoseControl.CircleColor = (SolidColorBrush)(new BrushConverter().ConvertFrom(vm.Model.Color));
                DiagnoseControl.CircleSize = vm.Model.CircleSize;
                DiagnoseControl.Duration = vm.Model.Duration;
            }
        }
    }
}
