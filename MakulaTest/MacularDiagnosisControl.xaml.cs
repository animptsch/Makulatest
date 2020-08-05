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
        private Draw _draw;

        public MacularDiagnosisControl()
        {
            InitializeComponent();
                        
            SettingsViewModel = new SettingsViewModel();            
            DataContext = SettingsViewModel;
            SettingsViewModel.IsRightEyeChecked = true;
            SettingsViewModel.IsBackwardChecked = true;

            SettingsViewModel.IsMeasureStarted = false;
            _draw = new Draw(MyCanvas);
            _session = new MakulaSession();
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

        private int _currentPointIndex;

        public const int CircleSize = 8;
        
        public Model.SettingsViewModel SettingsViewModel { get; set; }

        public MainWindow Parent { get; set; }


        public void StartDiagnosis()
        {
            if (!SettingsViewModel.IsMeasureStarted)
            {
                MyCanvas.Background = SettingsViewModel.BackgroundBrush;                
                
                drawLines();
                drawCenterCircle();

                SettingsViewModel.IsMeasureStarted = true;

                _removeTimer = new DispatcherTimer();
                _removeTimer.Interval = new TimeSpan(0, 0, SettingsViewModel.SelectedDuration);
                _removeTimer.Tick += new EventHandler(_removeTimer_Tick);
                _removeTimer.Start();

                _moveTimer = new DispatcherTimer();
                _moveTimer.Interval = new TimeSpan(0, 0, SettingsViewModel.SelectedDuration + 2);
                _moveTimer.Tick += new EventHandler(_timer_Tick);
                _moveTimer.Start();
                _currentPointIndex = 0;

                if (_polygon != null)
                {
                    MyCanvas.Children.Remove(_polygon);
                    _polygon = null;
                }

                Point pt = getPointInOuterCircle();
                _ellipse = moveCircle(pt, SettingsViewModel.IsBackwardChecked);
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

                _ellipse = moveCircle(pt, SettingsViewModel.IsBackwardChecked);
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
                double pos = 1.0 / (double)SettingsViewModel.Steps;                

                _ellipse = null;
            }
        }


        private Ellipse moveCircle(Point begin, bool backward)
        {
            int durationInSeconds;
            Point end;

            durationInSeconds = SettingsViewModel.SelectedDuration;


            if (!backward)
            {
                end = new Point(_centerX, _centerY);                
            }
            else
            {
                end = begin;
                begin = new Point(_centerX, _centerY);                
            }

            var ellipse = _draw.DrawCircle(begin.X, begin.Y, CircleSize, SettingsViewModel.MovedBallBrush);

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
                _currentPointIndex++;
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
            _polygon.Stroke = SettingsViewModel.PolygonBrush;            
         
            MyCanvas.Children.Add(_polygon);

            MakulaDataSet mds = new MakulaDataSet(FilePathSettings.Instance.CSVDataFilePath);

            mds.SaveData(_session.Points,                
                         SettingsViewModel.IsBackwardChecked,  // direction
                         SettingsViewModel.IsRightEyeChecked,  // rightEye
                         CircleSize,
                         MyRectangle);

            Parent.btnStartMacularDiagnosis.IsEnabled = true;
            _session.Points.Clear();
        }

        private Point getPointInOuterCircle()
        {
          if (_currentPointIndex >= SettingsViewModel.Steps)
          { StopDiagnosis();
            return _session.StartingPoints[0];
          }

          if (_currentPointIndex < 0)
          { Console.WriteLine("ERROR: _currentPointIndex=" + _currentPointIndex.ToString());
            _currentPointIndex = 0;
          }
        
          return _session.StartingPoints[_currentPointIndex];
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
           
            if (isDrawAmselGrid)
            {   double thickness = 3;

                //draw horizontal Line
                _draw.DrawLine(x, _centerY, width + x, _centerY, thickness, SettingsViewModel.LinesBrush);

                //draw vertical Line
                _draw.DrawLine(_centerX, y, _centerX, height + y, thickness, SettingsViewModel.LinesBrush);

                DetermineStartingPoints(width / 2 + 30, height / 2 + 30);
      
            }
        }


        private void DetermineStartingPoints(double width, double height)
        {
          _session.StartingPoints.Clear();
          _currentPointIndex = 0;

          for (var i = 0; i < SettingsViewModel.Steps; i++)
          {
            double anglePart = CorrectedAngle(1.0*(SettingsViewModel.Steps - i - 1) / SettingsViewModel.Steps + 0.25 ); //+ 0.25+ _offset
            double angle = anglePart * 2 * Math.PI;
            //Console.WriteLine(i.ToString() + ". anglePart=" + anglePart.ToString());

            Point pt = new Point(Math.Sin(angle) * width + _centerX, Math.Cos(angle) * height + _centerY);
            _session.StartingPoints.Add(pt);
          }
        }


        private double CorrectedAngle(double anglePart)
        {
          double tolerance = 0.03;
          //string outString= "Korrektur für " + anglePart.ToString();
 
          //while (anglePart > 1.0) anglePart -= tolerance;

          for (double boundery=0.00; boundery < 2.0; boundery+= 0.25)
            anglePart = CheckAndCorrect(anglePart, boundery, tolerance);

          //Console.WriteLine(outString + " auf " +anglePart.ToString());

          return anglePart;
        }

        private double CheckAndCorrect(double pos, double boundery, double tolerance)
        {
          if (Math.Abs(pos - boundery) < tolerance)
          {
            if (Math.Abs(pos - boundery - tolerance) >= tolerance || Math.Abs(pos - tolerance) > 1.0)
              return Math.Abs(pos - tolerance);
            else
              return Math.Abs(pos + tolerance);
          }
          return pos;
        }


        private void clearCanvas()
        {
            MyCanvas.Background = SettingsViewModel.BackgroundBrush;
            MyCanvas.Children.Clear();
            MyCanvas.Children.Add(MyRectangle);
            MyRectangle.Fill = SettingsViewModel.BackgroundBrush;            
        }

        private void drawCenterCircle()
        {            
            _draw.DrawCircle(_centerX, _centerY, CircleSize, SettingsViewModel.LinesBrush);
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
            if (e.Delta < 0 && _currentPointIndex > 0)
            {
                double pos = 2.0 / (double)SettingsViewModel.Steps;
                _currentPointIndex--;

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
