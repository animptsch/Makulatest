using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MakulaTest
{
    /// <summary>
    /// Interaction logic for AnalyseControl.xaml
    /// </summary>
    public partial class AnalyseControl : UserControl
    {
        public AnalyseControl()
        {
            InitializeComponent();

            var line = new Line()
            {
                X1 = 10,
                Y1 = 10,
                X2 = 300,
                Y2 = 200,
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 4
            };

            MyCanvas.Children.Add(line);

        }
    }
}
