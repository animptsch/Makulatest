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
using System.Windows.Shapes;

namespace MakulaTest
{
    /// <summary>
    /// Interaction logic for JustifyGridSize.xaml
    /// </summary>
    public partial class JustifyGridSize : Window
    {
        public JustifyGridSize()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

        }

        private Line _horzLine;
        private Line _vertLine;

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_horzLine == null)
            {
                _horzLine = new Line();
                _horzLine.Stroke = new SolidColorBrush(Colors.Black);
                _horzLine.StrokeThickness = 2.0;
                MyCanvas.Children.Add(_horzLine);

                _vertLine = new Line();
                _vertLine.Stroke = new SolidColorBrush(Colors.Black);
                _vertLine.StrokeThickness = 2.0;
                MyCanvas.Children.Add(_vertLine);
            }

                      
            _horzLine.X1 = 50.0;
            _horzLine.Y1 = (e.NewSize.Height) / 2.0;
            _horzLine.X2 = e.NewSize.Width - 50.0; ;
            _horzLine.Y2 = _horzLine.Y1;

            _vertLine.X1 = (e.NewSize.Width) / 2.0;
            _vertLine.Y1 = 50.0;
            _vertLine.X2 = _vertLine.X1;
            _vertLine.Y2 = e.NewSize.Height - 50.0; ;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
