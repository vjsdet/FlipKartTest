using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FrameworkSupport
{
    /// <summary>
    /// Helper to simulate errors in various places of the code based on if the helper contains a specific magic string
    /// </summary>
    /// <remarks>
    /// For example, lets say you have a function with many codepaths, and you want to simulate what happens if there's an
    /// error in a specific place.  A good example is if during checkout the server has an error inserting into the db (e.g. fatal error selling seats).
    ///
    /// To simulate this, you could for example add a special string to the order notes and have the
    /// checkout procedure use this class to help it generate a sample exception right at that exact place.
    ///
    /// Order note:  fgen=nliven-db-fail
    ///
    /// In code right before saving an order: fgen.ExecuteScenarioIfSpecified("nliven-db-fail", () => throw new Exception("fgen: simulating Nliven db nlvn_CheckOut failure"));
    /// </remarks>
    public class QAFailureGenerator
    {
        private Action<string> _logger = null;
        private List<string> _scenariosToTest = null;

        public static readonly string FGEN_TOKEN = "fgen";

        /// <summary>
        /// Create the instance with the list of active scenario names
        /// </summary>
        /// <param name="logger">A logger to write to</param>
        /// <param name="activeScenarios">A list of scenario names</param>
        public QAFailureGenerator(Action<string> logger, List<string> activeScenarios)
        {
            _logger = logger;
            _scenariosToTest = activeScenarios.Select(s => s?.ToLower()).ToList();
            _logger($"Started up with active scenarios: {string.Join(", ", activeScenarios.Select(s => $"'{s}'"))}");
        }

        /// <summary>
        /// If the scenario 'name' was specified for this class instance, call function 'action'.  Otherwise, do nothing.
        /// </summary>
        /// <param name="name">The name to test for</param>
        /// <param name="action">The function to call if name is found</param>
        public void ExecuteScenarioIfSpecified(string name, Action action)
        {
            if (_scenariosToTest.Contains(name?.ToLower()))
            {
                _logger($"Executing QA failure scenario: {name}");
                action();
            }
        }

        /// <summary>
        /// Return true if scenario 'name' was specified for this class instance.
        /// </summary>
        /// <param name="name">The name to test for</param>
        /// <returns>True if name was specified</returns>
        public bool HaveScenario(string name)
        {
            return _scenariosToTest.Contains(name?.ToLower());
        }

        /// <summary>
        /// Scan the given string for fgen={name} scenario names
        /// </summary>
        /// <param name="inputText">The text to scan</param>
        /// <returns>A list of scenario names found if any</returns>
        public static IEnumerable<string> ScanTextForScenarios(string inputText)
        {
            var ms = Regex.Matches(inputText ?? "", $"{FGEN_TOKEN}=(.*)", RegexOptions.IgnoreCase & RegexOptions.Multiline);
            foreach (Match m in ms)
            {
                string scenario = m.Groups.Count == 2 ? m.Groups[1].Value : "[err]";
                yield return scenario;
            }
        }
    }
}