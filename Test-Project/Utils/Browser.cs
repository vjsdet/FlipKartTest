using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;


namespace TestProject.Utils
{
    public enum BrowserDriver
    {
        Chrome,
        Firefox,
        Edge,
        Safari
    }

    public static class Browser
    {
        private static IWebDriver _driver;
        public static string _preloader = null;
        public static string downloadDirectory { get; set; }

        public static IWebDriver Driver
        {
            get { return _driver; }
        }

        public static void Start(BrowserDriver driver = BrowserDriver.Chrome, WindowSize windowSize = null)
        {
            switch (driver)
            {
                case BrowserDriver.Chrome:

                    ChromeOptions chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("--incognito");
                    chromeOptions.AddArgument("test-type");
                    chromeOptions.AddArgument("--ignore-certificate-errors");

                    _driver = new ChromeDriver(chromeOptions);
                    
                    break;

                case BrowserDriver.Edge:

                    EdgeOptions edgeOptions = new EdgeOptions();
                    edgeOptions.AddArgument("--allow-incognito");
                    edgeOptions.AddArgument("test-type");

                    var timestamp1 = DateTime.Now.ToFileTime();
                    downloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeleniumDownload", timestamp1.ToString());
                    Directory.CreateDirectory(downloadDirectory);

                    edgeOptions.AddUserProfilePreference("download.default_directory", downloadDirectory);
                    edgeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                    edgeOptions.AddUserProfilePreference("disable-popup-blocking", "true");

                    _driver = new EdgeDriver(edgeOptions);

                    if (windowSize != null)
                    {
                        _driver.Manage().Window.Size = new System.Drawing.Size(windowSize.Width, windowSize.Height);
                    }
                    else
                    {
                        _driver.Manage().Window.Maximize();
                    }
                    break;

                // TODO: Add support for other Browsers
                case BrowserDriver.Firefox:

                case BrowserDriver.Safari:
                    throw new NotImplementedException();
            }
        }
    }
}