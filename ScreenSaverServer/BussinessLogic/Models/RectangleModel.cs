using ScreenSaver;

using ScreenSaverServer.BussinessLogic.Enums;

namespace ScreenSaverServer.BussinessLogic.Models
{
    /// <summary>
    /// Модель прямоугольника хранящегося в хранилище
    /// </summary>
    public class RectangleModel
    {
        public int Id { get; set; }
        public RectangleSize Size { get; set; }
        public RectanglePoint Coordinate { get; set; }
        public MoveDirection Direction { get; set; }
    }
}
