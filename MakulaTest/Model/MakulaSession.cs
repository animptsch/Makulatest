using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MakulaTest.Model
{
    public class MakulaSession
    {
        public DateTime TimeStamp { get; set; }

        public List<Point> Points { get; set; }

        public MakulaSession()
        {
            TimeStamp = DateTime.Now;
            Points = new List<Point>();
        }

    }
}
