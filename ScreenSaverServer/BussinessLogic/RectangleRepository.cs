using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

        #region Public Indexers
        public RectangleModel this[int index]
        {
            get
            {
                lock (_locker)
                {
                    return _rectangles[index];
                }
            }

            set
            {
                lock (_locker)
                {
                    _rectangles[index] = value;
                }
            }
        }
        #endregion

        #region Public properties
        public IList<RectangleModel> Rectangles => _rectangles;
        #endregion

        #region Public Methods

        /// <summary>
        /// Добавляет прямоугольник в хранилище
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public int AddRectangle(RectangleModel rectangle)
        {
            lock (_locker)
            {
                _rectangles.Add(rectangle);
                rectangle.Id = _rectangles.Count - 1;
                return rectangle.Id;
            }
        }

        /// <summary>
        /// Добавляет прямоугольник в хранилище асинхронно
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public async Task<int> AddRectangleAsync(RectangleModel rectangle) => await Task.Run(() => AddRectangle(rectangle));

        /// <summary>
        /// Возвращает координаты прямоугольника
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RectanglePoint GetCoordinate(int index)
        {
            lock (_locker)
            {
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
        /// <param name="point"></param>
        public void UpdateCoordinate(int index, RectanglePoint point)
        {
            lock (_locker)
            {
                _rectangles[index].Coordinate = point;
            }
        }

        /// <summary>
        /// Обновление координат прямоугольника асинхронно
        /// </summary>
        /// <param name="index"></param>
        /// <param name="point"></param>
        public async Task UpdateCoordinateAsync(int index, RectanglePoint point) =>
            await Task.Run(() => UpdateCoordinate(index, point));

        /// <summary>
        /// Обновление направление в котором двигается прямоугольник
        /// </summary>
        /// <param name="rectangle"></param>
        public void UpdateRectangleDirection(RectangleModel rectangle)
        {
            lock (_locker)
            {
                _rectangles[rectangle.Id].Direction = rectangle.Direction;
            }
        }

        /// <summary>
        /// Обновление направление в котором двигается прямоугольник асинхронно
        /// </summary>
        /// <param name="rectangle"></param>
        public async Task UpdateRectangleDirectionAsync(RectangleModel rectangle) => await Task.Run(() => UpdateRectangleDirection(rectangle));

        public void Clear() => _rectangles = new List<RectangleModel>();
        #endregion

        #region IEnumerable contract implementation
        public IEnumerator<RectangleModel> GetEnumerator() => _rectangles.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException(); 
        #endregion
    }
}
