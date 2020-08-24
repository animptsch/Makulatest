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

using System.Media;

namespace MakulaTest
{
    /// <summary>
    /// Interaction logic for MacularDiagnosisControl.xaml
    /// </summary>
    public partial class MacularDiagnosisControl : UserControl
    {
        private Ellipse _ellipse;
        private const int LineNumber = 17;
        private const double _offset = 0.03;
        private Polygon _polygon;

        private DispatcherTimer _moveTimer;
        private DispatcherTimer _removeTimer;
        private Point _center;
        private Storyboard _sbTranslate;
        private MakulaSession _session;
        private Draw _draw;
        private MakulaDataSet _mds;

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
            _mds = new MakulaDataSet(FilePathSettings.Instance.CSVDataFilePath);
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

        //SystemSounds.Beep.Play();
        /*
        MediaPlayer mediaPlayer = new MediaPlayer();

        mediaPlayer.Open(new Uri(@"C:\Users\Jörg\source\repos\Makulatest\MakulaTest\SoundFiles\losgehts.mp3"));
        mediaPlayer.Play();
        */

            if (!SettingsViewModel.IsMeasureStarted)
            {
                MyCanvas.Background = SettingsViewModel.BackgroundBrush;                
  
                drawLines();
                drawCenterCircle();
                DetermineStartingPoints(MyRectangle.Width / 2 + 30, MyRectangle.Height / 2 + 30);

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

                Point start = getCurrentStartPoint();
                Point end   = getCurrentEndPoint();
                
                _ellipse = moveCircle(start, end);
            }
        }
        
 
        private void DetermineStartingPoints(double width, double height)
        {
          _session.StartingPoints.Clear();
          _currentPointIndex = 0;

          bool useNewestData = false;
      
          if (_mds.ReadNewestData(SettingsViewModel.IsBackwardChecked, SettingsViewModel.IsRightEyeChecked))
          { _mds.data.Points = ConvertDataToScreen(_mds.data.Points);
            useNewestData = true;
          }
        

          for (var i = 0; i < SettingsViewModel.Steps; i++)
          {
            Point pt = GetPointOnEllipse(i, width, height);

            if (useNewestData)
            {
              // convert data.Points --> StartingPoints
              var dp = _mds.data.Points.ToArray();
              if (_mds.data.Points.Count == SettingsViewModel.Steps)
              { if (distanceSquared(dp[i], _center) > 400.0) pt = dp[i]; // should be 20 pixels or more
              }
              else
              { for (int j = 0; j < dp.Length; j++)
                { var next = (j == dp.Length - 1 ? 0 : j + 1);

                  Point ip = FindIntersection(dp[j], dp[next], _center, pt);
                  if (IsInsideBox(ip, dp[j], dp[next]) && distanceSquared(pt, ip) < distanceSquared(pt, _center))
                  { if (distanceSquared(ip, _center) > 400.0) pt = ip; // should be 20 pixels or more
                    break;
                  }
                }
              }
            }

            _session.StartingPoints.Add(pt);
          }
        }

        private Point GetPointOnEllipse(double stepNo, double width, double height)
        {
          double anglePart = CorrectedAngle(1.0 * (SettingsViewModel.Steps - stepNo - 1) / SettingsViewModel.Steps + 0.25 + _offset);
          double angle = anglePart * 2 * Math.PI;
          //Console.WriteLine(i.ToString() + ". anglePart=" + anglePart.ToString());
          return new Point(Math.Sin(angle) * width + _center.X, Math.Cos(angle) * height + _center.Y);
        }

        private List<Point> ConvertDataToScreen(List<Point> d)
        {
          var screenCoord = d.ToArray();
          for (int i = 0; i < screenCoord.Length; i++)
          {
            screenCoord[i].X = screenCoord[i].X * MyRectangle.Width / 130.0 + _center.X;
            screenCoord[i].Y = screenCoord[i].Y * MyRectangle.Height / 130.0 + _center.Y;
          }
          return screenCoord.ToList<Point>();
        }

        private Point ExtendLine(Point p1, Point p2, double factor)
        {
          Vector v = (p2 - p1) * factor;
          
          var realLength = _mds.ConvertScreenToMillimeters(new Point(v.Length + _center.X, _center.Y), MyRectangle, CircleSize);

          if (realLength.X > 80.0) // max distance from center in mm
            return p1;

          return p2-v;
        }


        private double distanceSquared(Point p1, Point p2)
        {
          double dx = p1.X - p2.X;
          double dy = p1.Y - p2.Y;
          return dx * dx + dy * dy;
        }

        private bool IsInsideBox(Point p, Point boxCorner1, Point boxCorner2)
        {
          var left  = boxCorner1.X < boxCorner2.X ? boxCorner1 : boxCorner2;
          var right = boxCorner1.X < boxCorner2.X ? boxCorner2 : boxCorner1;
          var upper = boxCorner1.Y < boxCorner2.Y ? boxCorner1 : boxCorner2;
          var lower = boxCorner1.Y < boxCorner2.Y ? boxCorner2 : boxCorner1;

          if (p.X >= left.X && p.X <= right.X && p.Y >= upper.Y && p.Y <= lower.Y)
            return true;

          return false;
        }

        // https://rosettacode.org/wiki/Find_the_intersection_of_two_lines
        static Point FindIntersection(Point s1, Point e1, Point s2, Point e2)
        {
          double a1 = e1.Y - s1.Y;
          double b1 = s1.X - e1.X;
          double c1 = a1 * s1.X + b1 * s1.Y;

          double a2 = e2.Y - s2.Y;
          double b2 = s2.X - e2.X;
          double c2 = a2 * s2.X + b2 * s2.Y;

          double delta = a1 * b2 - a2 * b1;
          //If lines are parallel, the result will be (NaN, NaN).
          return delta == 0 ? new Point(double.NaN, double.NaN)
                 : new Point((b2* c1 - b1* c2) / delta, (a1* c2 - a2* c1) / delta);
        }

        
        private void resetTimer()
        {
            Point start = getCurrentStartPoint();
            Point end   = getCurrentEndPoint();

            if (_moveTimer != null && _removeTimer != null && SettingsViewModel.IsMeasureStarted)
            {
                _moveTimer.Stop();
                _removeTimer.Stop();
                _removeTimer.Start();
                _moveTimer.Start();

                _ellipse = moveCircle(start, end);
            }
        }


        private void _timer_Tick(object sender, EventArgs e)
        {
            cancelMovement();
        }

        private void cancelMovement()
        {
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


        private Ellipse moveCircle(Point begin,Point end)
        {
            int durationInSeconds;
          
            durationInSeconds = SettingsViewModel.SelectedDuration;

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
            _polygon.Points = new PointCollection(_session.Points);
            _polygon.Stroke = SettingsViewModel.PolygonBrush;            
         
            MyCanvas.Children.Add(_polygon);

            _mds.SaveData(_session.Points,                
                         SettingsViewModel.IsBackwardChecked,  // direction
                         SettingsViewModel.IsRightEyeChecked,  // rightEye
                         CircleSize,
                         MyRectangle);

            Parent.btnStartMacularDiagnosis.IsEnabled = true;
            _session.Points.Clear();
        }


            

        private Point getCurrentStartPoint()
        { if (_currentPointIndex >= SettingsViewModel.Steps)
          { StopDiagnosis();
            return _session.StartingPoints[0];
          }

          if (_currentPointIndex < 0)
          { Console.WriteLine("ERROR: _currentPointIndex=" + _currentPointIndex.ToString());
            _currentPointIndex = 0;
          }

          Point pt = _center;
          double amount = 0.10; // 10% (not 5%) - do we need a new property?

          if (SettingsViewModel.IsBackwardChecked)
          { // from inside to outside (backward)
            //_session.StartingPoints[_currentPointIndex] = ExtendLine(_session.StartingPoints[_currentPointIndex], _center, 1.0-amount);
            _session.StartingPoints[_currentPointIndex] = _center; // from inside to outside --> always center point!
          }
          else
          { // from outside to midpoint (forward)
            _session.StartingPoints[_currentPointIndex] = ExtendLine(_session.StartingPoints[_currentPointIndex], _center, 1.0+amount);
            
          }

          return _session.StartingPoints[_currentPointIndex];
        }

        private Point getCurrentEndPoint()
        {
          Point pt = _center;

          if (_currentPointIndex >= SettingsViewModel.Steps)
          { return pt;
          }


          if (_currentPointIndex < 0)
          { Console.WriteLine("ERROR: _currentPointIndex=" + _currentPointIndex.ToString());
            _currentPointIndex = 0;
          }

          if (SettingsViewModel.IsBackwardChecked)
            pt = GetPointOnEllipse(_currentPointIndex, MyRectangle.Width / 2 + 30, MyRectangle.Height / 2 + 30);

          return pt;
        }

        private void drawLines(bool isDrawAmselGrid = true)
        {
            double width = MyRectangle.Width;
            double height = MyRectangle.Height;
            double x = MyRectangle.Margin.Left;
            double y = MyRectangle.Margin.Top;

            _center.X = width / 2.0 + x;
            _center.Y = height / 2.0 + y;

            clearCanvas();
           
            if (isDrawAmselGrid)
            {   double thickness = 3;

                //draw horizontal Line
                _draw.DrawLine(x, _center.Y, width + x, _center.Y, thickness, SettingsViewModel.LinesBrush);

                //draw vertical Line
                _draw.DrawLine(_center.X, y, _center.X, height + y, thickness, SettingsViewModel.LinesBrush);
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
            _draw.DrawCircle(_center.X, _center.Y, CircleSize, SettingsViewModel.LinesBrush);
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
                    if (_currentPointIndex > 0)
                    { _currentPointIndex--;
                      if (_session.Points.Count > 0)
                        _session.Points.RemoveAt(_session.Points.Count - 1);
                    }

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
