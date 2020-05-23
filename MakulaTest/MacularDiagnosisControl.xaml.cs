using MakulaTest.Model;
using System;
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
        
        private DispatcherTimer _moveTimer;
        private DispatcherTimer _removeTimer;
        private double _centerX;
        private double _centerY;
        private Storyboard _sbTranslate;
        private MakulaSession _session;
        private double _lastPos;

        public MacularDiagnosisControl()
        {
            InitializeComponent();

            drawLines();
            drawCenterCircle();
            Duration = 10;
            CircleColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ffaacc"));
            CircleSize = 15;            
        }


        public double CircleSize { get; set; }
        public Brush CircleColor { get; set; }

        public int Duration { get; set; }

        public double MinSize
        {
            get { return (double)GetValue(MinSizeProperty); }
            set { SetValue(MinSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinSizeProperty =
            DependencyProperty.Register("MinSize", typeof(double), typeof(MacularDiagnosisControl), new PropertyMetadata(0.0));
        

        public void StartDiagnosis()
        {
            _session = new MakulaSession();
            
            //_removeTimer = new DispatcherTimer();
            //_removeTimer.Interval = new TimeSpan(0, 0, Duration);
            //_removeTimer.Tick += new EventHandler(_removeTimer_Tick);
            //_removeTimer.Start();

            //_moveTimer = new DispatcherTimer();
            //_moveTimer.Interval = new TimeSpan(0, 0, Duration+2);
            //_moveTimer.Tick += new EventHandler(_timer_Tick);
            //_moveTimer.Start();
            _lastPos = 0.0;

            

            while (_lastPos <= 1.0)
            {
                Point pt = getPointInOuterCircle();                
                var line = new Line()
                {                 
                    Stroke = new SolidColorBrush(Colors.Red),
                    Fill = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 5,
                    X1 = pt.X + CircleSize / 2.0,
                    X2 = _centerX + CircleSize / 2.0,
                    Y1 = pt.Y + CircleSize / 2.0,
                    Y2 = _centerY + CircleSize / 2.0
                };
                line.MouseDown += Line_MouseDown;
                MyCanvas.Children.Add(line);
            }

            //
            //_ellipse = moveCircle(pt);
        }

        private void Line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(MyCanvas);
            var line = sender as Line;
            line.Stroke = Brushes.Blue;
            line.X2 = p.X;
            line.Y2 = p.Y;
        }

        private void resetTimer()
        {
            _moveTimer.Stop();
            _removeTimer.Stop();
            _removeTimer.Start();
            _moveTimer.Start();

            Point pt = getPointInOuterCircle();
            _ellipse = moveCircle(pt);
        }


        private void _timer_Tick(object sender, EventArgs e)
        {
            Point pt = getPointInOuterCircle();
            _ellipse = moveCircle(pt);
            _removeTimer.Stop();
            _removeTimer.Start();
        }

        private void _removeTimer_Tick(object sender, EventArgs e)
        {
            if (_ellipse != null)
            {
                MyCanvas.Children.Remove(_ellipse);
                _ellipse = null;
            }
        }


        private Ellipse moveCircle(Point begin)
        {
            var ellipse = new Ellipse()
            {
                Width = CircleSize,
                Height = CircleSize,
                Stroke = CircleColor,
                Fill = CircleColor
            };

            Point end = new Point(_centerX, _centerY);
            ellipse.SetValue(Canvas.LeftProperty, begin.X - CircleSize / 2.0);
            ellipse.SetValue(Canvas.TopProperty, begin.Y - CircleSize / 2.0);
            MyCanvas.Children.Add(ellipse);

            _sbTranslate = new Storyboard();
            var daTranslateX = new DoubleAnimation();
            var daTranslateY = new DoubleAnimation();
            var duration = new Duration(TimeSpan.FromSeconds(Duration));

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
            daTranslateX.To = end.X - begin.X + CircleSize / 2.0;
            daTranslateY.To = end.Y - begin.Y + CircleSize / 2.0;

            _sbTranslate.Begin();

            return ellipse;
        }

        public void MarkPoint()
        {
            if (_sbTranslate != null)
            {
                Point relativePoint = _ellipse.TransformToAncestor(MyCanvas)
                                              .Transform(new Point(CircleSize / 2.0, CircleSize / 2.0));
                _session.Points.Add(relativePoint);
                _sbTranslate.Stop();

                MyCanvas.Children.Remove(_ellipse);
                resetTimer(); 
            }
        }

        public void StopDiagnosis()
        {
            if (_ellipse != null)
            {
                MyCanvas.Children.Remove(_ellipse);
            }
            drawLines();
            drawCenterCircle();

            double lineLength = 10.0;

            var polygon = new Polygon();
            polygon.Points = new PointCollection( _session.Points);
            polygon.Stroke = Brushes.Blue;
            polygon.Fill = Brushes.White;

            MyCanvas.Children.Add(polygon);

            MakulaDataSet mds = new MakulaDataSet(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MakulaData.csv"));
            mds.SaveData(_session.Points, 22, 10, true); // size, intensity, rightEye
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

            ellipse.SetValue(Canvas.LeftProperty, pt.X - CircleSize / 2.0);
            ellipse.SetValue(Canvas.TopProperty, pt.Y - CircleSize / 2.0);
            MyCanvas.Children.Add(ellipse);
        }

        private Point getPointInOuterCircle()
        {
            double width = MyRectangle.Width;
            double height = MyRectangle.Height;
      
            Point pt, ptTan;
            var rnd = new Random();

            double pos = 1/20.0;
            _lastPos += pos;
            _pathGeo.GetPointAtFractionLength(_lastPos, out pt, out ptTan);
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


        private void drawLines()
        {
            double width = MinSize;
            double height = MinSize;
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
                    _centerX = lineHeight - CircleSize / 2.0;
                    thickness = 3;
                }
                else
                {
                    thickness = 1;
                }

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
                lineHeight += deltaVert;
            }

            //draw vertical Lines
            for (int i = 0; i < LineNumber; i++)
            {
                double thickness;

                if (centerIndex == i)
                {
                    _centerY = lineWidth - CircleSize / 2.0;
                    thickness = 3;
                }
                else
                {
                    thickness = 1;
                }

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
                Fill = new SolidColorBrush(Colors.Black)
            };

            ellipse.SetValue(Canvas.LeftProperty, _centerX );
            ellipse.SetValue(Canvas.TopProperty, _centerY );
            MyCanvas.Children.Add(ellipse);
        }

        private void MyCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < e.NewSize.Height)
            {
                MinSize = e.NewSize.Width - (MyRectangle.Margin.Left +MyRectangle.Margin.Right);
            }
            else
            {
                MinSize = e.NewSize.Height - (MyRectangle.Margin.Bottom + MyRectangle.Margin.Top);
            }

            drawLines();
            drawCenterCircle(); 
            
        }

        private void MyCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
          MarkPoint();
        }

  }
}
