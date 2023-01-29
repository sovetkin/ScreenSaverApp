
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using ScreenSaver;

using ScreenSaverServer.BussinessLogic.Models;

namespace ScreenSaverServer.BussinessLogic.Interfaces
{
    public interface IRectangleRepository : IEnumerable<RectangleModel>
    {
        RectangleModel this[int index] { get; set; }
        IList<RectangleModel> Rectangles { get; }
        Task<int> AddRectangleAsync(RectangleModel rectangle);
        Task<RectanglePoint> GetCoordinateAsync(int index);
        Task UpdateCoordinateAsync(int index, RectanglePoint point);
        RectanglePoint GetCoordinate(int index);
        Task UpdateRectangleDirectionAsync(RectangleModel rectangle);
        void Clear();
    }
}
