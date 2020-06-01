using MakulaTest.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Input;

namespace MakulaTest
{
    /// <summary>
    /// Interaction logic for MacularDiagnosisControl.xaml
    /// </summary>
    public partial class MacularDiagnosisControl : UserControl
    {
        private Ellipse _ellipse;
        private PathGeometry _pathGeo;
        private const int LineNumber = 17;
        private const double _offset = 0.03;

        
        private DispatcherTimer _moveTimer;
        private DispatcherTimer _removeTimer;
        private double _centerX;
        private double _centerY;
        private Storyboard _sbTranslate;
        private MakulaSession _session;
        private double _lastPos;
        private bool _isMeasureStarted;

        public MacularDiagnosisControl()
        {
            InitializeComponent();
            SettingsModel = new Model.Settings();
            _isMeasureStarted = false;
        }

        public void SetSize(double width, double height)
        {
            Visibility = Visibility.Visible;
            MyCanvas.Width = width;
            MyCanvas.Height = height;

            MyRectangle.Height = height - MyRectangle.Margin.Top - MyRectangle.Margin.Bottom;
            MyRectangle.Width = width - MyRectangle.Margin.Left - MyRectangle.Margin.Right;

            drawLines();
            drawCenterCircle();
            
        }


        public const int CircleSize = 8;
        public readonly Brush CircleColor = new SolidColorBrush(Colors.Red);

        public Model.Settings SettingsModel { get; set; }
        public MainWindow Parent { get; set; }

        // Using a DependencyProperty as the backing store for MinSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinSizeProperty =
            DependencyProperty.Register("MinSize", typeof(double), typeof(MacularDiagnosisControl), new PropertyMetadata(0.0));
        

        public void StartDiagnosis()
        {
            if (!_isMeasureStarted)
            {
                _isMeasureStarted = true;
                _session = new MakulaSession();

                _removeTimer = new DispatcherTimer();
                _removeTimer.Interval = new TimeSpan(0, 0, SettingsModel.Duration);
                _removeTimer.Tick += new EventHandler(_removeTimer_Tick);
                _removeTimer.Start();

                _moveTimer = new DispatcherTimer();
                _moveTimer.Interval = new TimeSpan(0, 0, SettingsModel.Duration + 2);
                _moveTimer.Tick += new EventHandler(_timer_Tick);
                _moveTimer.Start();
                _lastPos = _offset;

                Point pt = getPointInOuterCircle();
                _ellipse = moveCircle(pt, SettingsModel.Backward);
            }           
        }

        private void Line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(MyCanvas);
            var line = sender as Line;
            line.Stroke = Brushes.Blue;

            double m = (line.Y1 - _centerY) / (_centerX - line.X1);
            if (Math.Abs(m) < 10)
            { //Console.WriteLine("m=" + m.ToString());
              line.Y2 = _centerY - (p.X - _centerX) * m;
              line.X2 = p.X;
            }
            else
            { m = (_centerX - line.X1) / (line.Y1 - _centerY);
              //Console.WriteLine("m=" + m.ToString());
              line.X2 = (_centerY - p.Y) * m + _centerX;
              line.Y2 = p.Y;
            }
        }

        private void resetTimer()
        {
            Point pt = getPointInOuterCircle();

            if (_moveTimer != null && _removeTimer != null && _isMeasureStarted)
           {
             _moveTimer.Stop();
             _removeTimer.Stop();
             _removeTimer.Start();
             _moveTimer.Start();
             
             _ellipse = moveCircle(pt, SettingsModel.Backward);
           }
        }


        private void _timer_Tick(object sender, EventArgs e)
        {
            cancelMovement();
        }

        private void cancelMovement()
        {
            Point pt = getPointInOuterCircle();


            if (_sbTranslate != null)
            {
                _sbTranslate.Stop();
            }

            if (_ellipse != null)
            {
                MyCanvas.Children.Remove(_ellipse);
            }

            _ellipse = moveCircle(pt, SettingsModel.Backward);
            if (_removeTimer != null && _isMeasureStarted)
            {
                _removeTimer.Stop();
                _removeTimer.Start(); 
            }
        }

        private void _removeTimer_Tick(object sender, EventArgs e)
        {
            if (_ellipse != null)
            {
                MyCanvas.Children.Remove(_ellipse);
                _ellipse = null;
            }
        }


        private Ellipse moveCircle(Point begin, bool backward)
        {
            var ellipse = new Ellipse()
            {
                Width = CircleSize,
                Height = CircleSize,
                Stroke = CircleColor,
                Fill = CircleColor
            };

            Point end;
            if (!backward)
            {
                end = new Point(_centerX, _centerY);
            }
            else
            {
                end = begin;
                begin = new Point(_centerX, _centerY);
            }
            
            ellipse.SetValue(Canvas.LeftProperty, begin.X - CircleSize/2);
            ellipse.SetValue(Canvas.TopProperty, begin.Y - CircleSize / 2);
            MyCanvas.Children.Add(ellipse);

            _sbTranslate = new Storyboard();
            var daTranslateX = new DoubleAnimation();
            var daTranslateY = new DoubleAnimation();
            var duration = new Duration(TimeSpan.FromSeconds(SettingsModel.Duration));

            daTranslateX.Duration = duration;
            daTranslateY.Duration = duration;
            _sbTranslate.Duration = duration;

            _sbTranslate.Children.Add(daTranslateX);
            _sbTranslate.Children.Add(daTranslateY);

            Storyboard.SetTarget(daTranslateX, ellipse);
            Storyboard.SetTarget(daTranslateY, ellipse);

            ellipse.RenderTransform = new TranslateTransform();

            Storyboard.SetTargetProperty(daTranslateX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            Storyboard.SetTargetProperty(daTranslateY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

            //animate the item to the center of the screen
            daTranslateX.To = end.X - begin.X ;
            daTranslateY.To = end.Y - begin.Y ;

            if (_isMeasureStarted)
            {
                _sbTranslate.Begin();
            }
            else
            {
                int i = 0;
            }
           
            return ellipse;
        }

        public void MarkPoint()
        {
            if (_sbTranslate != null && _ellipse != null)
            {
                Point relativePoint = _ellipse.TransformToAncestor(MyCanvas)
                                              .Transform(new Point(CircleSize / 2.0, CircleSize / 2.0));
                _session.Points.Add(relativePoint);
                _sbTranslate.Stop();

                MyCanvas.Children.Remove(_ellipse);
                if (_isMeasureStarted)
                {
                    resetTimer();
                }
            }
        }

        public void StopDiagnosis()
        {
            _isMeasureStarted = false;
            _moveTimer.Stop();
            _removeTimer.Stop();            
            
            _moveTimer = null;
            _removeTimer = null;
      
            _sbTranslate.Stop();
            _sbTranslate = null;

            if (_ellipse != null)
            {
                MyCanvas.Children.Remove(_ellipse);
            }

            drawLines();
            drawCenterCircle();
            
            var polygon = new Polygon();
            polygon.Points = new PointCollection( _session.Points);
            polygon.Stroke = Brushes.Blue;
            polygon.Fill = Brushes.White;

            MyCanvas.Children.Add(polygon);

            MakulaDataSet mds = new MakulaDataSet(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MakulaData.csv"));
            mds.SaveData(_session.Points, SettingsModel.Backward, true, CircleSize, 354, 354, (int)MyCanvas.Width, (int)MyCanvas.Height); // direction, rightEye, CircleSize, midX, midY, monitorWidth (mm), monitorHeight (mm)            
            Parent.btnStartMacularDiagnosis.IsEnabled = true;
        }

    private void drawCircle(Point pt)
        {
            var ellipse = new Ellipse()
            {
                Width = CircleSize,
                Height = CircleSize,
                Stroke = CircleColor,
                Fill = CircleColor
            };

            ellipse.SetValue(Canvas.LeftProperty, pt.X );
            ellipse.SetValue(Canvas.TopProperty, pt.Y);
            MyCanvas.Children.Add(ellipse);
        }

        private Point getPointInOuterCircle()
        {
            double width = MyRectangle.Width;
            double height = MyRectangle.Height;
      
            Point pt, ptTan;
            var rnd = new Random();

            double pos = 1.0 / (double)SettingsModel.Steps;
            _pathGeo.GetPointAtFractionLength(_lastPos, out pt, out ptTan);
            _lastPos += pos;
            if (_lastPos >= 1.0 +_offset + pos)
              StopDiagnosis();

            return pt;
        }

        

        private void drawOuterCircle()
        {
            var ellipse = new Ellipse()
            {
                Width = 708.0,
                Height = 708.0,
                Stroke = new SolidColorBrush(Colors.Red),
            };

            ellipse.SetValue(Canvas.LeftProperty, -50.0);
            ellipse.SetValue(Canvas.TopProperty, -50.0);
            MyCanvas.Children.Add(ellipse);
        }


        private void drawLines(bool isDrawAmselGrid = false)
        {
            double width = MyRectangle.Width;
            double height = MyRectangle.Height;
            double x = MyRectangle.Margin.Left;
            double y = MyRectangle.Margin.Top;
            _centerY = _centerX = 0.0;

            clearCanvas();
            double deltaHorz = width / LineNumber;
            double deltaVert = height / LineNumber;

            double lineHeight = y + deltaVert, lineWidth = x + deltaHorz;

            int centerIndex = LineNumber / 2;

            //draw horizontal Lines
            for (int i = 0; i < LineNumber; i++)
            {
                double thickness;
                if (centerIndex == i)
                {
                    _centerY = lineHeight;
                    thickness = 3;
                }
                else
                {
                    thickness = 1;
                }

                if (isDrawAmselGrid || centerIndex ==i)
                {
                    var line = new Line()
                    {
                        Stroke = new SolidColorBrush(Colors.Black),
                        Fill = new SolidColorBrush(Colors.Black),
                        StrokeThickness = thickness,
                        X1 = x,
                        X2 = width + x,
                        Y1 = lineHeight,
                        Y2 = lineHeight
                    };

                    MyCanvas.Children.Add(line);
                }
                
                lineHeight += deltaVert;
            }

            //draw vertical Lines
            for (int i = 0; i < LineNumber; i++)
            {
                double thickness;

                if (centerIndex == i)
                {
                    _centerX = lineWidth;
                    thickness = 3;
                }
                else
                {
                    thickness = 1;
                }

                if (isDrawAmselGrid || centerIndex == i)
                {
                    var line = new Line()
                    {
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = thickness,
                        Fill = new SolidColorBrush(Colors.Black),
                        X1 = lineWidth,
                        X2 = lineWidth,
                        Y1 = y,
                        Y2 = height + y
                    };

                    MyCanvas.Children.Add(line);
                }
                              
                lineWidth += deltaHorz;

                EllipseGeometry geo = new EllipseGeometry(new Point(_centerX, _centerY), 354, 354);
                _pathGeo = geo.GetFlattenedPathGeometry();
            }
        }

        private void clearCanvas()
        {
            MyCanvas.Children.Clear();
            MyCanvas.Children.Add(MyRectangle);
        }

        private void drawCenterCircle()
        {
            var ellipse = new Ellipse()
            {
                Width = CircleSize,
                Height = CircleSize,
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Colors.Black),
                //HorizontalAlignment = HorizontalAlignment.Center,
                //VerticalAlignment = VerticalAlignment.Center,
            };
 
            ellipse.SetValue(Canvas.LeftProperty, _centerX- CircleSize / 2.0);
            ellipse.SetValue(Canvas.TopProperty, _centerY - CircleSize / 2.0);
            MyCanvas.Children.Add(ellipse);
        }

        private void MyCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                MarkPoint();
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (_ellipse != null)
                {
                    MyCanvas.Children.Remove(_ellipse);
                    _ellipse = null;
                }
                cancelMovement();
            }
          
        }

        private void MyCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0 && _lastPos > _offset )
            {
                double pos = 2.0 / (double)SettingsModel.Steps;
                _lastPos -= pos;

                txtDelta.Text = _lastPos.ToString();
                var lastPoint = _session.Points.Last();
                if (lastPoint != null)
                {
                    _session.Points.Remove(lastPoint);
                }
            }
            cancelMovement();
                        
            e.Handled = true;
        }

        
    }
}
