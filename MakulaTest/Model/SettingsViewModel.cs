using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakulaTest.Model
{
    public class SettingsViewModel :INotifyPropertyChanged
    {
                
        public string Duration
        {
            get { return Model.Duration.ToString(); }
            set
            {
                int duration;
                if (int.TryParse(value, out duration))
                {
                    Model.Duration = duration;
                    OnPropertyChanged(nameof(Duration));
                }
            }
        }


        public string DurationBackwards
        {
            get { return Model.DurationBackwards.ToString(); }
            set
            {
                int duration;
                if (int.TryParse(value, out duration))
                {
                    Model.DurationBackwards = duration;
                    OnPropertyChanged(nameof(DurationBackwards));
                }
            }
        }



        public string Steps
        {
            get { return Model.Steps.ToString(); }
            set
            {
                int steps;

                if (int.TryParse(value, out steps))
                {
                    Model.Steps = steps;
                    OnPropertyChanged(nameof(Steps));
                }
            }
        }


        
        public string BackgroundColor
        {
            get { return Model.BackgroundColor; }
            set
            {
                Model.BackgroundColor = value;
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }


        public string LineColor
        {
            get { return Model.LineColor; }
            set
            {
                Model.LineColor = value;
                OnPropertyChanged(nameof(LineColor));
            }
        }


        public string BallColor
        {
            get { return Model.BallColor; }
            set
            {
                Model.BallColor = value;
                OnPropertyChanged(nameof(BallColor));
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
                Model.RightEye = _isRightEyeChecked;
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
                Model.RightEye = _isRightEyeChecked;
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
                Model.Backward = _isBackwardChecked;
                OnPropertyChanged(nameof(IsBackwardChecked));
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
                Model.Backward = _isBackwardChecked;
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



        public Settings Model { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public SettingsViewModel()
        {
            Model = new Settings();            
        }


    }
}
