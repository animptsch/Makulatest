using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System.Xml;
using System;

namespace MakulaTest.Model
{
    public enum MeasureMode
    {
        Backward,
        Forward,
        FreeStyle
    }



    public class SettingsViewModel :INotifyPropertyChanged
    {
        private FilePathSettings _filePathSettings;
        
        #region Commands

        public ICommand OpenScaleDialogCommand { get; set; }

        public ICommand SaveWindowsSizeCommand { get; set; }

        public static MeasureMode ParseMeasureMode(string text)
        {
            MeasureMode measureMode = MeasureMode.Backward;


            bool backward = false;
            bool parsed = Enum.TryParse<MeasureMode>(text, out measureMode);

            if (!parsed)
            {
                if (Boolean.TryParse(text, out backward))
                {
                    measureMode = backward ? MeasureMode.Backward : MeasureMode.Forward;
                }
            }

            return measureMode;
        }


        public static string GetMeasureModeText(MeasureMode measureMode)
        {
            string result;

            switch (measureMode)
            {
                case MeasureMode.Backward:
                    result = "von Innen nach Außen";
                    break;
                case MeasureMode.Forward:
                    result = "von Außen nach Innen";
                    break;
                case MeasureMode.FreeStyle:
                default:
                    result = "Freihand Modus";                                    
                    break;
            }

            return result;
        }

        private void saveWindowSettings()
        {
            SerializedWindowsState rect = new SerializedWindowsState()
            {
                Left = Application.Current.MainWindow.Left,
                Top = Application.Current.MainWindow.Top,
                Width = Application.Current.MainWindow.Width,
                Height = Application.Current.MainWindow.Height,
                IsMaximized = Application.Current.MainWindow.WindowState == WindowState.Maximized,
                CanvasHeight = size.Height,
                CanvasWidth = size.Width
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

            MessageBox.Show("Windowsdaten wurden gespeichert.");
        }

        private void openScaleDialog()
        {
            var calibDlg = new JustifyGridSize();

            var res = calibDlg.ShowDialog();

            if (res == true)
            {
                CalibScaleSize = new Size(calibDlg.MyCanvas.ActualWidth, calibDlg.MyCanvas.ActualHeight);
            }

        }

        #endregion

        #region Duration and StepCount Settings

        public int Duration
        {
            get { return Model.Duration; }
            set
            {
                Model.Duration = value;                
                OnPropertyChanged(nameof(Duration));
                UpdateSelectedDuration();
            }
        }

        private void UpdateSelectedDuration()
        {
            int selectedDuration = Mode == MeasureMode.Backward ? DurationBackwards : Duration;

            if (selectedDuration != SelectedDuration)
            {
                SelectedDuration = selectedDuration;
                OnPropertyChanged(nameof(SelectedDuration));
            }
        }

        public int SelectedDuration { get; set; }

        
        public int DurationBackwards
        {
            get { return Model.DurationBackwards; }
            set
            {

                Model.DurationBackwards = value;                
                OnPropertyChanged(nameof(DurationBackwards));
                UpdateSelectedDuration();
            }
        }

        public int Steps
        {
            get { return Model.Steps; }
            set
            {

                Model.Steps = value;
                OnPropertyChanged(nameof(Steps));
            }
        }
        #endregion

        #region Brush and Color Settings

        private BrushConverter _brushConv;


        public Brush BackgroundBrush { get => (SolidColorBrush)_brushConv.ConvertFrom(BackgroundColor); }

        public string BackgroundColor
        {
            get { return Model.BackgroundColor; }
            set
            {
                Model.BackgroundColor = value;
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(BackgroundBrush));
            }
        }

        public Brush LinesBrush { get => (SolidColorBrush)_brushConv.ConvertFrom(LineColor); }

        public string LineColor
        {
            get { return Model.LineColor; }
            set
            {
                Model.LineColor = value;
                OnPropertyChanged(nameof(LineColor));
                OnPropertyChanged(nameof(LinesBrush));
            }
        }


        public Brush MovedBallBrush { get => (SolidColorBrush)_brushConv.ConvertFrom(BallColor); }
        public Brush MovedBallForbidden { get => (SolidColorBrush)_brushConv.ConvertFrom("#ffff0000"); }

        public string BallColor
        {
            get { return Model.BallColor; }
            set
            {
                Model.BallColor = value;
                OnPropertyChanged(nameof(BallColor));
                OnPropertyChanged(nameof(MovedBallBrush));
            }
        }

        public Brush PolygonBrush { get => (SolidColorBrush)_brushConv.ConvertFrom(PolygonColor); }

        public string PolygonColor
        {
            get { return Model.PolygonColor; }
            set
            {
                Model.PolygonColor = value;
                OnPropertyChanged(nameof(PolygonColor));
            }
        }

        #endregion

        #region Left and Right Eye Radio Buttons


        private bool _isRightEyeChecked;

        public bool IsRightEyeChecked
        {
            get { return _isRightEyeChecked; }
            set
            {
                _isRightEyeChecked = value;
                _isLeftEyeChecked = value == false;                
                OnPropertyChanged(nameof(IsRightEyeChecked));
            }
        }

        private bool _isLeftEyeChecked;

        public bool IsLeftEyeChecked
        {
            get { return _isLeftEyeChecked; }
            set
            {
                _isLeftEyeChecked = value;
                _isRightEyeChecked = value == false;                
                OnPropertyChanged(nameof(IsLeftEyeChecked));
            }
        }

        #endregion

        #region Diagnose Modes        

        public bool IsFreestyleChecked
        {
            get { return (measureMode == MeasureMode.FreeStyle); }
            set
            {
              if (value) measureMode = MeasureMode.FreeStyle;
              OnPropertyChanged(nameof(IsFreestyleChecked));
              UpdateSelectedDuration();
            }
        }

        public bool IsBackwardChecked
        {
            get { return (measureMode == MeasureMode.Backward); }
            set
            {
                if (value) measureMode = MeasureMode.Backward;
                OnPropertyChanged(nameof(IsBackwardChecked));
                UpdateSelectedDuration();
            }
        }


        public bool IsForwardChecked
        {
            get { return (measureMode == MeasureMode.Forward); }
            set
            {
                if (value) measureMode = MeasureMode.Forward;
                OnPropertyChanged(nameof(IsForwardChecked));
                UpdateSelectedDuration();
            }
        }


        private MeasureMode measureMode;

        
        public MeasureMode Mode
        {
            get { return measureMode; }
            set 
            {
                measureMode = value;
                OnPropertyChanged(nameof(Mode));
                UpdateSelectedDuration();
            }
        }

        private bool _isMeasureStarted;

        public bool IsMeasureStarted
        {
            get { return _isMeasureStarted; }
            set
            {
                _isMeasureStarted = value;
                OnPropertyChanged(nameof(IsMeasureStarted));
            }
        }

        #endregion

        public Settings Model { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public SettingsViewModel()
        {
            _brushConv = new BrushConverter();
            Model = new Settings();
            
            _settingsFileName = FilePathSettings.Instance.AppSettingsFilePath;

            OpenScaleDialogCommand = new RelayCommand( (object obj) => openScaleDialog());
            SaveWindowsSizeCommand = new RelayCommand((object obj) => saveWindowSettings());
            _filePathSettings = FilePathSettings.Instance;            
            loadSettings();
        }

        private Size size;

        public Size CalibScaleSize
        {
            get { return size; }
            set 
            { 
                size = value;
                OnPropertyChanged(nameof(CalibScaleSize));
            }
        }


        public void SaveSettings()
        {

            using (Stream serializeStream = File.Open(_settingsFileName, FileMode.Create, FileAccess.Write))
            {
                var serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(serializeStream, Model);
            }
        }

        private bool loadSettings()
        {
            bool result = File.Exists(_settingsFileName);

            Settings loadedModel = new Settings();
            if (result)
            {
                try
                {
                    using (Stream fileStream = File.OpenRead(_settingsFileName))
                    {
                        var serializer = new XmlSerializer(typeof(Settings));
                        loadedModel = (Settings)serializer.Deserialize(fileStream);
                    }

                }
                catch (System.Exception)
                {
                    return false;
                }

                Model = loadedModel;                
            }

            return result;
        }




        public void LoadWindowSettings()
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

                        Application.Current.MainWindow.Left = rect.Left;
                        Application.Current.MainWindow.Top= rect.Top;
                        Application.Current.MainWindow.Width = rect.Width;
                        Application.Current.MainWindow.Height = rect.Height;

                    }
                    catch
                    {
                        rect = null;
                    }

                }
            }

            if (rect != null)
            {
                CalibScaleSize = new Size(rect.CanvasWidth, rect.CanvasHeight);
            }
            else
            {
                CalibScaleSize = new Size(457.0, 457.0);                
            }

        }

        private readonly string _settingsFileName;
    }
}
