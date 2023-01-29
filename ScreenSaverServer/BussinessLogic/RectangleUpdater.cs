using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ScreenSaver;

using ScreenSaverServer.BussinessLogic.Enums;
using ScreenSaverServer.BussinessLogic.Interfaces;
using ScreenSaverServer.BussinessLogic.Models;
using ScreenSaverServer.Extensions;

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

        public RectangleUpdater(IOptions<Config> options,IRectangleRepository repository, ILogger<RectangleUpdater> logger)
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
            var tasks = new Task[_options.ThreadsCount];

            for (int i = 0; i < tasks.Length; i++)
            {
                new Thread(() => TracePath(token)).Start();

                //tasks[i].Start();
                //_ = await Task.Factory.StartNew(TracePath, token);
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
                for (int i = 0; i < _repository.Rectangles.Count; i++)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                    await SetNewCoordinate(i);
                }
            }

            _repository.Clear();
        }

        /// <summary>
        /// Обновляет координаты прямоугольника в хранилище
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task SetNewCoordinate(int index)
        {
            await _repository.UpdateCoordinateAsync(index, await GetNewPosition(_repository[index]));
        }

        /// <summary>
        /// Получает новые координаты прямоугольника на основе текущих
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        private async Task<RectanglePoint> GetNewPosition(RectangleModel rectangle)
        {
            return await GetReflection(rectangle) switch
            {
                MoveDirection.UpLeft => rectangle.Coordinate.Offset((-1, -1)),
                MoveDirection.UpRight => rectangle.Coordinate.Offset((1, -1)),
                MoveDirection.DownLeft => rectangle.Coordinate.Offset((-1, 1)),
                MoveDirection.DownRight => rectangle.Coordinate.Offset((1, 1)),
                MoveDirection.None => rectangle.Coordinate,
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Вычисляет отражение для прямоугольника при достижении границы
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        private async Task<MoveDirection> GetReflection(RectangleModel rectangle)
        {
            if (rectangle.Coordinate.X <= 0)
            {
                rectangle.Direction = rectangle.Direction == MoveDirection.DownLeft ? MoveDirection.DownRight : MoveDirection.UpRight;
                await _repository.UpdateRectangleDirectionAsync(rectangle);
                return rectangle.Direction;
            }
            if (rectangle.Coordinate.X + rectangle.Size.Width >= 800)
            {
                rectangle.Direction = rectangle.Direction == MoveDirection.UpRight ? MoveDirection.UpLeft : MoveDirection.DownLeft;
                await _repository.UpdateRectangleDirectionAsync(rectangle);
                return rectangle.Direction;
            }
            if (rectangle.Coordinate.Y <= 0)
            {
                rectangle.Direction = rectangle.Direction == MoveDirection.UpRight ? MoveDirection.DownRight : MoveDirection.DownLeft;
                await _repository.UpdateRectangleDirectionAsync(rectangle);
                return rectangle.Direction;
            }
            if (rectangle.Coordinate.Y + rectangle.Size.Height >= 500)
            {
                rectangle.Direction = rectangle.Direction == MoveDirection.DownRight ? MoveDirection.UpRight : MoveDirection.UpLeft;
                await _repository.UpdateRectangleDirectionAsync(rectangle);
                return rectangle.Direction;
            }

            return rectangle.Direction = rectangle.Direction == MoveDirection.None ?
                                         RandomDirection((int)MoveDirection.DownLeft,
                                                         (int)MoveDirection.DownRight,
                                                         (int)MoveDirection.UpLeft,
                                                         (int)MoveDirection.UpRight) :
                                         rectangle.Direction;
        }

        /// <summary>
        /// Случайно определяет исходное направление передвижения
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private MoveDirection RandomDirection(params int[] values) => (MoveDirection)values[_rnd.Next(values.Length)];
    }
}
