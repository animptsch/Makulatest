using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MakulaTest.Model
{
    public class MenuItem : INotifyPropertyChanged
    {
        public ImageSource ImageLight { get; set; }
        public ImageSource ImageDark { get; set; }
        public FrameworkElement  Content { get; set; }

        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
