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
            MyAnalyse.Start(FilePathSettings.Instance.CSVDataFilePath);
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            Settings.ViewModel = DiagnoseControl.SettingsViewModel;
            Settings.ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            Settings.ViewModel.LoadWindowSettings();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CalibScaleSize")
            {                
                Size size = DiagnoseControl.SettingsViewModel.CalibScaleSize;
                this.DiagnoseControl.SetSize(size.Width, size.Height);
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {                
                if (e.AddedItems[0] is FrameworkElement arg)
                {
                    if (arg.Tag != null)
                    {
                        if (arg.Tag.ToString() == "Analyse")
                        {
                            MyAnalyse.Start(FilePathSettings.Instance.CSVDataFilePath);
                        }

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
            }

            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is TabItem tab)
            {
                if(tab.Tag != null && tab.Tag.ToString() == "Settings")
                {
                    Settings.ViewModel.SaveSettings();
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
