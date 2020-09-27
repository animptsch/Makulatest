using System.Windows;
using System.Windows.Input;

using MakulaTest.Model;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows.Controls;

namespace MakulaTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private AnalyseControl MyAnalyse;
        private MacularDiagnosisControl DiagnoseControl;
        private SettingsView Settings;

        public MainWindow()
        {
            InitializeComponent();

            var arrayList = MainMenu.ItemsSource as System.Collections.ArrayList;
            //MyAnalyse = ((Model.MenuItem)arrayList[0]).Content as AnalyseControl;
            //DiagnoseControl = ((Model.MenuItem)arrayList[1]).Content as MacularDiagnosisControl;
            //Settings = ((Model.MenuItem)arrayList[2]).Content as SettingsView;

            _filePathSettings = FilePathSettings.Instance;
            //MyAnalyse.Start(_filePathSettings.CSVDataFilePath);
        }

        private FilePathSettings _filePathSettings;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            if (_canvasHeight != 0.0 && _canvasWidth != 0.0)
            {
                DiagnoseControl.SetSize(_canvasWidth, _canvasHeight);
            }            
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            

            if (e.Key == Key.Space && DiagnoseControl.SettingsViewModel.IsMeasureStarted)
            {
                DiagnoseControl.MarkPoint();
            }

            if (e.Key == Key.Enter && DiagnoseControl.SettingsViewModel.IsMeasureStarted)
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

        private void BtnSettingSize_Click(object sender, RoutedEventArgs e)
        {
            SaveWindowSettings();
            MessageBox.Show("Windowsdaten wurden gespeichert.");
        }

               
        public void SaveWindowSettings()
        {
            SerializedWindowsState rect = new SerializedWindowsState()
            {
                Left = Application.Current.MainWindow.Left,
                Top = Application.Current.MainWindow.Top,
                Width = Application.Current.MainWindow.Width,
                Height = Application.Current.MainWindow.Height,
                IsMaximized = Application.Current.MainWindow.WindowState == WindowState.Maximized,
                CanvasHeight = _canvasHeight,
                CanvasWidth = _canvasWidth

            };

            var xmlserializer = new XmlSerializer(typeof(SerializedWindowsState));

            if (File.Exists(_filePathSettings.WindowsSettingsFilePath))
            {
                File.Delete(_filePathSettings.WindowsSettingsFilePath);
            }

            using (var fileStream = File.Open(_filePathSettings.WindowsSettingsFilePath, FileMode.Create))
            {
                using (var xWriter = XmlWriter.Create(fileStream))
                {
                    xmlserializer.Serialize(xWriter, rect);
                }
            }
        }
        
        public void LoadSettings()
        {
            SerializedWindowsState rect = null;

            var xmlserializer = new XmlSerializer(typeof(SerializedWindowsState));

            if (File.Exists(_filePathSettings.WindowsSettingsFilePath))
            {
                using (var fileStream = File.OpenRead(_filePathSettings.WindowsSettingsFilePath))
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
              _canvasHeight = rect.CanvasHeight;
              _canvasWidth = rect.CanvasWidth;
            }
            else
            {
              _canvasHeight = 457.0;
              _canvasWidth = 457.0;
            }

        }

        private void BtnScreenCalib_Click(object sender, RoutedEventArgs e)
        {
            var calibDlg = new JustifyGridSize();

            calibDlg.ShowDialog();

            _canvasHeight = calibDlg.MyCanvas.ActualHeight;
            _canvasWidth = calibDlg.MyCanvas.ActualWidth;
            this.DiagnoseControl.SetSize(_canvasWidth, _canvasHeight);
        }

        private double _canvasHeight;
        private double _canvasWidth;

        private void btnSendMail_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
