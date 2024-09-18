using NUnit.Framework;
using OpenQA.Selenium;
using Test_Project.Utils;
using TestProject.Utils;

namespace Test_Project.Pages
{
    public class SearchPage : BasePage
    {
        public override string PageURL => $"{BaseURL}";
        public int totalPages => EnvironmentHelper.SafeParse("NumberofPages", 120);

        public IWebElement SearchField => driver.FindElementWhenVisible(() => By.XPath("//input[@class='Pke_EE']"));
        public IWebElement SearchIcon => driver.FindElementWhenVisible(() => By.XPath("//button[@class='_2iLD__']"));
        public IWebElement SecondPageButton => driver.FindElementWhenVisible(() => By.XPath("//a[contains(text(),'2') and @class='cn++Ap']"));
        public IWebElement NextButton => driver.FindElementWhenVisible(() => By.XPath("//span[normalize-space()='Next']"));
        public IWebElement CartButton => driver.FindElementWhenVisible(() => By.ClassName("_9Wy27C"));
        public IWebElement AddToCartButton => driver.FindElementWhenVisible(() => By.ClassName("NwyjNT"));
        


        //Methods

        public void ClickOnSearchItemBox() //Click in Search Item Box
        {
            SearchField.Click();
        }

        public void EnterSearchParameter() //Enter Search Parameter to search Item
        {
            EnvironmentHelper = EnvironmentConfigUtils.GetEnvironmentForConfig();
            string searchParameter = EnvironmentHelper.GetSetting("SearchItem");
            SearchField.SendKeys(searchParameter);

        }

        public void ClickOnSearchIcon() // Click on Search Icon
        {
            SearchIcon.Click();
        }

        public void ClickOnSortByParameter() //Click on Sort by using parameter from Data source
        {
            EnvironmentHelper = EnvironmentConfigUtils.GetEnvironmentForConfig();
            string sortParameter = EnvironmentHelper.GetSetting("SortBy");
            driver.FindElementWhenVisible(() => By.XPath($"//div[contains(text(), '{sortParameter}')]")).Click();
            WaitSeconds(5000);
        }

        public bool IsPricelistSorted() // Get the list of prices and check if list is sorted or not
        {

            List<decimal> allPrices = new List<decimal>();

            for (int currentPage = 1; currentPage <= totalPages; currentPage++)
            {
                // Find all elements with the class "Nx9bqj" for the current page
                IList<IWebElement> priceElements = driver.FindElements(By.ClassName("Nx9bqj"));

                // Loop through each element and extract the price
                foreach (IWebElement element in priceElements)
                {
                    // Get the text, remove the currency symbol, and parse the value to decimal
                    string priceText = element.Text.Replace("₹", "").Trim();
                    if (decimal.TryParse(priceText, out decimal price))
                    {
                        allPrices.Add(price);
                    }
                }

                // Check if it's not the last page, and navigate to the next page
                if (currentPage < totalPages)
                {
                    try
                    {
                        // Next Button to click for pagination                       
                        NextButton.Click();
                        WaitSeconds(5000);
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("Next button not found. Stopping at page " + currentPage);
                        break;
                    }
                }
            }

            // Print the list of all prices from all pages
            Console.WriteLine("List of Prices from all pages:");
            foreach (var price in allPrices)
            {
                Console.WriteLine(price);
            }

            // Check if the entire list is sorted in ascending order
            bool isSorted = allPrices.SequenceEqual(allPrices.OrderBy(p => p));

            if (isSorted)
            {
                Console.WriteLine("The price list is sorted.");
            }
            else
            {
                Console.WriteLine("The price list is NOT sorted.");
            }
            return isSorted;

        }

        public void ClickOnSecondProductIntheList()
        {
            IList<IWebElement> products = driver.FindElements(By.ClassName("Nx9bqj")); 
            products[1].Click(); // Click on the second product

            // Switch to the new tab (product details page)
            driver.SwitchTo().Window(driver.WindowHandles.Last());
            AddProductToCart();
            WaitSeconds(2000);
            // Close the tab and switch back
            driver.Close();
            driver.SwitchTo().Window(driver.WindowHandles.First());
        }

        public void ClickOnThirdProductIntheList()
        {
            IList<IWebElement> products = driver.FindElements(By.ClassName("Nx9bqj"));
            products[2].Click(); // Click on the third product

            // Switch to the new tab (product details page)
            driver.SwitchTo().Window(driver.WindowHandles.Last());
            AddProductToCart();
            WaitSeconds(2000);
            // Close the tab and switch back
            driver.Close();
            driver.SwitchTo().Window(driver.WindowHandles.First());
        }

        public void GoToAddToCartPage()
        {
            CartButton.Click();
            WaitSeconds(2000);
        }

        public void ValidateCorrectProductAndPricing()
        {
            WaitSeconds(2000);
            IList<IWebElement> cartProducts = driver.FindElements(By.ClassName("_8X-K8p"));
            Assert.AreEqual(2, cartProducts.Count, "Not all products are added to the cart");

        }

        public void GetPricingOfAddedProducts()
        {
            IList<IWebElement> cartProducts = driver.FindElements(By.ClassName("_8X-K8p"));
            List<decimal> productPrices = new List<decimal>();
            foreach (var product in cartProducts)
            {
                // Use XPath to find the price element using both class names
                IWebElement priceElement = product.FindElement(By.XPath(".//span[contains(@class, 'LAlF6k') and contains(@class, 're6bBo')]"));

                // Extract the price text, remove ₹ symbol, commas and any other unwanted characters
                string priceText = priceElement.Text.Replace("₹", "").Replace(",", "").Trim();

                // Parse the price into a decimal
                if (decimal.TryParse(priceText, out decimal price))
                {
                    productPrices.Add(price);
                }
            }

            // Check and print total price
            decimal totalSum = productPrices.Sum();
            Console.WriteLine($"Total Price Sum: {totalSum}");


            string cartTotalText = driver.FindElement(By.ClassName("_1Y9Lgu")).Text.Replace("₹", "").Replace(",", "").Trim();
            decimal cartTotal = Convert.ToDecimal(cartTotalText);

            // Assert that the total price of products less than the adjusted cart total
            Assert.Less(totalSum, cartTotal, "The total sum of product prices should be less than the cart total including delivery charges.");

        }

        private void AddProductToCart()
        {
            // Add the product to cart
            try
            {                
                AddToCartButton.Click();
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("Add to Cart button not found.");
            }
        }

    }
}
