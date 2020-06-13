using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Shapes;

namespace MakulaTest.Model
{
  public class MakulaDataSet
  {
    private readonly string pathInternal;

    public MakulaDataSet(string path)
    {
      pathInternal = path;
    }

    public void SaveData(List<Point> Points, bool backward, bool rightEye, int circleSize, Rectangle MyRectangle)
    {
      double midX = MyRectangle.Width / 2.0 + MyRectangle.Margin.Left + circleSize / 2.0;
      double midY = MyRectangle.Height / 2.0 + MyRectangle.Margin.Top + circleSize / 2.0;

      var day = DateTime.Now;
      var maxSequenceId = GetMaxSequenceId();
      var entryId = 0;

      FileStream fs = new FileStream(pathInternal, FileMode.Append);

      foreach (var point in Points)
      {
        var realPoint = point;

        var relX = point.X + circleSize / 2 - midX;
        var relY = point.Y + circleSize / 2 - midY;

        realPoint.X = relX * 130.0 / MyRectangle.Width;
        realPoint.Y = relY * 130.0 / MyRectangle.Height;
      
        WriteCSV(fs, maxSequenceId + 1, entryId, day, backward, rightEye, realPoint);
        entryId++;
      }

      fs.Close();
    }

    private int GetMaxSequenceId()
    {
      var sequenceIdMax = 0;

      if (File.Exists(pathInternal))
      {
        StreamReader fs = new StreamReader(pathInternal);
        string csvLine;
        char[] charSeparators = new char[] { ';' };

        while ((csvLine = fs.ReadLine()) != null)
        {
          var elements = csvLine.Split(charSeparators, StringSplitOptions.None);

          try
          {
            var sequence_id = int.Parse(elements[0]);
            sequenceIdMax = Math.Max(sequenceIdMax, sequence_id);
          }
          catch (FormatException e)
          {
          }
        }
        fs.Close();
      }

      //Console.WriteLine("sequence_id_max=" + sequence_id_max.ToString());
      return sequenceIdMax;
    }

    private void WriteCSV(FileStream fs, int sequenceId, int entryId, DateTime day, bool backward, bool rightEye, Point point)
    {
      string csv_line = sequenceId.ToString() + ";" + entryId.ToString() + ";" + day.ToString() + ";" +
                        backward.ToString() + ";" + rightEye.ToString() + ";" +
                        Math.Round(point.X,1).ToString() + ";" + Math.Round(point.Y,1).ToString() + "\n";
      byte[] bytes = Encoding.UTF8.GetBytes(csv_line);
      fs.Write(bytes, 0, bytes.Length);
    }
  }

}