using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace TestProject.Utils
{
    public static class WebElementExtensions
    {
        public static bool IsElementVisibleAndEnabled(this IWebElement webElement)
        {
            return webElement.Displayed && webElement.Enabled;
        }

        public static bool DoesElementHaveClass(this IWebElement webElement, string className)
        {
            return webElement.GetAttribute("class").Split(' ').Contains(className);
        }

        public static void SelectByText(IWebElement element, string value)
        {
            SelectElement select = new SelectElement(element);
            select.SelectByText(value);
        }

        public static double ReadDoubleValueFromString(string value)
        {
            return Convert.ToDouble(Regex.Match(value.Trim(), @"\d+\.*\d*").Value);
        }
    }
}