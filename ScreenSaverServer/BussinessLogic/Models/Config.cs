using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ScreenSaverServer.BussinessLogic.Models
{
    public class Config
    {
        public double CanvasHeight { get; set; }
        public double CanvasWidth { get; set; }
        public double RectangleHeight { get; set; }
        public double RectangleWidth { get; set; }
        public int ThreadsCount { get; set; }
    }
}
