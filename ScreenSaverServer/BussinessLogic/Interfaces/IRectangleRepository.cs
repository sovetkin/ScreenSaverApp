
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using ScreenSaver;

using ScreenSaverServer.BussinessLogic.Enums;
using ScreenSaverServer.BussinessLogic.Models;

namespace ScreenSaverServer.BussinessLogic.Interfaces
{
    public interface IRectangleRepository : IEnumerable<RectangleModel>
    {
        int Count { get; }
        Task<int> AddRectangleAsync(RectanglePoint coordinate, RectangleSize size);
        Task<RectanglePoint> GetCoordinateAsync(int index);
        Task UpdateCoordinateAsync(int index, (RectanglePoint, MoveDirection) value);
        RectanglePoint GetCoordinate(int index);
        Task<RectangleModel> GetRectangleByIdAsync(int index, bool reCalculate = false);
        bool IsCalculationRequired(int index);
    }
}
