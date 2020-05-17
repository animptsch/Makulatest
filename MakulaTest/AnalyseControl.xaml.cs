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
using System.Diagnostics;

namespace MakulaTest
{
  /// <summary>
  /// Interaction logic for AnalyseControl.xaml
  /// </summary>
  public partial class AnalyseControl : UserControl
  {
    private int cursor_x;
    private int cursor_y;
    private Stopwatch stopwatch;

    public AnalyseControl()
    {
      cursor_x = 30; // used for text lines
      cursor_y = 50;
      stopwatch = new Stopwatch();

      InitializeComponent();
    }

    public void Start()
    {
      TimeMeasureStart();

      DrawString("Die Analyse ist abgeschlossen.", 24);
      DrawHorizontalLine(320);
      DrawString("", 24);
      DrawString("Es ist alles in Ordnung.", 36);
      DrawString("Schönes Leben noch.", 36);
      DrawString("Und lass dich nicht ärgern.", 20);

      DrawXAxis(20, 500, cursor_y + 150, 50);
      DrawCheckMark(450, 100);
      TimeMeasureStop("Dialog Ende");
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






    private void DrawPolygon(int[,] points, int x, int y, Color strokeColor, Color fillColor, int thickness)
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

      var x_min = int.MaxValue;
      var y_min = int.MaxValue;

      PointCollection polygonPoints = new PointCollection();
      for (int i = 0; i < points.Length / 2; i++)
      {
        x_min = Math.Min(y_min, points[i, 0]);
        y_min = Math.Min(y_min, points[i, 1]);
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

    private void DrawStringAtPos(string text, int x, int y, int size)
    {
      TextBlock txt1 = new TextBlock();
      txt1.FontSize = size;
      txt1.Text = text;
      Canvas.SetLeft(txt1, x);
      Canvas.SetTop(txt1, y);
      MyCanvas.Children.Add(txt1);
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
        DrawStringAtPos(i.ToString(), i - 10, y + 5, 14);

      }
      DrawArrowTip(x2, y - size / 2, 20, size);

    }



  }
}
