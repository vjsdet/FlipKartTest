using NLog;
using System;
using System.Net.Http;
using System.Threading;

namespace FrameworkSupport
{
    public class ForeverEndpointCaller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private string _descriptiveName = "[untitled]";

        public ForeverEndpointCaller(string descriptiveName)
        {
            _descriptiveName = descriptiveName;
        }

        public Func<TimeSpan> PeriodicInterval { get; set; }

        public Func<string> EndpointUri { get; set; }

        public TimeSpan? ClientTimeout { get; set; } = null;

        public Action<string> PostProcessResult { get; set; }

        public Action<Exception, string> ErrorResult { get; set; }

        public static TimeSpan IntervalFromSeconds(int seconds)
        {
            return new TimeSpan(0, 0, seconds);
        }

        public Func<bool> IsStillLive { get; set; }

        public string ApiKey { get; set; } = null;

        public void StartAfterInterval(TimeSpan firstIntervalToWait)
        {
            logger.Info($"[{_descriptiveName}] Starting endpoint caller, waiting {firstIntervalToWait}");

            Thread.Sleep(firstIntervalToWait);
            Start();
        }

        public void Start()
        {
            logger.Info($"[{_descriptiveName}] Starting endpoint caller...");

            while (true)
            {
                if (!IsStillLive())
                {
                    var defaultWaitInterval = new TimeSpan(0, 0, 15);
                    logger.Info($"[{_descriptiveName}] Job is not live anymore. Sleeping for interval {defaultWaitInterval}");
                    Thread.Sleep(defaultWaitInterval);
                    continue;
                }

                try
                {
                    logger.Debug($"[{_descriptiveName}] Calling endpoint: {EndpointUri()}");

                    using (var handler = new HttpClientHandler())
                    {
                        handler.ServerCertificateCustomValidationCallback = (m, c, ch, p) => true;
                        using (var client = new HttpClient(handler))
                        {
                            if (ApiKey != null)
                            {
                                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
                            }

                            if (ClientTimeout != null)
                            {
                                client.Timeout = (TimeSpan)ClientTimeout;
                            }

                            string response = client.GetStringAsync(EndpointUri()).Result;
                            client.Dispose();

                            if (string.IsNullOrWhiteSpace(response))
                            {
                                response = "[none]";
                            }

                            logger.Info($"[{_descriptiveName}] Completed call with result: {response}");
                            PostProcessResult?.Invoke(response);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorDetails = $"[{_descriptiveName}] Error in endpoint caller to endpoint: {EndpointUri()}";
                    logger.Error(ex, errorDetails);
                    ErrorResult?.Invoke(ex, errorDetails);
                }

                TimeSpan waitInterval = PeriodicInterval();
                logger.Debug($"[{_descriptiveName}] Sleeping for interval {waitInterval}");
                Thread.Sleep(waitInterval);
            }
        }
    }
}