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


        public bool Backward
        {
            get { return Model.Backward; }
            set
            {
                Model.Backward = value;
                OnPropertyChanged(nameof(Backward));
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
