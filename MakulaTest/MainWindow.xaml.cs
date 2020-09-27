using System.Windows;
using System.Windows.Input;

using MakulaTest.Model;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
            _filePathSettings = FilePathSettings.Instance;
            MyAnalyse.Start(_filePathSettings.CSVDataFilePath);
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var arg = e.AddedItems[0] as FrameworkElement;

            if (arg.Tag != null)
            {
                string tagName = arg.Tag.ToString();

                setImage(arg, tagName, "Dark");

                foreach (var item in e.RemovedItems)
                {
                    var frameworkElement = item as FrameworkElement;
                    tagName = frameworkElement.Tag.ToString();

                    setImage(frameworkElement, tagName, "Light");
                }
            }
        }

        private void setImage(FrameworkElement arg, string tagName, string theme)
        {
            Image image = findChild<Image>(arg);

            if (image != null)
            {
                string resourceName = tagName + theme;
                BitmapSource source = (BitmapSource)Application.Current.Resources[resourceName];
                image.Source = source;
            }
          
        }

        private T findChild<T>(DependencyObject parent)  where T : DependencyObject
        {
            object result = null;

            if (parent is T)
            {
                return parent as T;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                result = findChild<T>(child);
                if (result != null)
                {
                    break;
                }
            }

            return result as T;
        }

    }
}
