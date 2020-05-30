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



namespace MakulaTest
{


  public class MakulaDataSetInternal
  {
    public List<Point> Points;
    public int actualSequenceId;
    public DateTime actualDate;
  }


  /// <summary>
  /// Interaction logic for AnalyseControl.xaml
  /// </summary>
  public partial class AnalyseControl : UserControl
  {
    private int cursor_x;
    private int cursor_y;
    private Stopwatch stopwatch;
    private DispatcherTimer _moveTimer;
    private Rectangle testRect;
    private int maxSequenceId;
    private string path;
    private MakulaDataSetInternal data;
    private Size windowSize;
    private bool timeMeasure = false; // stop watch control 

    public MainWindow Parent { get; set; }

        public AnalyseControl()
    {
      stopwatch = new Stopwatch();
      data = new MakulaDataSetInternal();

      InitializeComponent();
    }


 
    public void Start()
    {
      var steps = 12;
      var radius = 100;
      Vector center = new Vector(radius, radius);

      Console.WriteLine("Punkt_Nr     X        Y");
      Console.WriteLine("--------  -------  -------");
      for (int entry_id = 0; entry_id < steps; entry_id++)
      { var start = GetStartingPoint(entry_id, steps, center, radius);
        Console.WriteLine(String.Format("{0,8:D} {1,8:N2} {2,8:N2}", entry_id, Math.Round(start.X, 2), Math.Round(start.Y, 2)));
      }

      path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MakulaTestData.csv");
  
      maxSequenceId = GetMaxSequenceId();
      data.actualSequenceId = maxSequenceId;

      ReadData();

      /*
      _moveTimer = new DispatcherTimer();
      _moveTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
      _moveTimer.Tick += new EventHandler(_timer_Tick);
      _moveTimer.Start();
      */

      //GenerateTestData();

    }

    private int GetMaxSequenceId()
    {
      var sequence_id_max = 0;
    
      StreamReader fs = new StreamReader(path);
      string csv_line;
      char[] charSeparators = new char[] { ';' };

      Point p = new Point() { X = 0.0, Y = 0.0 };

      while ((csv_line = fs.ReadLine()) != null)
      {
        var elements = csv_line.Split(charSeparators, StringSplitOptions.None);

        try
        {
          var sequence_id = int.Parse(elements[0]);
          sequence_id_max = Math.Max(sequence_id_max, sequence_id);
        }
        catch (FormatException e)
        {
        }
      }
      fs.Close();

      //Console.WriteLine("sequence_id_max=" + sequence_id_max.ToString());
      return sequence_id_max;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
      //Console.WriteLine("btnBack_Click");
      //MainWindow.AnalyseStop();
    }



    private void RefreshScreen_xxx()
    {
      MyCanvas.Children.Clear();
      DrawTestfield(40, 40);
    }


    private void RefreshScreen()
    {
      TimeMeasureStart();


      //Console.WriteLine("*********** Analyse.MyCanvas.Width (new): " + windowSize.Width.ToString());
      //Console.WriteLine("*********** Analyse.MyCanvas.Height (new): " + windowSize.Height.ToString());

      cursor_x = 30; // used for text lines
      cursor_y = 50;

      //Console.WriteLine("MyCanvas.ActualHeight="+ MyCanvas.Width.ToString());

      MyCanvas.Children.Clear();

      //DrawString("Die Analyse ist abgeschlossen.", 24);
      //DrawHorizontalLine(320);
      //DrawString("", 24);
      //DrawString("Es ist alles in Ordnung.", 36);


      /*
      foreach (var point in data.Points)
      {
        Console.WriteLine(point.ToString());
      }
      */

   
      DrawTestfield(40, 40);
      DrawTestPolygon(40, 40);
      DrawMidPoint(40, 40);

      var a = CalculatePolygonArea();
      var pct1 = Math.Round(a * 100 / 69528.8237343631, 2);
      var pct2 = 100 - pct1;

      DrawLegend(80, 20, "Sehfeld:", pct2, "außerhalb:", pct1);

      cursor_y = 420;

      //Console.WriteLine("Die Fläche des Polygons beträgt: " + a.ToString());

    
      var y_off = 220;
    
      //DrawXAxis(20, 400, cursor_y + 150, 50);
      //DrawCheckMark(450, 100);

      TimeMeasureStop("Dialog Ende");
    }

    
    private void DrawLegend(int xOffset, int yOffset, string text1, double pct2, string text2, double pct1)
    {
      double radius = GetRadius(40, 40);
      Vector center = new Vector(radius, radius);

      var x_pos  = Convert.ToInt32(radius) * 2 + xOffset;
      var y_pos1 = Convert.ToInt32(center.Y + 30 - yOffset);
      var y_pos2 = Convert.ToInt32(center.Y + 30 + yOffset);

      DrawRectangle(x_pos, y_pos1, 40, 20, Colors.LightGreen);
      DrawStringAtPos("Sehfeld:", x_pos + 50, y_pos1, 16);
      DrawStringAtPos(pct2.ToString() + "%", x_pos + 140, y_pos1, 16);

      DrawRectangle(x_pos, y_pos2, 40, 20, Colors.LightPink);
      DrawStringAtPos("außerhalb:", x_pos + 50, y_pos2, 16);
      DrawStringAtPos(pct1.ToString() + "%", x_pos + 140, y_pos2, 16);
      
      DrawStringAtPos(data.actualDate.ToString(), Convert.ToInt32(radius)+40, Convert.ToInt32(radius)*2 + 60, 16, TextAlignment.Center);
      //DrawBlackLine(0, Convert.ToInt32(center.Y)+40,1000, Convert.ToInt32(center.Y)+40, 2);
    }



    private void MyCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    { if (e.NewSize.Width == 0.0 && e.NewSize.Height == 0.0)
        windowSize = e.PreviousSize;
      else
        windowSize = e.NewSize;

      RefreshScreen();
    }


    public void GoBackInTime()
    {
      if (data.actualSequenceId > 1)
      { data.actualSequenceId--;
        ReadData();
        RefreshScreen();
      }

    }

    public void GoForwardInTime()
    {
      if (data.actualSequenceId < maxSequenceId)
      { data.actualSequenceId++;
        ReadData();
        RefreshScreen();
      }
    }


    private void _timer_Tick(object sender, EventArgs e)
    {
      int x=0;
      int y=0;

      if (testRect != null)
      {
        x = Convert.ToInt32(Canvas.GetLeft(testRect));
        y = Convert.ToInt32(Canvas.GetTop(testRect));
        MyCanvas.Children.Remove(testRect);
      }

      if (x > 500) _moveTimer.Stop();

      x++;
      y++;
    
      testRect = DrawRectangle(x, y, 40, 20, Colors.LightGreen);

      //Console.WriteLine("Timer Tick Event "+x.ToString()+","+y.ToString());

    }


    //https://de.wikipedia.org/wiki/Gaußsche_Trapezformel
    public double CalculatePolygonArea()
    {
      if (data.Points == null) return 0.0;              
  
      int n = data.Points.Count;
      if (n < 3) return 0.0; // a polygon must have at least 3 points
      double a = 0.0;                        
      for (int i = 0; i < n; i++)
      {
        a += (data.Points[i].Y + data.Points[(i + 1) % n].Y) * (data.Points[i].X - data.Points[(i + 1) % n].X);
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

    private void DrawBlackLine(int px1, int py1, int px2, int py2, int thickness)
    {
      var line = new Line()
      {
        X1 = px1,
        Y1 = py1,
        X2 = px2,
        Y2 = py2,
        Stroke = new SolidColorBrush(Colors.Black),
        StrokeThickness = thickness
      };

      MyCanvas.Children.Add(line);
    }


    private void DrawHorizontalLine(int size)
    {
      DrawBlackLine(cursor_x, cursor_y - 15, cursor_x + size, cursor_y - 15, 2);
    }



    private double GetRadius(int xOffset, int yOffset)
    { double radius;

      if (windowSize.Width - xOffset > windowSize.Height - yOffset - 50)
        radius = (windowSize.Height - yOffset - 50) / 2.0;
      else
        radius = (windowSize.Width - xOffset) / 2.0;

      if (radius * 2.0 > (windowSize.Width - xOffset) - 200.0)
        radius = ((windowSize.Width - xOffset) - 200.0) / 2.0;

      return radius;
    }

    private void DrawMidPoint(int x, int y)
    {
      double radius = GetRadius(40, 40);
      Vector center = new Vector(radius, radius);

      var ellipse = new Ellipse()
      {
        Width = 15,
        Height = 15,
        Stroke = new SolidColorBrush(Colors.Black),
        Fill = new SolidColorBrush(Colors.Black),
      };

      ellipse.SetValue(Canvas.LeftProperty, x+center.X - 7.5);
      ellipse.SetValue(Canvas.TopProperty, y+center.Y - 7.5);
      MyCanvas.Children.Add(ellipse);
    }


    private void DrawTestfield(int x, int y)
    {
      int[,] intPoints = new int[20, 2];

      var minWinExtend = Math.Min(windowSize.Width, windowSize.Height);

      double radius = GetRadius(40,40);
      int steps = 20;

      Vector center = new Vector(radius, radius);

      int count = 0;
      for (var i = 0; i < steps; i++)
      {
        Vector start = GetStartingPoint(i, steps, center, radius);

        intPoints[count, 0] = Convert.ToInt32(start.X);
        intPoints[count, 1] = Convert.ToInt32(start.Y);
        count++;
      }

      DrawPolygon(intPoints, x, y, Colors.Black, Colors.LightGreen, 2, false);
    }

        
    private void DrawTestPolygon(int x, int y)
    {

      double radius = GetRadius(40, 40);
      double scaleFactor = radius / data.Points[0].X;

      int[,] intPoints = new int[20, 2];

      int count = 0;
      foreach (var point in data.Points)
      {
        intPoints[count, 0] = Convert.ToInt32(point.X * scaleFactor);
        intPoints[count, 1] = Convert.ToInt32(point.Y * scaleFactor);
        count++;
       }

      DrawPolygon(intPoints, x, y, Colors.Black, Colors.LightPink, 2, false);

    }


    private void DrawPolygon(int[,] points, int x, int y, Color strokeColor, Color fillColor, int thickness)
    {
      DrawPolygon(points, x, y, strokeColor, fillColor, thickness, true);
    }

    private void DrawPolygon(int[,] points, int x, int y, Color strokeColor, Color fillColor, int thickness, bool translation)
    {
      // Create a blue and a black Brush  
      SolidColorBrush strokeBrush = new SolidColorBrush();
      strokeBrush.Color = strokeColor;
      SolidColorBrush fillBrush = new SolidColorBrush();
      fillBrush.Color = fillColor;
      // Create a Polygon  
      Polygon polygon = new Polygon();
      polygon.Stroke = strokeBrush;
      polygon.Fill = fillBrush;
      polygon.StrokeThickness = thickness;
      // Create a collection of points for a polygon 

      var x_min = 0;
      var y_min = 0;

      PointCollection polygonPoints = new PointCollection();
      if (translation)
      { x_min = int.MaxValue;
        y_min = int.MaxValue;

        for (int i = 0; i < points.Length / 2; i++)
        {
          x_min = Math.Min(y_min, points[i, 0]);
          y_min = Math.Min(y_min, points[i, 1]);
        }
      }


      for (int i = 0; i < points.Length / 2; i++)
      {
        System.Windows.Point Point = new System.Windows.Point(points[i, 0] - x_min + x, points[i, 1] - y_min + y);
        polygonPoints.Add(Point);
        //Console.WriteLine(i.ToString() + "  done " + points[i, 0].ToString() + "," + points[i, 1].ToString());
      }

      // Set Polygon.Points properties  
      polygon.Points = polygonPoints;

      // Add Polygon to the page 

      MyCanvas.Children.Add(polygon);
    }
    
    private Rectangle DrawRectangle(int x, int y, int x_size, int y_size, Color fillColor)
    {
      var MyStroke = new SolidColorBrush(fillColor);
     
      var myRect = new Rectangle
      { Stroke = System.Windows.Media.Brushes.Black,
        StrokeThickness = 2,
        Fill = MyStroke,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Center,
        Width = x_size,
        Height = y_size
      };
      MyCanvas.Children.Add(myRect);
      Canvas.SetLeft(myRect, x);
      Canvas.SetTop(myRect, y);
      return myRect;
    }


    private void DrawCheckMark(int x, int y)
    {
      int[,] points = { { 7, 83 }, { 20, 73 }, { 26, 74 }, { 41, 98 }, { 124, 14 }, { 128, 18 }, { 52, 116 }, { 34, 129 } };

      DrawPolygon(points, x, y, Colors.Black, Colors.LightGreen, 2);

    }

    private void DrawArrowTip(int x, int y, int x_size, int y_size)
    {
      int[,] points = { { 0, 0 }, { 0, y_size }, { x_size, y_size / 2 } };

      DrawPolygon(points, x, y, Colors.Black, Colors.Black, 2);

    }

    private void DrawStringAtPos(string text, int x, int y, int size, TextAlignment align)
    {
      TextBlock txt1 = new TextBlock
      { //TextAlignment = TextAlignment.Center,
        FontSize = size,
        Text = text,
        //FontWeight = FontWeights.UltraBold
      };

      if (align == TextAlignment.Center)
      {
        var sz = MeasureText(text, txt1.FontFamily, txt1.FontStyle, txt1.FontWeight, txt1.FontStretch, txt1.FontSize);
        Canvas.SetLeft(txt1, x - sz.Width / 2);
        //Console.WriteLine(" Text: " + text + " Width: " + sz.Width.ToString());
      }
      else
        Canvas.SetLeft(txt1, x);

      Canvas.SetTop(txt1, y);
    
      MyCanvas.Children.Add(txt1);
    }


    private void DrawStringAtPos(string text, int x, int y, int size)
    {
      DrawStringAtPos(text, x,y, size, TextAlignment.Left);
    }

    private void DrawString(string text, int size)
    {
      DrawStringAtPos(text, cursor_x, cursor_y, size);
      cursor_y += size * 2;
    }


    private void DrawXAxis(int x1, int x2, int y, int step)
    {
      const int size = 10;
      DrawBlackLine(x1, y, x2, y, 2);
      for (int i = x1 + step; i < x2; i += step)
      {
        DrawBlackLine(i, y - size / 2, i, y + size / 2, 2);
        DrawStringAtPos(i.ToString(), i, y + 5, 14, TextAlignment.Center);

      }
      DrawArrowTip(x2, y - size / 2, 20, size);

    }

    private void ReadData()
    {
      data.Points = new List<Point>();

      var count = 0;
      int entry_id;

      StreamReader fs = new StreamReader(path);
      string csv_line;
      char[] charSeparators = new char[] { ';' };

      Point p = new Point() { X = 0.0, Y = 0.0 };

      while ((csv_line = fs.ReadLine()) != null)
      {
        var elements = csv_line.Split(charSeparators, StringSplitOptions.None);

        DateTime Date;
   
        try
        {
          if (int.Parse(elements[0]) == data.actualSequenceId)
          {
            entry_id = int.Parse(elements[1]);
            Date = DateTime.Parse(elements[2]);
            p.X = double.Parse(elements[3]);
            p.Y = double.Parse(elements[4]);

            data.actualDate = Date;
            data.Points.Add(p);
       
           // if (count < 20)
           //   Console.WriteLine(count.ToString() + ". Date="+  Date.ToString()+" X = " + p.X.ToString() + " Y= " + p.Y.ToString());
          }
        }
        catch (FormatException e)
        {
          //Date = DateTime.Now;
          //X = 0;
          //Y = 0;
        }
        count++;
      }

      fs.Close();

    }



    private void GenerateTestData()
    {
      var rnd = new Random();

      GenerateTestDataFirstDay(1, 70.0,rnd);

      data.actualSequenceId = 1;
      ReadData();

      FileStream fs = new FileStream(path, FileMode.Append);

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
      data.actualSequenceId++;
      var day = DateTime.Now.AddDays(data.actualSequenceId);

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

        WriteCSV(fs, data.actualSequenceId, entry_id, day, center_vector);
        entry_id++;
       // break;
      }

      /*
            for (var i = 0; i < steps; i++)
            {
              double angle = i * 2*Math.PI / steps;
              start.X = data.Points[i].X;
              start.Y = data.Points[i].Y;

              center_vector = center - start;

              double pos = rnd.NextDouble();
              center_vector *= (pos - 0.5);

              center_vector += center;

              WriteCSV(fs, sequence_id, entry_id, day, center_vector);

              entry_id++;

            }
            */

    }


    private void WriteCSV(FileStream fs, int sequence_id, int entry_id, DateTime day, Vector center_vector)
    {
      string csv_line = sequence_id.ToString() + ";" + entry_id.ToString() + ";" + day.ToString() + ";" + 
                        center_vector.X.ToString() + ";" + center_vector.Y.ToString() + "\n";
      byte[] bytes = Encoding.UTF8.GetBytes(csv_line);
      fs.Write(bytes, 0, bytes.Length);

    }






    private Vector GetStartingPoint(int i, int n, Vector center, double radius)
    {
      double angle = i * 2 * Math.PI / n;
      return new Vector(Math.Sin(angle) * radius + center.X, Math.Cos(angle) * radius + center.Y);
    }


    private void GenerateTestDataFirstDay(int sequence_id, double sight_pct, Random rnd)
    {
      File.Delete(path);
      FileStream fs = File.OpenWrite(path);

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






  /// <summary>
  /// Get the required height and width of the specified text. Uses Glyph's
  /// </summary>
  public static Size MeasureText(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
    {
      Typeface typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
      GlyphTypeface glyphTypeface;

      if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
      {
        return MeasureTextSize(text, fontFamily, fontStyle, fontWeight, fontStretch, fontSize);
      }

      double totalWidth = 0;
      double height = 0;

      for (int n = 0; n < text.Length; n++)
      {
        ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[text[n]];

        double width = glyphTypeface.AdvanceWidths[glyphIndex] * fontSize;

        double glyphHeight = glyphTypeface.AdvanceHeights[glyphIndex] * fontSize;

        if (glyphHeight > height)
        {
          height = glyphHeight;
        }

        totalWidth += width;
      }

      return new Size(totalWidth, height);
    }

    /// <summary>
    /// Get the required height and width of the specified text. Uses FormattedText
    /// </summary>
    public static Size MeasureTextSize(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
    {
      FormattedText ft = new FormattedText(text,
                                           CultureInfo.CurrentCulture,
                                           FlowDirection.LeftToRight,
                                           new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                                           fontSize,
                                           Brushes.Black);
      return new Size(ft.Width, ft.Height);
    }


  }

}