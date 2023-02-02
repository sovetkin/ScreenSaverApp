using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;
using Grpc.Net.Client;

using ScreenSaver;

using ScreenSaverWpfClient.Model;

using static ScreenSaver.ScreenSaver;

namespace ScreenSaverWpfClient.DataAccess
{
    /// <summary>
    /// Ответственнен за обмен данными с Grpc сервером
    /// </summary>
    internal class RectangleDataProvider : IDisposable
    {
        #region Private Fields
        private bool _disposedValue;

        private GrpcChannel _channel;
        private ObservableCollection<RectangleModel> _collection;
        private CancellationTokenSource _cts;
        #endregion

        #region Public constructions
        public RectangleDataProvider()
        {
            _channel = GrpcChannel.ForAddress("https://localhost:5001");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Инициирует токен отмены
        /// </summary>
        public void GenerateCancelationToken() => _cts.Cancel();

        public async Task<RectanglePoint> GetNewCoordinate(int id)
        {
            var client = new ScreenSaverClient(_channel);

            return client.GetNewPosition(new IdRequestMessage() { Id = id });
        }

        /// <summary>
        /// Обращается к gRPC серверу за размерами поля для передвежения прямоугольников и
        /// создает его.
        /// </summary>
        /// <returns></returns>
        public async Task<RectangleSize> InitilazeCanvasBoundariesAsync()
        {
            var client = new ScreenSaverClient(_channel);

            CanvasBoundariesMessage reply = await client.GetCanvasBoundariesAsync(new Empty());

            return new RectangleSize() { Width = reply.Width, Height = reply.Height };
        }

        public async Task GetRectangleList(ObservableCollection<RectangleModel> rectangleCollection)
        {
            var client = new ScreenSaverClient(_channel);
            _cts = new();
            using AsyncServerStreamingCall<RectangleModelMessage> call = client.GetRectangleList(new Empty());

            while (await call.ResponseStream.MoveNext())
            {
                RectangleModelMessage reply = call.ResponseStream.Current;

                rectangleCollection.Add(new RectangleModel()
                {
                    Coordinate = reply.Coordinate,
                    Size = reply.Size,
                    Id = reply.Id
                });

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        #endregion

        #region IDisposable contract implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _channel.ShutdownAsync();
                }

                _disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);

        ~RectangleDataProvider()
        {
            Dispose(true);
        }
        #endregion;
    }
}
