using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        #region Public Properties
        public CanvasBoundariesMessage BordersSize => _borders;
        #endregion

        #region Public Methods
        /// <summary>
        /// Запускает расчет траеторий прямоугольников
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task StartPathGeneration(CancellationToken token) => await _updater.StartAsync(token);

        public async Task GenerateRepository()
        {
            for (int i = 0; i < _options.RectangleCount; i++)
            {
                RectanglePoint coordinate = await GenerateRectangleInitialPositionAsync();

                await _repository.AddRectangleAsync(coordinate, _recDefaultSize);
            }
        }

        public async Task<RectanglePoint> GetRectangleCurrentPosition(int index) => await _repository.GetCoordinateAsync(index);

        public async IAsyncEnumerable<RectangleModelMessage> RectangleStrem()
        {
            for (int i = 0; i < _repository.Count; i++)
            {
                RectangleModel rectangle = await _repository.GetRectangleByIdAsync(i);

                yield return new RectangleModelMessage()
                {
                    Coordinate = rectangle.Coordinate,
                    Size = rectangle.Size,
                    Id = rectangle.Id
                };
            }
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
