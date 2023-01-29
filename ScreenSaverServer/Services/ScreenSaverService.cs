using System.Collections.Generic;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using ScreenSaverServer.BussinessLogic;
using ScreenSaverServer.BussinessLogic.Models;

namespace ScreenSaver
{
    /// <summary>
    /// Сервер gRPC
    /// </summary>
    public class ScreenSaverService : ScreenSaver.ScreenSaverBase
    {
        #region Private Fields
        private readonly ILogger<ScreenSaverService> _logger;
        private CanvasBoundariesMessage _canvasSize;
        private IRectangleHandler _rectangleHandler;
        #endregion

        #region Public constructions

        public ScreenSaverService(ILogger<ScreenSaverService> logger, IRectangleHandler handler)
        {
            _logger = logger;
            _canvasSize = new() { Width = 800, Height = 500 };
            _rectangleHandler = handler;
        } 
        #endregion

        #region Public Methods
        /// <summary>
        /// Возвращает на клиент данные по полю где должны перемещаться прямоугольники
        /// </summary>
        /// <param name="_"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<CanvasBoundariesMessage> GetCanvasBoundaries(Empty _, ServerCallContext context) =>
            Task.FromResult(_canvasSize);

        /// <summary>
        /// Возвращает свойства вновь созданного прямоугольника
        /// </summary>
        /// <param name="_"></param>
        /// <param name="contex"></param>
        /// <returns></returns>
        public async override Task<RectangleModelMessage> GetRectangle(Empty _, ServerCallContext contex) =>
            await _rectangleHandler.GetInitialRectangleAsync();

        /// <summary>
        /// Записывает в стриминг запрошенные с клиента данные
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task GetRectangleCurrentPosition(IdRequestMessage request, IServerStreamWriter<RectanglePoint> responseStream, ServerCallContext context)
        {
            await _rectangleHandler.StartPathGeneration(context.CancellationToken);

            while (!context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(_rectangleHandler.GetRectangleCurrentPosition(request.Id));
            }
        }

        /// <summary>
        /// Серверная часть стриминга данных
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task DuplexRectangleStream(
            IAsyncStreamReader<RectangleModelMessage> requestStream,
            IServerStreamWriter<RectangleModelMessage> responseStream,
            ServerCallContext context)
        {
            try
            {
                await _rectangleHandler.StartPathGeneration(context.CancellationToken);

                while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
                {
                    RectangleModelMessage current = requestStream.Current;

                    await _rectangleHandler.SendResponseMessageAsync(current, responseStream);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                _logger.LogInformation("Operation cancelled");
            }
        }
        #endregion
    }
}
