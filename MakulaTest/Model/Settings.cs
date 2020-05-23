using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakulaTest.Model
{
    public class Settings
    {
        public double CircleSize { get; set; }
        public string Color { get; set; }

        public int Duration { get; set; }

        public Settings()
        {
            CircleSize = 15;
            Color = "#FFFF0000";
            Duration = 10;
        }
    }


}
