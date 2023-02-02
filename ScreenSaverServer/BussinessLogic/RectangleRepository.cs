using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using ScreenSaver;

using ScreenSaverServer.BussinessLogic.Enums;
using ScreenSaverServer.BussinessLogic.Interfaces;
using ScreenSaverServer.BussinessLogic.Models;

namespace ScreenSaverServer.BussinessLogic
{
    /// <summary>
    /// Ответственный за хранение прямоугольников
    /// </summary>
    public class RectangleRepository : IRectangleRepository
    {
        #region Private Fields
        private IList<RectangleModel> _rectangles;
        private object _locker;
        #endregion

        #region Public constructors
        public RectangleRepository()
        {
            _rectangles = new List<RectangleModel>();
            _locker = new object();
        }
        #endregion

        #region Public properties
        public int Count => _rectangles.Count;
        #endregion

        #region Public Methods

        /// <summary>
        /// Добавляет прямоугольник в хранилище
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public int AddRectangle(RectanglePoint point, RectangleSize size)
        {
            lock (_locker)
            {
                RectangleModel rectangle = new()
                {
                    Coordinate = point,
                    Size = size,
                    Id = _rectangles.Count,
                    Direction = MoveDirection.None,
                    Status = RectangleCalculationStatus.CalculationRequired
                };
                _rectangles.Add(rectangle);

                return rectangle.Id;
            }
        }

        /// <summary>
        /// Добавляет прямоугольник в хранилище асинхронно
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public async Task<int> AddRectangleAsync(RectanglePoint coordinate, RectangleSize size)
            => await Task.Run(() => AddRectangle(coordinate, size));

        /// <summary>
        /// Возвращает координаты прямоугольника
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RectanglePoint GetCoordinate(int index)
        {
            lock (_locker)
            {
                _rectangles[index].Status = RectangleCalculationStatus.CalculationRequired;
                return _rectangles[index].Coordinate;
            }
        }

        /// <summary>
        /// Возвращает координаты прямоугольника асинхронно
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<RectanglePoint> GetCoordinateAsync(int index) => await Task.Run(() => GetCoordinate(index));

        /// <summary>
        /// Обновление координат прямоугольника
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void UpdateCoordinate(int index, (RectanglePoint, MoveDirection) value)
        {
            lock (_locker)
            {
                if (IsCalculationRequired(index))
                {
                    _rectangles[index].Coordinate = value.Item1;
                    _rectangles[index].Direction = value.Item2;
                    _rectangles[index].Status = RectangleCalculationStatus.CalculationDone;
                }
            }
        }

        /// <summary>
        /// Обновление координат прямоугольника асинхронно
        /// </summary>
        /// <param name="index"></param>
        /// <param name="point"></param>
        public async Task UpdateCoordinateAsync(int index, (RectanglePoint, MoveDirection) value) =>
            await Task.Run(() => UpdateCoordinate(index, value));

        public RectangleModel GetRectangleById(int index, bool reCalculate = false)
        {
            lock (_locker)
            {
                if (reCalculate)
                    _rectangles[index].Status = RectangleCalculationStatus.CalculationRequired;
                return _rectangles[index];
            }
        }

        public async Task<RectangleModel> GetRectangleByIdAsync(int index, bool reCalculate = false) =>
            await Task.Run(() => GetRectangleById(index, reCalculate));

        public bool IsCalculationRequired(int index)
        {
            lock (_locker)
            {
                return _rectangles[index].Status == RectangleCalculationStatus.CalculationRequired;
            }
        }
        #endregion

        #region IEnumerable contract implementation
        public IEnumerator<RectangleModel> GetEnumerator() => _rectangles.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        #endregion
    }
}
