using Microsoft.Extensions.Logging;

namespace Vecc.Dns.Example
{
    public class LoggingExtensionsLogger : ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public LoggingExtensionsLogger(ILogger<LoggingExtensionsLogger> logger)
        {
            _logger = logger;
        }

        public void LogError(string message, params object[] values)
        {
            _logger.LogError(message, values);
        }

        public void LogError(Exception ex, string message, params object[] values)
        {
            _logger.LogError(ex, message, values);
        }

        public void LogFatal(Exception ex, string message, params object?[] values)
        {
            _logger.LogCritical(ex, message, values);
        }

        public void LogInformation(string message, params object[] values)
        {
            _logger.LogInformation(message, values);
        }

        public void LogVerbose(string message, params object[] values)
        {
            _logger.LogTrace(message, values);
        }

        public void LogWarning(string message, params object?[] values)
        {
            _logger.LogWarning(message, values);
        }
    }
}
