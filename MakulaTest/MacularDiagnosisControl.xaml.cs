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
using System.Collections.Generic;
using System.Text;

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
        private const double EmptyValue = -1;
        private const double _offset = 0.03;
        private Polygon _polygon;

        private DispatcherTimer _moveTimer;
        private DispatcherTimer _removeTimer;
        private double _centerX;
        private double _centerY;
        private Storyboard _sbTranslate;
        private MakulaSession _session;        

        public MacularDiagnosisControl()
        {
            InitializeComponent();
                        
            SettingsViewModel = new SettingsViewModel();
            SettingsModel = SettingsViewModel.Model;
            DataContext = SettingsViewModel;
            SettingsViewModel.IsRightEyeChecked = true;
            SettingsViewModel.IsBackwardChecked = true;

            SettingsViewModel.IsMeasureStarted = false;
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

        private double _lastPos;

        public double LastPos
        {
            get { return _lastPos; }
            set
            {
                _lastPos = value;
                txtLastPos.AppendText(string.Format("{0:00.00}\n", value));
                txtLastPos.ScrollToEnd();
            }
        }

        public const int CircleSize = 8;
        public readonly Brush CircleColor = new SolidColorBrush(Colors.Red);

        public Model.Settings SettingsModel { get; set; }
        public Model.SettingsViewModel SettingsViewModel { get; set; }

        public MainWindow Parent { get; set; }


        public void StartDiagnosis()
        {
            if (!SettingsViewModel.IsMeasureStarted)
            {
                SettingsViewModel.IsMeasureStarted = true;
                _session = new MakulaSession();

                _removeTimer = new DispatcherTimer();
                _removeTimer.Interval = new TimeSpan(0, 0, SettingsModel.Duration);
                _removeTimer.Tick += new EventHandler(_removeTimer_Tick);
                _removeTimer.Start();

                _moveTimer = new DispatcherTimer();
                _moveTimer.Interval = new TimeSpan(0, 0, SettingsModel.Duration + 2);
                _moveTimer.Tick += new EventHandler(_timer_Tick);
                _moveTimer.Start();
                LastPos = _offset;

                if (_polygon != null)
                {
                    MyCanvas.Children.Remove(_polygon);
                    _polygon = null;
                }

                Point pt = getPointInOuterCircle();
                _ellipse = moveCircle(pt, SettingsModel.Backward);
            }
        }

        
        private void resetTimer()
        {
            Point pt = getPointInOuterCircle();

            if (_moveTimer != null && _removeTimer != null && SettingsViewModel.IsMeasureStarted)
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
            //Point pt = getPointInOuterCircle();

            _session.Points.Add(new Point(EmptyValue, EmptyValue));
            UpdateTextBox();

            resetAnimation();
            resetTimer();
        }

        private void resetAnimation()
        {
            if (_sbTranslate != null)
            {
                _sbTranslate.Stop();
                _sbTranslate = null;
            }

            if (_ellipse != null)
            {
                MyCanvas.Children.Remove(_ellipse);
            }

        }

        private void _removeTimer_Tick(object sender, EventArgs e)
        {
            if (_ellipse != null)
            {
                MyCanvas.Children.Remove(_ellipse);
                double pos = 1.0 / (double)SettingsModel.Steps;                
                LastPos -= pos;

                _ellipse = null;
            }
        }


        private Ellipse moveCircle(Point begin, bool backward)
        {
            int durationInSeconds;
            Point end;
            if (!backward)
            {
                end = new Point(_centerX, _centerY);
                durationInSeconds = SettingsModel.Duration;
            }
            else
            {
                end = begin;
                begin = new Point(_centerX, _centerY);
                durationInSeconds = SettingsModel.DurationBackwards;
            }
            
            Point pt = new Point(begin.X - CircleSize / 2, begin.Y - CircleSize / 2);

            var ellipse = drawCircle(pt, CircleColor);            

            _sbTranslate = new Storyboard();
            var daTranslateX = new DoubleAnimation();
            var daTranslateY = new DoubleAnimation();
            var duration = new Duration(TimeSpan.FromSeconds(durationInSeconds));

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
            daTranslateX.To = end.X - begin.X;
            daTranslateY.To = end.Y - begin.Y;

            if (SettingsViewModel.IsMeasureStarted)
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
                UpdateTextBox();
                _sbTranslate.Stop();

                MyCanvas.Children.Remove(_ellipse);
                if (SettingsViewModel.IsMeasureStarted)
                {
                    resetTimer();
                }
            }
        }

        public void StopDiagnosis()
        {
            SettingsViewModel.IsMeasureStarted = false;
            _moveTimer.Stop();
            _removeTimer.Stop();

            _moveTimer = null;
            _removeTimer = null;

            if (_sbTranslate != null)
            {
                _sbTranslate.Stop();
                _sbTranslate = null;
            }

            if (_ellipse != null)
            {
                MyCanvas.Children.Remove(_ellipse);
            }

            drawLines();
            drawCenterCircle();

            _polygon = new Polygon();

            var removedPoints = (from p in _session.Points
                                 where p.X == EmptyValue && p.Y == EmptyValue
                                 select p).ToList();

            foreach (var p in removedPoints)
            {
                _session.Points.Remove(p);
            }

            _polygon.Points = new PointCollection(_session.Points);
            _polygon.Stroke = Brushes.Blue;
            _polygon.Fill = Brushes.White;

            MyCanvas.Children.Add(_polygon);

            MakulaDataSet mds = new MakulaDataSet(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MakulaData.csv"));


            mds.SaveData(_session.Points,
                         SettingsModel.Backward,  // direction
                         SettingsModel.RightEye,  // rightEye
                         CircleSize,
                         MyRectangle);

            Parent.btnStartMacularDiagnosis.IsEnabled = true;
            _session.Points.Clear();
        }

        private Ellipse drawCircle(Point pt, Brush color)
        {
            var ellipse = new Ellipse()
            {
                Width = CircleSize,
                Height = CircleSize,
                Stroke = color,
                Fill = color
            };

            ellipse.SetValue(Canvas.LeftProperty, pt.X);
            ellipse.SetValue(Canvas.TopProperty, pt.Y);
            MyCanvas.Children.Add(ellipse);

            return ellipse;
        }

        private Point getPointInOuterCircle()
        {
            double width = MyRectangle.Width;
            double height = MyRectangle.Height;

            Point pt, ptTan;
            var rnd = new Random();

            double pos = 1.0 / (double)SettingsModel.Steps;
            _pathGeo.GetPointAtFractionLength(LastPos, out pt, out ptTan);
            LastPos += pos;
            if (LastPos >= 1.0 + _offset + pos)
                StopDiagnosis();

            return pt;
        }

        private void drawLines(bool isDrawAmselGrid = true)
        {
            double width = MyRectangle.Width;
            double height = MyRectangle.Height;
            double x = MyRectangle.Margin.Left;
            double y = MyRectangle.Margin.Top;

            _centerX = width / 2.0 + x;
            _centerY = height / 2.0 + y;

            clearCanvas();

            //draw horizontal Line
            double thickness = 3;

            if (isDrawAmselGrid)
            {
                var line1 = new Line()
                {
                    Stroke = new SolidColorBrush(Colors.Black),
                    Fill = new SolidColorBrush(Colors.Black),
                    StrokeThickness = thickness,
                    X1 = x,
                    X2 = width + x,
                    Y1 = _centerY,
                    Y2 = _centerY
                };

                MyCanvas.Children.Add(line1);

                //draw vertical Lines
                var line2 = new Line()
                {
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = thickness,
                    Fill = new SolidColorBrush(Colors.Black),
                    X1 = _centerX,
                    X2 = _centerX,
                    Y1 = y,
                    Y2 = height + y
                };

                MyCanvas.Children.Add(line2);

                EllipseGeometry geo = new EllipseGeometry(new Point(_centerX, _centerY), width / 2 + 30, height / 2 + 30);
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
            Point pt = new Point(_centerX - CircleSize / 2.0, _centerY - CircleSize / 2.0);

            var ellipse = drawCircle(pt, new SolidColorBrush(Colors.Black));                       
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

        private void UpdateTextBox()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in _session.Points)
            {
                sb.AppendLine(string.Format("{0:000.00}, {1:000.00}", item.X, item.Y));
            }
            txtPointList.Text = sb.ToString();
            txtPointList.ScrollToEnd();
        }

        private void MyCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0 && LastPos > _offset)
            {
                double pos = 2.0 / (double)SettingsModel.Steps;
                LastPos -= pos;

                var lastPoint = _session.Points.Last();
                if (lastPoint != null)
                {
                    _session.Points.Remove(lastPoint);
                    UpdateTextBox();
                }
            }

            resetAnimation();
            resetTimer();

            e.Handled = true;
        }


    }
}
