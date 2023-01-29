using System;
using System.Threading;
using System.Threading.Tasks;

using Grpc.Core;

using Microsoft.Extensions.Options;

using ScreenSaver;

using ScreenSaverServer.BussinessLogic.Interfaces;
using ScreenSaverServer.BussinessLogic.Models;

namespace ScreenSaverServer.BussinessLogic
{
    /// <summary>
    /// Ответственнен за логику работы с хранилищем прямоугольников
    /// </summary>
    public class RectangleHandler : IRectangleHandler
    {
        #region Private Fields
        private readonly Config _options;
        private readonly IRectangleRepository _repository;
        private readonly IPathGenerator _updater;
        private RectangleSize _recDefaultSize;
        private CanvasBoundariesMessage _borders;
        private Random _rnd;
        #endregion

        #region Public constructors
        public RectangleHandler(IOptions<Config> options, IRectangleRepository repository, IPathGenerator updater)
        {
            _rnd = new();
            _options = options.Value;
            _repository = repository;
            _updater = updater;
            _recDefaultSize = new()
            {
                Width = _options.RectangleWidth,
                Height = _options.RectangleHeight
            };

            _borders = new()
            {
                Height = _options.CanvasHeight,
                Width = _options.CanvasWidth
            };
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Запускает расчет траеторий прямоугольников
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task StartPathGeneration(CancellationToken token) => await _updater.StartAsync(token);

        /// <summary>
        /// Создает прямоугольник в хранилище и предоставляет начальные значения для прямоугольника
        /// </summary>
        /// <returns></returns>
        public async Task<RectangleModelMessage> GetInitialRectangleAsync()
        {
            RectangleModel rectangle = new()
            {
                Size = _recDefaultSize,
                Coordinate = await GenerateRectangleInitialPositionAsync(),
                Direction = Enums.MoveDirection.None
            };

            int index = await _repository.AddRectangleAsync(rectangle);

            return new RectangleModelMessage()
            {
                Size = rectangle.Size,
                Coordinate = rectangle.Coordinate,
                Id = index
            };
        }

        /// <summary>
        /// Предоставляет текущие позиции прямоугольника
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RectanglePoint GetRectangleCurrentPosition(int index) => _repository.GetCoordinate(index);

        /// <summary>
        /// Записывет данные прямоугольника в стрим данных сервера
        /// </summary>
        /// <param name="current"></param>
        /// <param name="responseStream"></param>
        /// <returns></returns>
        public async Task SendResponseMessageAsync(RectangleModelMessage current, IServerStreamWriter<RectangleModelMessage> responseStream)
        {
            await responseStream.WriteAsync(new()
            {
                Coordinate = _repository[current.Id].Coordinate,
                Size = _repository[current.Id].Size,
                Id = _repository[current.Id].Id
            });
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Предоставляет начальные значения координат прямоугольника
        /// </summary>
        /// <returns></returns>
        private async Task<RectanglePoint> GenerateRectangleInitialPositionAsync()
        {
            return await Task.Run(() => new RectanglePoint()
            {
                X = _rnd.Next(0, (int)_borders.Width - (int)_recDefaultSize.Width),
                Y = _rnd.Next(0, (int)_borders.Height - (int)_recDefaultSize.Height)
            });
        }
        #endregion
    }
}
