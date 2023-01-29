using System.Threading;
using System.Threading.Tasks;

namespace ScreenSaverServer.BussinessLogic.Interfaces
{
    public interface IPathGenerator
    {
        Task StartAsync(CancellationToken token);
    }
}
