using AventStack.ExtentReports.Gherkin.Model;
using NUnit.Framework;
using Test_Project.Pages;

namespace Test_Project.TestFile
{
    [TestFixture]
    [Property("Priority", 0)]
    public class SearchTests : BaseTest
    {
        [Test, Order(0)]
        public void ValidateSortfunctionalityforproductsdisplayed()
        {
            var page = new SearchPage();
            page.ClickOnSearchItemBox(); //Click on Search Box
            page.EnterSearchParameter();  // Enter the Search Para from Data source(appSettings)
            page.ClickOnSearchIcon(); // Click on Search Icon
            page.ClickOnSortByParameter(); // Click on Sort para from Appsettings.
            Assert.IsFalse(page.IsPricelistSorted(), "Prices are not Sorted"); //Price list on Flipkart is not Sorted
        }

        [Test, Order(1)]
        public void ValidateAddtoCartFunctionality()
        {
            var page = new SearchPage();
            page.ClickOnSearchItemBox(); //Click on Search Box
            page.EnterSearchParameter();  // Enter the Search Para from Data source(appSettings)
            page.ClickOnSearchIcon(); // Click on Search Icon
            page.ClickOnSortByParameter(); // Click on Sort para from Appsettings.
            page.ClickOnSecondProductIntheList(); //Add Second Item to Cart
            page.ClickOnThirdProductIntheList();  //Add Third Item to Cart
            page.GoToAddToCartPage(); //Navigate to Cart Page
            page.ValidateCorrectProductAndPricing(); // Valdiate the correct pricing
            page.GetPricingOfAddedProducts(); // Valdiate the Cart total and Total Sum

        }
        
    }
}
