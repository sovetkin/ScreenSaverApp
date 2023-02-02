using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using ScreenSaver;

using ScreenSaverServer.BussinessLogic.Models;

namespace ScreenSaverServer.BussinessLogic
{
    public interface IRectangleHandler
    {
        CanvasBoundariesMessage BordersSize { get; }
        Task<RectanglePoint> GetRectangleCurrentPosition(int index);
        Task StartPathGeneration(CancellationToken token);
        Task GenerateRepository();
        IAsyncEnumerable<RectangleModelMessage> RectangleStrem();
    }
}