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
    /// Interaction logic for MakulaNorm.xaml
    /// </summary>
    public partial class MakulaNorm : Window
    {
        public MakulaNorm()
        {
            InitializeComponent();


            

        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(MyCanvas);

            if (p.X < 0 || p.Y < 0)
            {
                MyEllipse.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (MyEllipse.Visibility == Visibility.Collapsed)
                {
                    MyEllipse.Visibility = Visibility.Visible;
                }

            }

            MyEllipse.SetValue(Canvas.LeftProperty, p.X - 3);
            MyEllipse.SetValue(Canvas.TopProperty, p.Y - 3);           
        }
        

        private void MyCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            MyEllipse.Visibility = Visibility.Collapsed;
        }
    }


}

