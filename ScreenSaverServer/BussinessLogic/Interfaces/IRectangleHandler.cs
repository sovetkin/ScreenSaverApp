using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Grpc.Core;

using ScreenSaver;

namespace ScreenSaverServer.BussinessLogic
{
    public interface IRectangleHandler
    {
        Task<RectangleModelMessage> GetInitialRectangleAsync();
        RectanglePoint GetRectangleCurrentPosition(int index);
        Task StartPathGeneration(CancellationToken token);
        Task SendResponseMessageAsync(RectangleModelMessage current, IServerStreamWriter<RectangleModelMessage> responseStream);
    }
}