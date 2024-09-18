using NLog;
using System;

namespace FrameworkSupport
{
    /// <summary>
    /// Middleware allowing logging a series of messages through an intermediate string processor
    /// </summary>
    public class ThreadedLogger
    {
        private Logger _logger = null;
        private Func<string, string> _messageProcessor = null;

        public ThreadedLogger()
        { }

        public ThreadedLogger(Logger logger, Func<string, string> messageProcessor)
        {
            _logger = logger;
            _messageProcessor = messageProcessor;
        }

        public void Trace(string msg) => _logger.Trace(_messageProcessor(msg));

        public void Debug(string msg) => _logger.Debug(_messageProcessor(msg));

        public virtual void Info(string msg) => _logger.Info(_messageProcessor(msg));

        public void Warn(string msg) => _logger.Warn(_messageProcessor(msg));

        public void Warn(Exception ex, string msg) => _logger.Warn(ex, _messageProcessor(msg));

        public virtual void Error(Exception ex, string msg) => _logger.Error(ex, _messageProcessor(msg));

        public virtual void Error(string msg) => _logger.Error(_messageProcessor(msg));

        public void Fatal(string msg) => _logger.Fatal(_messageProcessor(msg));
    }
}