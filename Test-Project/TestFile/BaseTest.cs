using AventStack.ExtentReports;
using NUnit.Framework;
using OpenQA.Selenium;
using System.Text;
using TestProject.Utils;
using Test_Project.Data;

namespace Test_Project.TestFile
{
    [TestFixture]
    public class BaseTest
    {

        public IWebDriver driver;
        private static string LoggingPath { get; set; }
        public static string status { get; set; }
        public static string message { get; set; }
        public static string stackTrace { get; set; }

        public BaseTest()
        {
            //
        }

        [SetUp]
        public void SetUp()
        {
                StringBuilder logMessage = new StringBuilder();
                string baseURL = TestData.Url;
                WindowSize browserWindowSize = new WindowSize(1280, 720);
                LogMessage(logMessage.ToString());
                Browser.Start(BrowserDriver.Chrome);
                driver = Browser.Driver;
                driver.Manage().Window.Maximize();

        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }



        public MediaEntityModelProvider captureScreenShot(IWebDriver driver, string screenShotName)
        {
            ITakesScreenshot ts = (ITakesScreenshot)driver;
            var screenshot = ts.GetScreenshot().AsBase64EncodedString;

            return MediaEntityBuilder.CreateScreenCaptureFromBase64String(screenshot, screenShotName).Build();
        }


        public static void AttatchLogToTest()
        {
            if (File.Exists(LoggingPath))
            {
                TestContext.AddTestAttachment(LoggingPath);
                TestContext.Write("Test Failed on the this URL " + Browser.Driver.Url);
            }
        }

        public static void LogMessage(string message) => WebDriverExtensions.LogMessage(message);
    }
}
