using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakulaTest.Model
{
    public class Settings
    {
        public int Steps { get; set; }

        public int Duration { get; set; }

        public int DurationBackwards { get; set; }

        public bool Backward { get; set; }

        public bool RightEye { get; set; }

        public string BackgroundColor { get; set; }
        public string LineColor { get; set; }
        public string BallColor { get; set; }

        public Settings()
        {                        
            Duration = 10;
            DurationBackwards = 10;
            Steps = 20;
            LineColor = "#FFFFFF00";
            BackgroundColor = "#FFffffff";
            BallColor = "#ff000000";
        }
    }


}
