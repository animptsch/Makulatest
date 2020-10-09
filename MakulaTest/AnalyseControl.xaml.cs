using MakulaTest.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Threading;
using System.Printing;
using System.Windows.Documents;
using System.Linq;
using System.Diagnostics.Eventing.Reader;
using MakulaTest.Properties;
using System.Windows.Media.Imaging;

namespace MakulaTest
{





    /// <summary>
    /// Interaction logic for AnalyseControl.xaml
    /// </summary>
    public partial class AnalyseControl : UserControl
    {
        private Stopwatch stopwatch;
        private DispatcherTimer _moveTimer;
        private Rectangle testRect;
        private int maxSequenceId;
        static private Size _windowSize;
        private bool timeMeasure = false;
        private bool _showGrid = true;
        //private Rectangle _rect;
        private Draw _draw;
        private MakulaDataSet _mds;
        private double _radius;

        private bool _rightEyeFilter;        

        public AnalyseControl()
        {
            stopwatch = new Stopwatch();
            timeMeasure = false; // stop watch control 
            _showGrid = true; // toggle grid display
            InitializeComponent();
        }

        public void Start(string path)
        {
            _draw = new Draw(MyCanvas);
            this.SizeChanged += MyCanvas_SizeChanged;
            _mds = new MakulaDataSet(path);

            //_rect = MyRectangle;
/*********************************
                  var steps = 17;
                  var radius = 100;
                  Vector center = new Vector(radius, radius);

                  Console.WriteLine("Punkt_Nr     X        Y");
                  Console.WriteLine("--------  -------  -------");
                  for (int entry_id = 0; entry_id < steps; entry_id++)
                  { var start = GetStartingPoint(entry_id, steps, center, radius);
                    Console.WriteLine(String.Format("{0,8:D} {1,8:N2} {2,8:N2}", entry_id, Math.Round(start.X, 2), Math.Round(start.Y, 2)));
                  }
***************************************/

            _mds.ReadSequences();
            _mds.ReadData();

            if (_windowSize.Width != 0.0 && _windowSize.Height != 0.0)
              RefreshScreen();

            //TestOffsets();

            /*
            _moveTimer = new DispatcherTimer();
            _moveTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            _moveTimer.Tick += new EventHandler(_timer_Tick);
            _moveTimer.Start();
            */

            //GenerateTestData();            
        }

       

        //https://de.cleanpng.com/png-z809ef/download-png.html


        private void CbRightEye_IsChecked(object sender, RoutedEventArgs e)
        {
            if (CbLeftEye != null)
                _rightEyeFilter = (CbLeftEye.IsChecked == true);
            else
                _rightEyeFilter = false;
        }

        private void CbLeftEye_IsChecked(object sender, RoutedEventArgs e)
        {
            if (CbLeftEye != null)
              _rightEyeFilter = (CbLeftEye.IsChecked == true);
            else
              _rightEyeFilter = false;
        }


        private void CbForward_IsChecked(object sender, RoutedEventArgs e)
        {
            if (CbLeftEye != null)
              _rightEyeFilter = (CbLeftEye.IsChecked == true);
            else
              _rightEyeFilter = false;
        }

        private void CbBackward_IsChecked(object sender, RoutedEventArgs e)
        {
            if (CbLeftEye != null)
              _rightEyeFilter = (CbLeftEye.IsChecked == true);
            else
              _rightEyeFilter = false;
        }

        // ***************************************************************
        // BtnDaten
        // ***************************************************************
        private void BtnDaten_Click(object sender, RoutedEventArgs e)
        {   _mds.DeleteRecord(_mds.data.actualSequence);
            _mds.ReadSequences();
            GoBackInTime();
        }
        private void BtnDaten_Enter(object sender, RoutedEventArgs e)
        {
            BtnDatenImage.Source = (ImageSource) Application.Current.Resources["Trash2"];
        }

        private void BtnDaten_Leave(object sender, RoutedEventArgs e)
        {
            BtnDatenImage.Source = (ImageSource) Application.Current.Resources["Trash"];
        }

        // ***************************************************************
        // BtnDayBefore
        // ***************************************************************
        private void BtnDayBefore_Click(object sender, RoutedEventArgs e)
        {
            GoBackInTime();
        }

        private void BtnDayBefore_Enter(object sender, RoutedEventArgs e)
        {
            BtnDayBeforeImage.Source = (ImageSource) Application.Current.Resources["Backward2"];
        }

        private void BtnDayBefore_Leave(object sender, RoutedEventArgs e)
        {
            BtnDayBeforeImage.Source = (ImageSource) Application.Current.Resources["Backward"];
        }

        // ***************************************************************
        // BtnNextDay
        // ***************************************************************
        private void BtnNextDay_Click(object sender, RoutedEventArgs e)
        {
            GoForwardInTime();
        }

        private void BtnNextDay_Enter(object sender, RoutedEventArgs e)
        {
            BtnNextDayImage.Source = (ImageSource) Application.Current.Resources["Forward2"];
        }

        private void BtnNextDay_Leave(object sender, RoutedEventArgs e)
        {
            BtnNextDayImage.Source = (ImageSource) Application.Current.Resources["Forward"];
        }

        // ***************************************************************
        // BtnPrint
        // ***************************************************************
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintFlowDocument();
        }
        private void BtnPrint_Enter(object sender, RoutedEventArgs e)
        {
            BtnPrintImage.Source = (ImageSource) Application.Current.Resources["Print2"];
        }

        private void BtnPrint_Leave(object sender, RoutedEventArgs e)
        {
            BtnPrintImage.Source = (ImageSource) Application.Current.Resources["Print"];
        }




        private void CanvasPrint()
        {
            var originalCanvasWidth = MyCanvas.Width;
            var originalCanvasHeight = MyCanvas.Height;

            PrintDialog prnt = new PrintDialog();

            if (prnt.ShowDialog() == true)
            {
                MyCanvas.Width = MyCanvas.Width * 0.7;
                MyCanvas.Height = MyCanvas.Height * 0.7;

                prnt.PrintVisual(MyCanvas, "Printing Canvas");

                MyCanvas.Width = originalCanvasWidth;
                MyCanvas.Height = originalCanvasHeight;
            }
            // this.Close();
        }


        private void PrintFlowDocument()
        {
            PrintDialog prnt = new PrintDialog();

            //PrintDialog object's UseEXDialog to true.
            if (prnt.ShowDialog() != true) return;

            // Create a FlowDocument dynamically.

            //FlowDocument doc = new FlowDocument(new Paragraph(new Run("Some text goes here")));
            FlowDocument doc = CreateFlowDocument();
            doc.Name = "FlowDoc";

            // Create IDocumentPaginatorSource from FlowDocument  
            IDocumentPaginatorSource idpSource = doc;

            // Call PrintDocument method to send document to printer  
            prnt.PrintDocument(idpSource.DocumentPaginator, "Hello WPF Printing.");
        }

        /// <summary>  
        /// This method creates a dynamic FlowDocument. You can add anything to this  
        /// FlowDocument that you would like to send to the printer  
        /// </summary>  
        /// <returns></returns>  
        private FlowDocument CreateFlowDocument()
        {
            // Create a FlowDocument  
            FlowDocument doc = new FlowDocument();

            doc.ColumnWidth = ConvertPrinterPixel(190.0);

            // Create a Section  
            Section sec = new Section();
            // Create first Paragraph  
            Paragraph p1 = new Paragraph();
            // Create and add a new Bold, Italic and Underline  
            Bold bld = new Bold();

            /*
            bld.Inlines.Add(new Run("First Paragraph"));
            Italic italicBld = new Italic();
            italicBld.Inlines.Add(bld);
            Underline underlineItalicBld = new Underline();
            underlineItalicBld.Inlines.Add(italicBld);
            // Add Bold, Italic, Underline to Paragraph  
            p1.Inlines.Add(underlineItalicBld);
            // Add Paragraph to Section  
            sec.Blocks.Add(p1);
            // Add Section to FlowDocument  
            doc.Blocks.Add(sec);

            Paragraph myParagraph = new Paragraph();

            // Add some Bold text to the paragraph
            myParagraph.Inlines.Add(new Bold(new Run("Some bold text in the paragraph.")));

            // Add some plain text to the paragraph
            myParagraph.Inlines.Add(new Run(" Some text that is not bold."));

            // Create a List and populate with three list items.
            List myList = new List();

            // First create paragraphs to go into the list item.
            Paragraph paragraphListItem1 = new Paragraph(new Run("ListItem 1"));
            Paragraph paragraphListItem2 = new Paragraph(new Run("ListItem 2"));
            Paragraph paragraphListItem3 = new Paragraph(new Run("ListItem 3"));

            // Add ListItems with paragraphs in them.
            myList.ListItems.Add(new ListItem(paragraphListItem1));
            myList.ListItems.Add(new ListItem(paragraphListItem2));
            myList.ListItems.Add(new ListItem(paragraphListItem3));

            // Create a FlowDocument with the paragraph and list.
            doc.Blocks.Add(myParagraph);
            doc.Blocks.Add(myList);
*/
            Canvas printerCanvas = new Canvas();
            Draw printer = new Draw(printerCanvas);

            var cm = ConvertPrinterPixel(10.0);
            var xPos = ConvertPrinterPixel(36.75+15.0);
            var lineSize = ConvertPrinterPixel(10.0);

            var darkYellow =  (Color)Application.Current.Resources["Dark"];
            var lightYellow = Color.FromArgb(255, 254, 244, 229);
            printer.DrawRectangle(cm * 1.5, cm * 2.0, cm * 18.0, cm * 25.0, darkYellow,  0, cm, cm);
            printer.DrawRectangle(cm * 2.5, cm * 3.0, cm * 16.0, cm * 23.0, lightYellow, 0, cm*16.0/18.0, cm*23.0/25.0);

            printer.DrawStringAtPos("Hubert Nimptsch",  xPos, lineSize * 3.5, 36, TextAlignment.Left);
            printer.DrawStringAtPos("Angerweg 2",  xPos, lineSize * 4.5, 16, TextAlignment.Left);
            printer.DrawStringAtPos("31028 Gronau/Leine", xPos, lineSize * 5.0, 22, TextAlignment.Left);
            printer.DrawStringAtPos("Mobil: 01575 1086320", xPos, lineSize * 6.0, 16, TextAlignment.Left);

            var originalRadius = _radius;
            _radius = 201.135;

            DrawTestfield(xPos, lineSize * 7.0, printer);
            DrawTestPolygon(xPos, lineSize * 7.0, printer);
            DrawMidPoint(xPos, lineSize * 7.0, printer);

            DrawGrid(xPos, lineSize * 7.0, printer);

            _radius = originalRadius;

            var a = CalculatePolygonArea();
            var pct1 = a * 100.0 / (Math.PI * 65 * 65);
            var pct2 = 100 - pct1;

            printer.DrawRectangle(xPos, lineSize * 18.0,  cm*2.0, lineSize*0.75, darkYellow,  0, 5.0, 5.0); 
            printer.DrawStringAtPos(string.Format("{0:N2}%", pct2), xPos+cm*0.3, lineSize * 18.1, 16, TextAlignment.Left, Colors.Black);
            printer.DrawStringAtPos("Sehbereich", xPos+cm*2.3, lineSize * 18.1, 16);

            printer.DrawRectangle(xPos+cm*5.7, lineSize * 18.0, cm*2.0, lineSize*0.75, Colors.DarkGray,  0, 5.0, 5.0); 
            printer.DrawStringAtPos(string.Format("{0:N2}%", pct1), xPos+cm*6.0, lineSize * 18.1, 16, TextAlignment.Left, Colors.Black);
            printer.DrawStringAtPos("Ausfallbereich",  xPos+cm*8.0, lineSize * 18.1, 16);

            printer.DrawStringAtPos("minimaler Abstand: "+ string.Format("{0:N2} mm", _mds.minDistance),  xPos, lineSize * 19.0, 16);

            printer.DrawStringAtPos(_mds.data.actualDate.ToString(), xPos, lineSize * 20.0, 16);

            if (_mds.backward)
              printer.DrawStringAtPos("von Innen nach Außen", xPos, lineSize * 20.5, 16);
            else
              printer.DrawStringAtPos("von Außen nach Innen", xPos, lineSize * 20.5, 16);

            if (_mds.rightEye)
              printer.DrawStringAtPos("rechtes Auge", xPos, lineSize * 21.0, 16);
            else
              printer.DrawStringAtPos("linkes Auge", xPos, lineSize * 21.5, 16);

            var buiContainer = new BlockUIContainer(printerCanvas);
            doc.Blocks.Add(buiContainer);

            return doc;
        }

        private double ConvertPrinterPixel(double mm)  // millimeter to pixel
        {
          return mm * 96 / 2.54 / 10 ;  // 96 dots per inch (2.54 cm)
        }

        private void RefreshScreen_xxx()
        {
            MyCanvas.Children.Clear();
            DrawTestfield(40, 40, _draw);
        }


        private void RefreshScreen()
        {
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                TimeMeasureStart();

                //Console.WriteLine("*********** Analyse.MyCanvas.Width (new): " + _windowSize.Width.ToString());
                //Console.WriteLine("*********** Analyse.MyCanvas.Height (new): " + _windowSize.Height.ToString());
                _radius = GetRadius(520.0, 99.0);

                //Console.WriteLine("MyCanvas.ActualHeight="+ MyCanvas.Width.ToString());

                MyCanvas.Children.Clear();

                /*
                foreach (var point in data.Points)
                {
                  Console.WriteLine(point.ToString());
                }
                */

                if (IsFatalError)
                {
                    return;
                }

                DrawTestfield(40, 40, _draw);
                DrawTestPolygon(40, 40, _draw);
                DrawMidPoint(40, 40, _draw);
                if (_showGrid) DrawGrid(40.0, 40.0, _draw);

                var a = CalculatePolygonArea();
                var pct1 = a * 100.0 / (Math.PI * 65 * 65);
                var pct2 = 100 - pct1;

                DrawLegend(80, 20, "Sehbereich:", pct2, "Ausfallbereich:", pct1);
                txtViewArea.Text = string.Format("{0:N2}%", pct2);
                txtDefaultArea.Text = string.Format("{0:N2}%", pct1);
                txtMinDistance.Text = string.Format("minimaler Abstand: {0:N2} mm", _mds.minDistance);

                txtDateTime.Text = _mds.data.actualDate.ToString();
                txtDirection.Text = _mds.backward ? "von Innen nach Außen" : "von Außen nach Innen";
                txtWhichEye.Text = _mds.rightEye ? "rechtes Auge" : "linkes Auge";


#if DEBUG
                txtDebugInformation.Text = "(" + _mds.data.record_no + ", " + _mds.data.deleted + ")";
#else
        txtDebugInformation.Visibility = txtDebugInformation.Visibility = Visibility.Collapsed;
#endif





                //Console.WriteLine("Die Fläche des Polygons beträgt: " + a.ToString());
                //DrawXAxis(20, 400, cursor_y + 150, 50);
                //DrawCheckMark(450, 100);

                TimeMeasureStop("Dialog Ende");
            }
        }


        private void DrawLegend(double xOffset, double yOffset, string text1, double pct2, string text2, double pct1)
        {
        
            return;
            Vector center = new Vector(_radius, _radius);

            var x_pos = _radius * 2.0 + xOffset;
            var y_pos1 = center.Y + 30.0 - yOffset;
            var y_pos2 = center.Y + 30.0 + yOffset;

            _draw.DrawStringAtPos("Hubert Nimptsch", x_pos - 2, 40, 36, TextAlignment.Left);
            _draw.DrawStringAtPos("Angerweg 2", x_pos - 2, 90, 16, TextAlignment.Left);
            _draw.DrawStringAtPos("31028 Gronau/Leine", x_pos - 2, 120, 22, TextAlignment.Left);
            _draw.DrawStringAtPos("Mobil: 01575 1086320", x_pos - 2, 150, 16, TextAlignment.Left);

            _draw.DrawRectangle(x_pos, y_pos1, 40, 20, Colors.LightGreen);
            _draw.DrawStringAtPos(text1, x_pos + 50, y_pos1, 16);
            _draw.DrawStringAtPos(string.Format("{0:N2}%", pct2), x_pos + 160, y_pos1, 16);

            _draw.DrawRectangle(x_pos, y_pos2, 40, 20, Colors.LightPink);
            _draw.DrawStringAtPos(text2, x_pos + 50, y_pos2, 16);
            _draw.DrawStringAtPos(string.Format("{0:N2}%", pct1), x_pos + 160, y_pos2, 16);

            _draw.DrawStringAtPos("minimaler Abstand:",          x_pos, y_pos2+30, 16);
            _draw.DrawStringAtPos(string.Format("{0:N2} mm", _mds.minDistance), x_pos + 160, y_pos2+30, 16);

            var textBelowPicture = _mds.data.actualDate.ToString();
            if (_mds.backward)
                textBelowPicture += " von Innen nach Außen";
            else
                textBelowPicture += " von Außen nach Innen";

            if (_mds.rightEye)
                textBelowPicture += "; rechtes Auge";
            else
                textBelowPicture += "; linkes Auge";

      #if DEBUG
        textBelowPicture += " ("+ _mds.data.record_no + ", "+ _mds.data.deleted + ")";
      #endif

            _draw.DrawStringAtPos(textBelowPicture, Convert.ToInt32(_radius) + 40, Convert.ToInt32(_radius) * 2 + 60, 16, TextAlignment.Center);
            //_draw.DrawBlackLine(0, Convert.ToInt32(center.Y)+40,1000, Convert.ToInt32(center.Y)+40, 2);
        }



        private void MyCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            Vector v;
            if (e.NewSize.Width == 0.0 && e.NewSize.Height == 0.0)
                v = new Vector(e.PreviousSize.Width, e.PreviousSize.Height);
            else
                v = new Vector(e.NewSize.Width, e.NewSize.Height);

            //v -= new Vector(BtnBack.ActualWidth, BtnBack.ActualHeight);

            _windowSize = new Size(v.X, v.Y);

            RefreshScreen();

        }

        private void MyCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            int i = 44;
        }


        public void GoBackInTime()
        {
            if (_mds.data.actualSequence > 0)
            { _mds.data.actualSequence--;
              _mds.ReadData();
              RefreshScreen();
            }

        }

        public void GoForwardInTime()
        {
          if (_mds.data.actualSequence < _mds.GetMaxSequenceId())
          { _mds.data.actualSequence++;
            _mds.ReadData();
            RefreshScreen();
          }
        }


        private void _timer_Tick(object sender, EventArgs e)
        {
      double x=0;
      double y =0;

            if (testRect != null)
            {
                x = Canvas.GetLeft(testRect);
                y = Canvas.GetTop(testRect);
                MyCanvas.Children.Remove(testRect);
            }

            if (x > 500) _moveTimer.Stop();

            x++;
            y++;

            testRect = _draw.DrawRectangle(x, y, 40, 20, (Color)this.Resources["Dark"] );
            //Console.WriteLine("Timer Tick Event "+x.ToString()+","+y.ToString());

        }


        //https://de.wikipedia.org/wiki/Gaußsche_Trapezformel
        public double CalculatePolygonArea()
        {
            if (_mds.data.Points == null) return 0.0;

            int n = _mds.data.Points.Count;
            if (n < 3) return 0.0; // a polygon must have at least 3 points
            double a = 0.0;
            for (int i = 0; i < n; i++)
            {
                a += (_mds.data.Points[i].Y + _mds.data.Points[(i + 1) % n].Y) * (_mds.data.Points[i].X - _mds.data.Points[(i + 1) % n].X);
            }

            return Math.Abs(a / 2.0);
        }



        private void TimeMeasureStart()
        {
            stopwatch.Reset();
            stopwatch.Start();

            long seed = Environment.TickCount;  // Prevents the JIT Compiler 
                                                // from optimizing Fkt calls away
        }


        private void TimeMeasureStop(string what)
        {
            stopwatch.Stop();
            if (timeMeasure)
                Console.WriteLine(what + " Ticks: " + stopwatch.ElapsedTicks + " Real time: " + stopwatch.ElapsedMilliseconds + " ms");
        }

        private double GetRadius(double xOffset, double yOffset)
        {   double radius;
            double w = _windowSize.Width - xOffset;
            double h = _windowSize.Height- yOffset;

            if (w  > h)
                radius = h / 2.0;
            else
                radius = w / 2.0;

            //Console.WriteLine("Radius=" + radius.ToString());

            return radius;
        }

        private void DrawMidPoint(double x, double y, Draw draw)
        {
            Vector center = new Vector(_radius, _radius);

            draw.DrawEllipse(x + center.X, y + center.Y, 15.0, 15.0, Colors.Black, Colors.Black);
        }


        private void DrawTestfield(double x, double y, Draw draw)
        {
            if (IsFatalError)
            {
                return;
            }
            if (_mds != null)
            {
                int steps = _mds.data.Points.Count;
                double[,] Points = new double[steps, 2];

                var minWinExtend = Math.Min(_windowSize.Width, _windowSize.Height);

                Vector center = new Vector(_radius, _radius);

                int count = 0;
                for (var i = 0; i < steps; i++)
                {
                    Vector start = GetStartingPoint(i, steps, center, _radius);

                    Points[count, 0] = start.X;
                    Points[count, 1] = start.Y;
                    //Console.WriteLine(count.ToString() + ". TestPoints=" + Points[count, 0].ToString() + "," + Points[count, 1].ToString());
                    count++;
                }
                
                var obj = (Color)Application.Current.Resources["Dark"];
                draw.DrawPolygon(Points, x, y, Colors.Black, obj, 2, false);
            }

        }


        private void DrawTestPolygon(double x, double y, Draw draw)
        {
            if (IsFatalError)
            {
                return;
            }

            if (_mds != null)
            {
                //double scaleFactor = radius * 2.0 / data.Points[0].X;
                double scaleFactor = _radius / 65;

                int steps = _mds.data.Points.Count;
                double[,] Points = new double[steps, 2];

                int count = 0;
                foreach (var point in _mds.data.Points)
                {
                    Points[count, 0] = point.X * scaleFactor + _radius;
                    Points[count, 1] = point.Y * scaleFactor + _radius;
                    //Console.WriteLine(count.ToString() + ". Points=" + Points[count, 0].ToString() +"," + Points[count, 1].ToString());
                    count++;
                }

                draw.DrawPolygon(Points, x, y, Colors.Black, Colors.DarkGray, 2, false);
            }
            
        }


        //protected override Size ArrangeOverride(Size arrangeBounds)
        //{
        //    MyCanvas.Measure(arrangeBounds);

        //    MyCanvas.Arrange(new Rect(arrangeBounds));

        //    _windowSize = MyCanvas.RenderSize;
            
        //    //RefreshScreen();
            

        //    return base.ArrangeOverride(arrangeBounds);
        //}

        public static bool IsFatalError { get; set; }


        private void DrawGrid(double x, double y, Draw draw)
        {
            int LineNumber = 20;

            double width = _radius;
            double height = _radius;

            double centerIndex = Math.Floor(LineNumber / 2.0);

      double deltaHorz = width*2.0 / LineNumber;
      double deltaVert = height*2.0 / LineNumber;

            double xLoop = x + width - centerIndex * deltaHorz;
      double yLoop = y + height -centerIndex * deltaVert;

            double lineHeight = yLoop, lineWidth = xLoop;

            //draw frame
            draw.DrawRectangle(x, y, width * 2, height * 2, Colors.Transparent, 1, 0.0, 0.0);

            //draw Lines
            for (int i = 0; i < LineNumber; i++)
            {
                double thickness = 1.0;
                if (centerIndex == i) thickness = 3.0;

                draw.DrawBlackLine(x, yLoop + i * deltaVert, x + width * 2, yLoop + i * deltaVert, thickness);
                draw.DrawBlackLine(xLoop + i * deltaHorz, y, xLoop + i * deltaHorz, y + height * 2, thickness);
            }

        }

/*
        private void GenerateTestData()
        {
            var rnd = new Random();

            GenerateTestDataFirstDay(1, 70.0,rnd);

            data.actualSequence = 1;
            ReadData();

            FileStream fs = new FileStream(_path, FileMode.Append);

            GenerateTestDataNextDay(-5.0, rnd, fs);
            GenerateTestDataNextDay(-2.0, rnd, fs);
            GenerateTestDataNextDay(-3.0, rnd, fs);
            GenerateTestDataNextDay( 2.0, rnd, fs);
            GenerateTestDataNextDay( 3.0, rnd, fs);
            GenerateTestDataNextDay(-2.0, rnd, fs);
            GenerateTestDataNextDay(-4.0, rnd, fs);

            fs.Close();
        }


        private void GenerateTestDataNextDay(double change_factor, Random rnd, FileStream fs)
        {
            data.actualSequence++;
            var day = DateTime.Now.AddDays(data.actualSequence);

            var entry_id = 0;

            double radius = 150;
            int steps = 20;
            Vector start = new Vector();
            Vector center = new Vector(radius, radius);

            foreach (var point in data.Points)
            {
                //Console.WriteLine(point.ToString());

                start.X = point.X;
                start.Y = point.Y;

                var center_vector = start-center;
                //Console.WriteLine("Center="+center_vector.ToString());

                var pct = center_vector.Length* 100.0/ radius;
                //Console.WriteLine("pct1=" + pct.ToString());

                double fluctuation = rnd.NextDouble();

                pct += (change_factor * fluctuation);
                //Console.WriteLine("fluctuation=" + fluctuation.ToString());

                //Console.WriteLine("pct2=" + pct.ToString());

                pct = Math.Max(Math.Min(pct, 100.0),0.0);
                //Console.WriteLine("pct3=" + pct.ToString());

                var new_l = radius * pct / 100.0;
                //Console.WriteLine("new_l=" + new_l.ToString());

                center_vector.Normalize();
                //Console.WriteLine("center_vector=" + center_vector.ToString());

                center_vector *= new_l;
                //Console.WriteLine("center_vector=" + center_vector.ToString());

                center_vector += center;

                //Console.WriteLine("center_vector=" + center_vector.ToString());

                WriteCSV(fs, data.actualSequence, entry_id, day, center_vector);
                entry_id++;
                // break;
            }

            
                  //for (var i = 0; i < steps; i++)
                  //{
                  //  double angle = i * 2*Math.PI / steps;
                  //  start.X = data.Points[i].X;
                  //  start.Y = data.Points[i].Y;

                  //  center_vector = center - start;

                  //  double pos = rnd.NextDouble();
                  //  center_vector *= (pos - 0.5);

                  //  center_vector += center;

                  //  WriteCSV(fs, sequence_id, entry_id, day, center_vector);

                  //  entry_id++;

                  //}
                

        }
        */







        private Vector GetStartingPoint(int i, int n, Vector center, double radius)
        {
            double angle = i * 2 * Math.PI / n;
            return new Vector(Math.Sin(angle) * radius + center.X, Math.Cos(angle) * radius + center.Y);
        }


    /*
        private void GenerateTestDataFirstDay(int sequence_id, double sight_pct, Random rnd)
        {
            File.Delete(_path);
            FileStream fs = File.OpenWrite(_path);

            var day = DateTime.Now.AddDays(sequence_id);


            double radius = 150;
            int steps = 20;
            Vector center = new Vector()
            {
                X = radius,
                Y = radius
            };
            Vector center_vector = new Vector();

            for (int entry_id = 0; entry_id < steps; entry_id++)
            {
                Vector start = GetStartingPoint(entry_id, steps, center, radius);

                center_vector = center - start;

                double pos = rnd.NextDouble();
                center_vector *= (pos * sight_pct / 100.0) + (100.0 - sight_pct) / 100.0;

                center_vector += center;

                WriteCSV(fs, sequence_id, entry_id, day, center_vector);
            }

            fs.Close();


        }
*/

        private void TestOffsets()
        {

            for (int schritte = 2; schritte < 21; schritte++)
            {
                double max_d = -1.0;
                double best_q = -1.0;

                for (double q = 0.0; q < 0.2; q += 0.000001)
                //double q = 1.0/15.0/2.0;
                {
                    double min_d = 20000.0;

                    for (int i = 0; i < schritte; i++)
                    {
                        double value = q + 1.0 * i / schritte;

                        if (Math.Abs(value) < min_d) min_d = Math.Abs(value);
                        if (Math.Abs(value - 0.25) < min_d) min_d = Math.Abs(value - 0.25);
                        if (Math.Abs(value - 0.50) < min_d) min_d = Math.Abs(value - 0.50);
                        if (Math.Abs(value - 0.75) < min_d) min_d = Math.Abs(value - 0.75);
                        if (Math.Abs(value - 1.00) < min_d) min_d = Math.Abs(value - 1.00);

                        //Console.WriteLine(i.ToString() + ".  v=" + Math.Round(value, 2).ToString() + " min_d=" + Math.Round(min_d, 2).ToString());

                    }

                    if (min_d > max_d)
                    {
                        max_d = min_d;
                        best_q = q;
                    }

                    //Console.WriteLine("q="+q.ToString()+" min_d=" + Math.Round(min_d, 2).ToString());

                }
                //Console.WriteLine("best_q=" + best_q.ToString() + " max_d=" + max_d.ToString());
                best_q *= 360.0;
                max_d *= 360.0;
                Console.WriteLine(schritte.ToString() + ". best_q=" + best_q.ToString() + " max_d=" + max_d.ToString());

            }
        }


        /*
            Stopwatch stopwatch = new Stopwatch();

            long seed = Environment.TickCount;  // Prevents the JIT Compiler 
                                                // from optimizing Fkt calls away

            stopwatch.Reset();
            stopwatch.Start();



              stopwatch.Stop();
              Console.WriteLine(count.ToString() + " lines written to file. Ticks: " + stopwatch.ElapsedTicks + " Real time: " + stopwatch.ElapsedMilliseconds + " ms");
              //   40000 lines read from file. Ticks: 299458 Real time:    29 ms
              //  400000 lines read from file. Ticks: 2624901 Real time:  262 ms
              // 4000000 lines read from file.Ticks: 26967563 Real time: 2696 ms

          */




    }

}