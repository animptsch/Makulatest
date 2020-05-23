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

        public bool FullColor
        {
            get { return Model.Color == "#FFFF0000"; }
            set
            {
                if(value)
                {
                    Model.Color = "#FFFF0000";
                }
                OnPropertyChanged(nameof(FullColor));                
            }
        }



        public bool HalfColor
        {
            get { return Model.Color == "#80FF0000"; }
            set
            {
                if (value)
                {
                    Model.Color = "#80FF0000";
                }
                OnPropertyChanged(nameof(HalfColor));
            }
        }

        public bool QuaterColor
        {
            get { return Model.Color == "#40FF0000"; }
            set
            {
                if (value)
                {
                    Model.Color = "#40FF0000";
                }
                OnPropertyChanged(nameof(QuaterColor));
            }
        }

        
        public bool SmallSize
        {
            get
            {
                return Model.CircleSize == 10;
            }
            set
            {
                if (value)
                {
                    Model.CircleSize = 10;
                }
                OnPropertyChanged(nameof(SmallSize));
            }
        }


        public bool MiddleSize
        {
            get
            {
                return Model.CircleSize == 15;
            }
            set
            {
                if (value)
                {
                    Model.CircleSize = 15;
                }
                OnPropertyChanged(nameof(MiddleSize));
            }
        }


        public bool LargeSize
        {
            get
            {
                return Model.CircleSize == 20;
            }
            set
            {
                if (value)
                {
                    Model.CircleSize = 20;
                }
                OnPropertyChanged(nameof(LargeSize));
            }
        }


        public bool GiantSize
        {
            get
            {
                return Model.CircleSize == 30;
            }
            set
            {
                if (value)
                {
                    Model.CircleSize = 30;
                }
                OnPropertyChanged(nameof(GiantSize));
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
