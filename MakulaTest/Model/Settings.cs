using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakulaTest.Model
{
    [Serializable]
    public class Settings
    {
        public int Steps { get; set; }

        public int Duration { get; set; }

        public int DurationBackwards { get; set; }
     
        public string BackgroundColor { get; set; }
        public string LineColor { get; set; }
        public string BallColor { get; set; }
        public string PolygonColor { get; set; }

        public Settings()
        {                        
            Duration = 10;
            DurationBackwards = 10;
            Steps = 20;
            LineColor = "#FFFFFF00";
            BackgroundColor = "#FFffffff";
            BallColor = "#ff000000";
            PolygonColor = "#FFAADD00";
        }

        
    }


}
