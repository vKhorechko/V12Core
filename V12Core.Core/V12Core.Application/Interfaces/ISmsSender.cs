using System.Threading.Tasks;

namespace V12Core.Application.Interfaces
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
