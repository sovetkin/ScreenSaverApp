using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
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
    class RectangleDataProvider : IDisposable
    {
        #region Private Fields
        private GrpcChannel _channel;
        private ObservableCollection<RectangleModel> _collection;
        private CancellationTokenSource _cts = new();
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
        public void GenerateCancelationToken()
        {
            _cts.Cancel();
            if (_collection is not null)
                _collection = new();
        }

        /// <summary>
        /// Инициализирует стриминг с gRPC сервером
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public async Task StartDataStreamingAsync(int rectangleCount, ObservableCollection<RectangleModel> collection)
        {
            _collection = collection;

            List<Task> tasks = new();

            for (int i = 0; i < rectangleCount; i++)
            {
                Task task = GetCurrentRectanglePosition();

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
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

        /// <summary>
        /// Обращается к gRPC серверу за совойствами прямоугольника
        /// </summary>
        /// <returns></returns>
        public async Task<RectangleModelMessage> GetNewRectangle()
        {
            var client = new ScreenSaverClient(_channel);

            return await client.GetRectangleAsync(new Empty());
        }

        /// <summary>
        /// Клиентская часть стриминга данных
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<RectangleModelMessage> GetRectanglePointAsync(RectangleModelMessage rectangle, [EnumeratorCancellation] CancellationToken token)
        {
            var client = new ScreenSaverClient(_channel);
            using AsyncDuplexStreamingCall<RectangleModelMessage, RectangleModelMessage> stream =
                client.DuplexRectangleStream(cancellationToken: token);

            while (!token.IsCancellationRequested)
            {
                await WriteToStreamAsync(stream.RequestStream, rectangle);

                yield return await ReadFromStreamAsync(stream.ResponseStream);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// При первом обращении создает прямоугольник.
        /// Обеспечивает обновление координат прямоугольника.
        /// </summary>
        /// <returns></returns>
        private async Task GetCurrentRectanglePosition()
        {
            Task<(int, RectangleModelMessage)> task = CreateNewRectangle();

            (int, RectangleModelMessage) result = await task;

            int index = result.Item1;
            RectangleModelMessage rectangle = result.Item2;

            await foreach (RectangleModelMessage item in GetRectanglePointAsync(rectangle, _cts.Token))
            {
                rectangle = item;
                _collection[index].Coordinate = rectangle.Coordinate;
            }
        }

        /// <summary>
        /// Обращается к gRPC серверу за координатами создаваемого прямоугольника и создает его
        /// </summary>
        /// <returns></returns>
        private async Task<(int, RectangleModelMessage)> CreateNewRectangle()
        {
            RectangleModelMessage rectangle;

            rectangle = await GetNewRectangle();

            RectangleModel model = new()
            {
                Coordinate = rectangle.Coordinate,
                Size = rectangle.Size,
                Id = rectangle.Id
            };

            _collection.Add(model);

            return (_collection.IndexOf(model), rectangle);
        }

        /// <summary>
        /// Считывает данные из стриминга
        /// </summary>
        /// <param name="responseStream"></param>
        /// <returns></returns>
        private async Task<RectangleModelMessage> ReadFromStreamAsync(IAsyncStreamReader<RectangleModelMessage> responseStream)
        {
            while (await responseStream.MoveNext())
            {
                return responseStream.Current;
            }

            return responseStream.Current;
        }

        /// <summary>
        /// записывает данные в стриминг
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        private async Task WriteToStreamAsync(IClientStreamWriter<RectangleModelMessage> requestStream, RectangleModelMessage rectangle) =>
            await requestStream.WriteAsync(rectangle); 
        #endregion

        #region IDisposable contract implementation

        public void Dispose() => _channel.ShutdownAsync();

        #endregion;
    }
}
