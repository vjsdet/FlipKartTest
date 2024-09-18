using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Text;

namespace TestProject.Utils
{
    public static class WebDriverExtensions
    {
        public const int WaitTimeoutInSeconds = 120; // TODO: Read from config

        public static void LogMessage(string message)
        {
#if DEBUG
            TestContext.Progress.WriteLine(message);
#else
            TestContext.Out.WriteLine(message);
#endif
        }

        /// <summary>
        /// Finds an element on a page when it becomes available
        /// </summary>
        /// <param name="driver">WebDriver instance</param>
        /// <param name="lazyElementGetter">A function that returns the element to test</param>
        /// <param name="timeoutInSeconds">The timeout</param>
        /// <returns>The web element</returns>

        public static IWebElement FindElementWhenVisible(this IWebDriver driver, Func<By> lazyElementGetter, int timeoutInSeconds = WaitTimeoutInSeconds)
        {
            driver.WaitUntilElementIsDisplayed(() => driver.FindElement(lazyElementGetter()), timeoutInSeconds);
            return driver.FindElement(lazyElementGetter());
        }

        public static void WaitforTextToContain(this IWebDriver driver, string match, Func<By> lazyElementGetter, int timeoutInSeconds = 20)
            => driver.WaitUntilWebElementCondition(() => driver.FindElement(lazyElementGetter()), e => e.Text.Contains(match), timeoutInSeconds);

        public static void WaitUntilVisible(this IWebDriver driver, Func<By> lazyElementGetter, int timeoutInSeconds = 30)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.VisibilityOfAllElementsLocatedBy(lazyElementGetter()));
        }

        public static void WaitUntilInvisible(this IWebDriver driver, Func<By> lazyElementGetter, int timeoutInSeconds = 20)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(lazyElementGetter()));
            }
            catch (WebDriverTimeoutException)
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(lazyElementGetter()));
            }
        }

        /// <summary>
        /// Wait for element to be clickable, then perform the click action
        /// </summary>
        /// <param name="driver">WebDriver instance</param>
        /// <param name="lazyElementGetter">A function that returns the element to test</param>
        /// <param name="timeoutInSeconds">The timeout</param>
        public static void WaitUntilElementClickableThenClick(this IWebDriver driver, Func<IWebElement> lazyElementGetter, int timeoutInSeconds = 40)
        {
            Action<string> log = s => LogMessage($"[WaitUntilElementClickableThenClick] {s}");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            wait.Until(drv =>
            {
                try
                {
                    lazyElementGetter().Click();
                    return true;
                }
                catch (ElementClickInterceptedException ex)
                {
                    log($"caught ElementClickInterceptedException error ({ex.Message})");
                    return false;
                }
                catch (StaleElementReferenceException ex)
                {
                    log($"caught StaleElementReferenceException error ({ex.Message})");
                    return false;
                }
                catch (NoSuchElementException ex)
                {
                    log($"caught NoSuchElementException error ({ex.Message})");
                    return false;
                }
                catch (ElementNotInteractableException ex)
                {
                    log($"caught ElementNotInteractableException error ({ex.Message})");
                    return false;
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    string msg = "Current theory: this can happen with elements specified with the 'FindsBy' attribute and is usually " +
                    "non-recoverable by waiting. The fix is to convert the lazyElementGetter to the " +
                    "'driver.FindElement' syntax.";
                    log($"caught TargetInvocationException error ({ex.Message})");
                    throw new Exception(msg, ex);
                }
            });
        }

        public static bool TitleContains(this IWebDriver driver, string title, int timeoutInSeconds = 20)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until((d) => { return d.Title.Contains(title); });
            }
            catch (TimeoutException)
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until((d) => { return d.Title.Contains(title); });
            }
        }

        /// <summary>
        /// Wait for element to be displayed
        /// </summary>
        /// <param name="driver">WebDriver instance</param>
        /// <param name="lazyElementGetter">A function that returns the element to test</param>
        /// <param name="timeoutInSeconds">The timeout</param>
        /// <returns>bool if its displayed within the timeout period</returns>
        /// <remarks>
        /// This function does not require the element to exist yet in the calling page - so no
        /// need for a try/catch block and additional logic around your call.
        /// </remarks>
        public static void WaitUntilElementIsDisplayed(this IWebDriver driver, Func<IWebElement> lazyElementGetter, int timeoutInSeconds = 5)
            => driver.WaitUntilWebElementCondition(lazyElementGetter, e => e.Displayed, timeoutInSeconds);

        public static void WaitUntilElementDisabled(this IWebDriver driver, Func<IWebElement> lazyElementGetter, int timeoutInSeconds = 5)
          => driver.WaitUntilWebElementCondition(lazyElementGetter, e => e.IsElementVisibleAndEnabled(), timeoutInSeconds);

        /// <summary>
        /// Function to handle waiting for an element and an arbitrary element condition
        /// </summary>
        /// <param name="driver">WebDriver instance</param>
        /// <param name="lazyElementGetter">A function that returns the element to test</param>
        /// <param name="elementChecker">An arbitrary function accepting an element that should return true when the desired element condition is met</param>
        /// <param name="timeoutInSeconds">The timeout</param>
        public static void WaitUntilWebElementCondition(this IWebDriver driver, Func<IWebElement> lazyElementGetter, Func<IWebElement, bool> elementChecker, int timeoutInSeconds = 5)
        {
            Action<string> log = s => LogMessage($"[WaitUntilWebElementCondition] {s}");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            wait.Until(drv =>
            {
                try
                {
                    log("trying to get element");
                    IWebElement elementOnPage = lazyElementGetter();
                    return elementChecker(elementOnPage);
                }
                catch (Exception e)
                {
                    log($"caught elementGetter error ({e.Message})");
                    return false;
                }
            });
        }

        /// <summary>
        /// Repeatedly call the passed condition test until it returns true
        /// </summary>
        /// <param name="driver">WebDriver instance</param>
        /// <param name="conditionTest">A function that returns true when the desired condition is satisfied.</param>
        /// <param name="timeoutInSeconds">The timeout</param>
        /// <returns>True if the condition returns true within the timeout period. Otherwise false.</returns>
        public static bool WaitForArbitraryCondition(this IWebDriver driver, Func<bool> conditionTest, int timeoutInSeconds = 30)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => conditionTest());
            }
            catch (Exception e)
            {
                LogMessage($"Exception caught: {e.Message}");
                return false;
            }
        }

        public static void JSElementClick(this IWebDriver driver, IWebElement el)
        {
            int maxTries = 3;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el);
                    break;
                }
                catch (Exception e)
                {
                    if (i == maxTries)
                    {
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// this method is used to select the values from dropdown without select class
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="by"></param>
        /// <param name="value"></param>
        public static void SelectDropdownWithoutSelect(this IWebDriver driver, List<IWebElement> elements, string value)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                string text = elements[i].Text;
                if (text.Contains(value))
                {
                    driver.WaitUntilElementClickableThenClick(() => elements[i]);
                    break;
                }
            }
        }

        public static void ScrollToElement(this IWebDriver driver, IWebElement element)
        {
            driver.ExecuteJavascript("arguments[0].scrollIntoView(true);", element);
        }

        public static void ScrollUpWindow(this IWebDriver driver, int pixels = 50)
        {
            driver.ExecuteJavascript($"window.scrollBy(0,{pixels})");
        }

        public static void MoveToElement(this IWebDriver driver, IWebElement element)
        {
            Actions builder = new Actions(driver);
            builder.MoveToElement(element).Perform();
        }

        public static void ExecuteJavascript(this IWebDriver driver, string script, params object[] parameters)
        {
            ((IJavaScriptExecutor)driver)
                .ExecuteScript(script, parameters);
        }

        public static object ExecuteJavascriptWithReturn(this IWebDriver driver, string script, params object[] parameters)
        {
            return ((IJavaScriptExecutor)driver)
                .ExecuteScript(script, parameters);
        }

        /* It can be used in future
        public static void JSSendKeys(this IWebDriver driver, IWebElement el, string value)
        {
            IJavaScriptExecutor runJS = ((IJavaScriptExecutor)driver);
            runJS.ExecuteScript("arguments[1].value = arguments[0]; ", value, el);
        }*/

        public static void ActionSendKeys(this IWebDriver driver, IWebElement el, string value)
        {
            Actions action = new Actions(driver);
            action.MoveToElement(el).SendKeys(value).Build().Perform();
        }

        public static void ActionClick(this IWebDriver driver, IWebElement el)
        {
            Actions action = new Actions(driver);
            action.MoveToElement(el).Click().Build().Perform();
        }

        public static void ActionDoubleClick(this IWebDriver driver, IWebElement el)
        {
            Actions action = new Actions(driver);
            action.MoveToElement(el).DoubleClick().Build().Perform();
        }

        public static void WaitforAttributeToContain(this IWebDriver driver, string attribute, string match, Func<IWebElement> lazyElementGetter, int timeoutInSeconds = 60)
            => driver.WaitUntilWebElementCondition(() => lazyElementGetter(), e => e.GetAttribute(attribute).Contains(match), timeoutInSeconds);

        public static void SendKeysOneAtATime(this IWebDriver driver, IWebElement el, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                string s = new StringBuilder().Append(c).ToString();
                driver.ActionSendKeys(el, s);
            }
        }

        public static string WaitForDownload(this IWebDriver driver, string fileName, int timeoutInSeconds = 30)
        {
            string path = null;
            new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds)).Until(d =>
            {
                path = CheckFile(fileName);
                return !string.IsNullOrEmpty(path);
            });
            return path;
        }

        public static string CheckFile(string fileName)
        {
            string[] files = Directory.GetFiles(Browser.downloadDirectory);
            string filePath = null;
            if (files.Count() > 0 && files.Where(x => x.Contains(fileName)).Any())
            {
                foreach (var file in files)
                {
                    if (file.Contains(fileName))
                    {
                        filePath = file;
                        break;
                    }
                }
            }
            return filePath;
        }

        public static void SwitchToWindow(this IWebDriver driver, int order = 0)
        {
            var defaultWindow = driver.WindowHandles[order];
            driver.SwitchTo().Window(defaultWindow);
        }

        public static void ScrollDownWindow(this IWebDriver driver, int pixels = 250)
        {
            driver.ExecuteJavascript($"window.scrollBy(0,{pixels})");
        }
    }
}