using ScreenSaver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScreenSaverServer.Extensions
{
    public static class RectanglePointExtensions
    {
        public static RectanglePoint Offset(this RectanglePoint rectangle, (double x, double y) offset)
        {
            rectangle.X += offset.x;
            rectangle.Y += offset.y;

            return rectangle;
        }
    }
}
