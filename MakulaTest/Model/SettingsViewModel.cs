using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Media;
using System;

namespace MakulaTest.Model
{
    public class SettingsViewModel :INotifyPropertyChanged
    {
                
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

        private BrushConverter _brushConv;

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

        private bool _isBackwardChecked;

        public bool IsBackwardChecked
        {
            get { return _isBackwardChecked; }
            set
            {
                _isBackwardChecked = value;
                _isForwardChecked = value == false;                
                OnPropertyChanged(nameof(IsBackwardChecked));
                UpdateSelectedDuration();
            }
        }

        private bool _isForwardChecked;

        public bool IsForwardChecked
        {
            get { return _isForwardChecked; }
            set
            {
                _isForwardChecked = value;
                _isBackwardChecked = value == false;                
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

            loadSettings();
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

        private readonly string _settingsFileName;
    }
}
