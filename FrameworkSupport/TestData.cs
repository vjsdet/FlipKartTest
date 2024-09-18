using System.Collections.Generic;

namespace FrameworkSupport
{
    public class TestData
    {
        public string TestSuiteName { get; set; }
        public string TestRunName { get; set; }
        public string TestCaseName { get; set; }
        public string TestCaseStatus { get; set; }
        public string TestCaseVideoURL { get; set; }
        public string TestSuiteStartDateTime { get; set; }
        public string TestSuiteEndDateTime { get; set; }
        public string TestRunStartDateTime { get; set; }
        public string TestRunEndDateTime { get; set; }
        public string TestCaseSteps { get; set; }
        public string TesterName { get; set; }
        public string TestEnvironment { get; set; }
        //public string TestFeature { get; set; }
    }

    public class TestStepColumns
    {
        public string Status { get; set; }
        public string Timestamp { get; set; }
        public string Details { get; set; }
        public string FailureMessage { get; set; }
        public string FailureException { get; set; }
        public string FailureScreenShots { get; set; }
    }

    public static class TestDataSharedInstance
    {
        public static TestData testData { get; } = new TestData();
    }

    public static class TestCaseStepsInstance
    {
        public static List<TestStepColumns> TestSteps { get; } = new List<TestStepColumns>();
    }
}