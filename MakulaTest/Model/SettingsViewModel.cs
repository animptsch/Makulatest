using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System.Xml;

namespace MakulaTest.Model
{
    public class SettingsViewModel :INotifyPropertyChanged
    {
        private FilePathSettings _filePathSettings;

        public const int ModeBackward  = 1;
        public const int ModeForward   = 2;
        public const int ModeFreestyle = 3;

        #region Commands

        public ICommand OpenScaleDialogCommand { get; set; }

        public ICommand SaveWindowsSizeCommand { get; set; }

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
            int selectedDuration = IsBackwardChecked ? DurationBackwards : Duration;

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

        private int _modus;

        public bool IsFreestyleChecked
        {
            get { return (_modus == ModeFreestyle); }
            set
            {
              if (value) _modus = ModeFreestyle;
              OnPropertyChanged(nameof(IsFreestyleChecked));
              UpdateSelectedDuration();
            }
        }

        public bool IsBackwardChecked
        {
            get { return (_modus == ModeBackward); }
            set
            {
                if (value) _modus = ModeBackward;
                OnPropertyChanged(nameof(IsBackwardChecked));
                UpdateSelectedDuration();
            }
        }

        public bool IsForwardChecked
        {
            get { return (_modus == ModeForward); }
            set
            {
               if (value) _modus = ModeForward;
               OnPropertyChanged(nameof(IsForwardChecked));
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
