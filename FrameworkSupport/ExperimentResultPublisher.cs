using GitHub;
using NLog;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkSupport
{
    public class ExperimentResultPublisher : IResultPublisher
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Task Publish<T, TClean>(Result<T, TClean> result)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Publishing results for experiment '{result.ExperimentName}'");
            sb.AppendLine($"Result: {(result.Matched ? "MATCH" : "MISMATCH")}");

            foreach (var item in result.Contexts)
            {
                sb.AppendLine($"Context: {item.Key} = {item.Value}");
            }

            sb.AppendLine($"Control value: {result.Control.Value}");
            sb.AppendLine($"Control duration: {result.Control.Duration}");

            foreach (var observation in result.Candidates)
            {
                sb.AppendLine($"Candidate name: {observation.Name}");
                sb.AppendLine($"Candidate value: {observation.Value}");
                sb.AppendLine($"Candidate duration: {observation.Duration}");
            }

            if (result.Mismatched)
            {
                // saved mismatched experiments to DB
                //DbHelpers.SaveExperimentResults(result);
            }

            logger.Info(sb.ToString());

            return Task.FromResult(0);
        }
    }
}