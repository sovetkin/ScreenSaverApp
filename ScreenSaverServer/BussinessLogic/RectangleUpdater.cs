using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ScreenSaver;

using ScreenSaverServer.BussinessLogic.Enums;
using ScreenSaverServer.BussinessLogic.Interfaces;
using ScreenSaverServer.BussinessLogic.Models;

namespace ScreenSaverServer.BussinessLogic
{
    /// <summary>
    /// Ответственнен за расчет трактории для прямоугольника
    /// </summary>
    public class RectangleUpdater : IPathGenerator
    {
        private Random _rnd = new();
        private readonly IRectangleRepository _repository;
        private readonly Config _options;
        private object _locker;

        public RectangleUpdater(IOptions<Config> options, IRectangleRepository repository, ILogger<RectangleUpdater> logger)
        {
            _repository = repository;
            _options = options.Value;
        }

        /// <summary>
        /// Инициализирует расчет траектории
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken token)
        {
            for (int i = 0; i < _options.ThreadsCount; i++)
            {
                await Task.Factory.StartNew(TracePath, token);
            }
        }

        /// <summary>
        /// Поддерживает вычисление траектории пока не будет получен токен на отмену.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task TracePath(object? token)
        {
            var tkn = (CancellationToken)token;

            while (!tkn.IsCancellationRequested)
            {
                for (int i = 0; i < _repository.Count; i++)
                {
                    //await Task.Delay(TimeSpan.FromMilliseconds(10));

                    if (_repository.IsCalculationRequired(i))
                        await SetNewCoordinate(i);
                }
            }
        }

        /// <summary>
        /// Обновляет координаты прямоугольника в хранилище
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task SetNewCoordinate(int index)
        {
            RectangleModel rectangle = await _repository.GetRectangleByIdAsync(index);
            (RectanglePoint, MoveDirection) value = await GetNewPosition(rectangle);
            await _repository.UpdateCoordinateAsync(index, value);
        }

        /// <summary>
        /// Получает новые координаты прямоугольника на основе текущих
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        private async Task<(RectanglePoint, MoveDirection)> GetNewPosition(RectangleModel rectangle)
        {
            MoveDirection move = await GetReflection(rectangle);
            RectanglePoint point = await CalculateNewCoordinate(rectangle.Coordinate, move);

            return (point, move);
        }

        /// <summary>
        /// Вычисляет отражение для прямоугольника при достижении границы
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        private async Task<MoveDirection> GetReflection(RectangleModel rectangle)
        {
            if (rectangle.Coordinate.X <= 0)
                return await Task.Run(() => rectangle.Direction == MoveDirection.DownLeft ? MoveDirection.DownRight : MoveDirection.UpRight);

            if (rectangle.Coordinate.X + rectangle.Size.Width >= 800)
                return await Task.Run(() => rectangle.Direction == MoveDirection.UpRight ? MoveDirection.UpLeft : MoveDirection.DownLeft);

            if (rectangle.Coordinate.Y <= 0)
                return await Task.Run(() => rectangle.Direction == MoveDirection.UpRight ? MoveDirection.DownRight : MoveDirection.DownLeft);

            if (rectangle.Coordinate.Y + rectangle.Size.Height >= 500)
                return await Task.Run(() => rectangle.Direction == MoveDirection.DownRight ? MoveDirection.UpRight : MoveDirection.UpLeft);

            return await Task.Run(() => rectangle.Direction == MoveDirection.None ?
                                                 RandomDirection((int)MoveDirection.DownLeft,
                                                                 (int)MoveDirection.DownRight,
                                                                 (int)MoveDirection.UpLeft,
                                                                 (int)MoveDirection.UpRight) :
                                                 rectangle.Direction);
        }

        /// <summary>
        /// Случайно определяет исходное направление передвижения
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private MoveDirection RandomDirection(params int[] values) => (MoveDirection)values[_rnd.Next(values.Length)];

        private async Task<RectanglePoint> CalculateNewCoordinate(RectanglePoint coordinate, MoveDirection direction)
        {
            double deltaY;
            double deltaX;
            RectanglePoint point = new();

            switch (direction)
            {
                case MoveDirection.None:
                    break;
                case MoveDirection.UpLeft:
                    deltaY = coordinate.Y;
                    deltaX = coordinate.X;
                    if (deltaY > deltaX)
                    {
                        point.Y = coordinate.Y - deltaX;
                        point.X = 0;
                    }
                    else
                    {
                        point.Y = 0;
                        point.X = coordinate.X - deltaY;
                    }
                    break;
                case MoveDirection.UpRight:
                    deltaY = coordinate.Y;
                    deltaX = _options.CanvasWidth - (coordinate.X + _options.RectangleWidth);
                    if (deltaY > deltaX)
                    {
                        point.X = coordinate.X + deltaX;
                        point.Y = coordinate.Y - deltaX;
                    }
                    else
                    {
                        point.X = coordinate.X + deltaY;
                        point.Y = 0;
                    }
                    break;
                case MoveDirection.DownLeft:
                    deltaY = _options.CanvasHeight - (coordinate.Y + _options.RectangleHeight);
                    deltaX = coordinate.X;
                    if (deltaY > deltaX)
                    {
                        point.Y = coordinate.Y + deltaX;
                        point.X = 0;
                    }
                    else
                    {
                        point.Y = coordinate.Y + deltaY;
                        point.X = coordinate.X - deltaY;
                    }
                    break;
                case MoveDirection.DownRight:
                    deltaY = _options.CanvasHeight - (coordinate.Y + _options.RectangleHeight);
                    deltaX = _options.CanvasWidth - (coordinate.X + _options.RectangleWidth);
                    if (deltaY > deltaX)
                    {
                        point.Y = coordinate.Y + deltaX;
                        point.X = _options.CanvasWidth - _options.RectangleWidth;
                    }
                    else
                    {
                        point.Y = coordinate.Y + deltaY;
                        point.X = coordinate.X + deltaY;
                    }
                    break;
                default:
                    break;
            }

            return await Task.FromResult(point);
        }
    }
}
