using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Globalization;

namespace MakulaTest.Model
{
  class Draw
  {
    private Canvas _canvas;
    private double cursor_x;
    private double cursor_y;

    public Draw(Canvas canvas)
    {
      _canvas = canvas;
      cursor_x = 30.0; // used for text lines
      cursor_y = 50.0;

    }


    private void DrawHorizontalLine(int size)
    {
      DrawBlackLine(cursor_x, cursor_y - 15, cursor_x + size, cursor_y - 15, 2);
    }



    private void DrawCheckMark(int x, int y)
    {
      double[,] points = { { 7, 83 }, { 20, 73 }, { 26, 74 }, { 41, 98 }, { 124, 14 }, { 128, 18 }, { 52, 116 }, { 34, 129 } };

      DrawPolygon(points, x, y, Colors.Black, Colors.LightGreen, 2);

    }

    private void DrawArrowTip(int x, int y, int x_size, int y_size)
    {
      double[,] points = { { 0, 0 }, { 0, y_size }, { x_size, y_size / 2 } };

      DrawPolygon(points, x, y, Colors.Black, Colors.Black, 2);

    }


    private void DrawXAxis(double x1, double x2, double y, double step)
    {
      const int size = 10;
      DrawBlackLine(x1, y, x2, y, 2);
      for (double i = x1 + step; i < x2; i += step)
      {
        DrawBlackLine(i, y - size / 2, i, y + size / 2, 2);
        DrawStringAtPos(i.ToString(), i, y + 5, 14, TextAlignment.Center);

      }
      DrawArrowTip(x2, y - size / 2, 20, size);

    }
    public void DrawArrowTip(double x, double y, double x_size, double y_size)
    {
      double[,] points = { { 0.0, 0.0 }, { 0.0, y_size }, { x_size, y_size / 2.0 } };

      DrawPolygon(points, x, y, Colors.Black, Colors.Black, 2);

    }
    public void DrawPolygon(double[,] points, double x, double y, Color strokeColor, Color fillColor, int thickness)
    {
      DrawPolygon(points, x, y, strokeColor, fillColor, thickness, true);
    }

    public void DrawPolygon(double[,] points, double x, double y, Color strokeColor, Color fillColor, int thickness, bool translation)
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

      double x_min = 0;
      double y_min = 0;

      PointCollection polygonPoints = new PointCollection();
      if (translation)
      {
        x_min = int.MaxValue;
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

      _canvas.Children.Add(polygon);
    }

    public Ellipse DrawCircle(double x, double y, double CircleSize, Brush brush)
    {
      return DrawEllipse(x, y, CircleSize, CircleSize, brush, brush);
    }

    public Ellipse DrawEllipse(double x, double y, double width, double height, Brush strokeBrush, Brush fillBrush)
    {
      var ellipse = new Ellipse()
      {

        Width = width,
        Height = height,
        Stroke = strokeBrush,
        Fill = fillBrush,
      };

      ellipse.SetValue(Canvas.LeftProperty, x - width/2.0);
      ellipse.SetValue(Canvas.TopProperty, y - height/2.0);
      _canvas.Children.Add(ellipse);
      return ellipse;
    }

    public Ellipse DrawEllipse(double x, double y, double width, double height, Color strokeColor, Color fillColor)
    {
      var strokeBrush = new SolidColorBrush(strokeColor);
      var fillBrush = new SolidColorBrush(fillColor);

      return DrawEllipse(x, y, width, height, strokeBrush, fillBrush);

    }


    public void DrawLine(double px1, double py1, double px2, double py2, double thickness, Color strokeColor)
    {
      var strokeBrush = new SolidColorBrush(strokeColor);

      DrawLine(px1, py1, px2, py2, thickness, strokeBrush);
    }

    public void DrawLine(double px1, double py1, double px2, double py2, double thickness, Brush strokeBrush)
    {
      var line = new Line()
      {
        X1 = px1,
        Y1 = py1,
        X2 = px2,
        Y2 = py2,
        Stroke = strokeBrush,
        StrokeThickness = thickness
      };

      _canvas.Children.Add(line);
    }


    public void DrawBlackLine(double px1, double py1, double px2, double py2, double thickness)
    {
      DrawLine(px1, py1, px2, py2, thickness, Colors.Black);
    }


    public Rectangle DrawRectangle(double x, double y, double x_size, double y_size, Color fillColor)
    {
      return DrawRectangle(x, y, x_size, y_size, fillColor, 2);
    }

    public Rectangle DrawRectangle(double x, double y, double x_size, double y_size, Color fillColor, int thickness)
    {
      var MyStroke = new SolidColorBrush(fillColor);

      var myRect = new Rectangle
      {
        Stroke = System.Windows.Media.Brushes.Black,
        StrokeThickness = thickness,
        Fill = MyStroke,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Center,
        Width = x_size,
        Height = y_size
      };

      _canvas.Children.Add(myRect);
      Canvas.SetLeft(myRect, x);
      Canvas.SetTop(myRect, y);
      return myRect;
    }


    public void DrawStringAtPos(string text, double x, double y, double size)
    {
      DrawStringAtPos(text, x, y, size, TextAlignment.Left);
    }

    public void DrawString(string text, double size)
    {
      DrawStringAtPos(text, cursor_x, cursor_y, size);
      cursor_y += size * 2;
    }

    public void DrawStringAtPos(string text, double x, double y, double size, TextAlignment align)
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

      _canvas.Children.Add(txt1);
    }

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
