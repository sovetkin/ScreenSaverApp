using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using ScreenSaverServer.BussinessLogic;

namespace ScreenSaver
{
    /// <summary>
    /// Сервер gRPC
    /// </summary>
    public class ScreenSaverService : ScreenSaver.ScreenSaverBase
    {
        #region Private Fields
        private readonly ILogger<ScreenSaverService> _logger;
        private IRectangleHandler _rectangleHandler;
        #endregion

        #region Public constructions

        public ScreenSaverService(ILogger<ScreenSaverService> logger, IRectangleHandler handler)
        {
            _logger = logger;
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
            Task.FromResult(_rectangleHandler.BordersSize);

        public override async Task<RectanglePoint> GetNewPosition(IdRequestMessage request, ServerCallContext context)
        {
            return await _rectangleHandler.GetRectangleCurrentPosition(request.Id);
        }

        public override async Task GetRectangleList(Empty request, IServerStreamWriter<RectangleModelMessage> responseStream, ServerCallContext context)
        {
            await _rectangleHandler.GenerateRepository();
            await _rectangleHandler.StartPathGeneration(context.CancellationToken);

            //await Task.WhenAll(task1, task2);

            await foreach (RectangleModelMessage item in _rectangleHandler.RectangleStrem())
            {
                if (context.CancellationToken.IsCancellationRequested)
                    break;

                await responseStream.WriteAsync(item);
            }
        }

        #endregion
    }
}
