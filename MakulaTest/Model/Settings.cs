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

        public bool Backward { get; set; }

        public Settings()
        {                        
            Duration = 10;
            Steps = 20;
            Backward = false;
        }
    }


}
