using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakulaTest.Model
{
    [Serializable]
    public class SerializedWindowsState
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsMaximized { get; set; }

        public double CanvasHeight { get; set; }
        public double CanvasWidth { get; set; }
    }
}
