using System.Net.Sockets;

namespace Vecc.Dns
{
    public interface ILogger
    {
        void LogError(Exception ex, string message, params object?[] values);
        void LogFatal(Exception ex, string message, params object?[] values);
        void LogInformation(string message, params object?[] values);
        void LogWarning(string message, params object?[] values);
        void LogVerbose(string message, params object?[] values);
        void LogError(string message, params object?[] values);
    }
}
