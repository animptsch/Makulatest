using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;

namespace MakulaTest.Model
{
  public class MakulaDataSet
  {
    private readonly string pathInternal;

    public MakulaDataSet(string path)
    {
      pathInternal = path;
    }

    public void SaveData(List<Point> Points, bool backward, bool rightEye, int circleSize, int midX, int midY, int monitorWidth, int monitorHeight)
    {
      var day = DateTime.Now;
      var maxSequenceId = GetMaxSequenceId();
      var entryId = 0;

      FileStream fs = new FileStream(pathInternal, FileMode.Append);

      foreach (var point in Points)
      {
        var realPoint = point;

        var relX = Convert.ToInt32(point.X) + circleSize / 2 - midX;
        var relY = Convert.ToInt32(point.Y) + circleSize / 2 - midY;
        realPoint.X = relX * monitorWidth / SystemParameters.PrimaryScreenWidth;
        realPoint.Y = relY * monitorHeight / SystemParameters.PrimaryScreenHeight;

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
                        Math.Round(point.X).ToString() + ";" + Math.Round(point.Y).ToString() + "\n";
      byte[] bytes = Encoding.UTF8.GetBytes(csv_line);
      fs.Write(bytes, 0, bytes.Length);
    }
  }

}