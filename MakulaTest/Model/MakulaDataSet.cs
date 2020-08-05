using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Shapes;

namespace MakulaTest.Model
{
  public class MakulaDataSetInternal
  {
    public List<Point> Points;
    public int actualSequence;
    public DateTime actualDate;
    public int record_no;
    public bool deleted;

  }

  public class MakulaDataSet
  {
    private readonly string pathInternal;
    private List<int> _sequences;

    public bool backward;
    public bool rightEye;
    public double minDistance; // point distance to center
    public MakulaDataSetInternal data;

    public MakulaDataSet(string path)
    {
      pathInternal = path;
      data = new MakulaDataSetInternal();

    }

    public void SaveData(List<Point> Points, bool backward, bool rightEye, int circleSize, Rectangle MyRectangle)
    {
      double midX = MyRectangle.Width / 2.0 + MyRectangle.Margin.Left + circleSize / 2.0;
      double midY = MyRectangle.Height / 2.0 + MyRectangle.Margin.Top + circleSize / 2.0;

      var day = DateTime.Now;
      var maxSequenceId = GetMaxSequenceIdFromFile();
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
    public void DeleteRecord(int sequenceIdx)
    {
      List<string> fileContent = new List<string>();

      StreamReader fs = new StreamReader(pathInternal);
      string csv_line;
      char[] charSeparators = new char[] { ';' };

      while ((csv_line = fs.ReadLine()) != null)
      {
        csv_line += ";True"; // add ActiveFlag to ensure to have at least 8 elements
        var elements = csv_line.Split(charSeparators, StringSplitOptions.None);
          
        DateTime Date;

        try
        {
          if (int.Parse(elements[0]) == _sequences[sequenceIdx]) elements[7] = "False";
        }
        catch (FormatException e)
        { }

        csv_line = elements[0] + ";" + elements[1] + ";" + elements[2] + ";" +
                 elements[3] + ";" + elements[4] + ";" +
                 elements[5] + ";" + elements[6] + ";" + elements[7] + "\n";

        fileContent.Add(csv_line);
      }
      fs.Close();

      //  FileStream fs2 = new FileStream(pathInternal, FileMode.Create);
      FileStream fs2 = new FileStream(pathInternal, FileMode.Create);
     
      foreach (var r in fileContent)
      {
        byte[] bytes = Encoding.UTF8.GetBytes(r);
        fs2.Write(bytes, 0, bytes.Length);
      }

      fs2.Close();
    }

    public int GetMaxSequenceId()
    {
      return _sequences.Count - 1;
    }
      
      
    private int GetMaxSequenceIdFromFile()
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

    public void WriteCSV(FileStream fs, int sequence_id, int entry_id, DateTime day, Vector center_vector)
    {
      string csv_line = sequence_id.ToString() + ";" + entry_id.ToString() + ";" + day.ToString() + ";" +
                        center_vector.X.ToString() + ";" + center_vector.Y.ToString() + "\n";
      byte[] bytes = Encoding.UTF8.GetBytes(csv_line);
      fs.Write(bytes, 0, bytes.Length);

    }



    public void ReadSequences()
    {
      _sequences = new List<int>();

      var lastSequenceId = -1;
      if (File.Exists(pathInternal))
      {
        StreamReader fs = new StreamReader(pathInternal);
        string csv_line;
        char[] charSeparators = new char[] { ';' };

        //Point p = new Point() { X = 0.0, Y = 0.0 };

        while ((csv_line = fs.ReadLine()) != null)
        {
          var elements = csv_line.Split(charSeparators, StringSplitOptions.None);
          if (elements.Length < 8 || elements[7] == "True" || elements[7] == "False")
          {
            try
            {
              var sequence_id = int.Parse(elements[0]);
              if (sequence_id != lastSequenceId)
                _sequences.Add(sequence_id);
              lastSequenceId = sequence_id;
            }
            catch (FormatException e)
            { }
          }
        }
        fs.Close();
      }
 
      data.actualSequence = _sequences.Count - 1;
    }

    public void ReadData()
    {
      data.Points = new List<Point>();

      if (File.Exists(pathInternal))
      {
        var count = 0;
        int entry_id;

        StreamReader fs = new StreamReader(pathInternal);
        string csv_line;
        char[] charSeparators = new char[] { ';' };

        Point p = new Point() { X = 0.0, Y = 0.0 };

        minDistance = 20000.0;

        while ((csv_line = fs.ReadLine()) != null)
        {
          var elements = csv_line.Split(charSeparators, StringSplitOptions.None);


          DateTime Date;

          try
          {
            if (int.Parse(elements[0]) == _sequences[data.actualSequence])
            {
              entry_id = int.Parse(elements[1]);
              Date = DateTime.Parse(elements[2]);
              backward = Boolean.Parse(elements[3]);
              rightEye = Boolean.Parse(elements[4]);

              p.X = double.Parse(elements[5]);
              p.Y = double.Parse(elements[6]);

              data.actualDate = Date;
              data.Points.Add(p);

              data.record_no = int.Parse(elements[0]);
              if (elements.Length < 8)
                data.deleted = false;
              else
                data.deleted = Boolean.Parse(elements[7]);

              var d = Math.Sqrt(p.X * p.X + p.Y * p.Y);
              if (d < minDistance) minDistance = d;

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
    }




  }

}