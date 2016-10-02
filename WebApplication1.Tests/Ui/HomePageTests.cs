using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebApplication1.Web.Models;

namespace WebApplication1.Tests.Ui
{
    /// <summary>
    /// Summary description for WebApplicationUiTests
    /// </summary>
    [TestClass]
    public class HomePageTests
    {
        private static IWebDriver _driver;

        public TestContext TestContext { get; set; }

        #region Additional test attributes        
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            // Use Chrome WebDriver.
            var chromeOptions = new ChromeOptions();

            var args = new[]
            {
                "window-position=10,10",
                "window-size=768,1024",
                "disable-plugins",
                "disable-extensions"
            };

            chromeOptions.AddArguments(args);

            // Create the WebDriver.
            _driver = new ChromeDriver(chromeOptions);

            _driver.Manage().Cookies.DeleteAllCookies();
        }
        
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            _driver?.Quit();
        }
        #endregion

        //[Ignore]
        [TestMethod]
        public void VerifyWebDriver()
        {
            var page = new
            {
                Url = "https://www.google.com",
                SearchBoxName = "q",
                SearchString = "cheese"
            };

            // Load a new web page in the current browser window.
            _driver.Url = page.Url;

            // Find the search box and query for 'cheese'.
            var element = _driver.FindElement(By.Name(page.SearchBoxName));
            element.SendKeys(page.SearchString);
            element.SendKeys(Keys.Enter);

            // Wait for search result. Throws timeout exception if not found.
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(c => c.Title.Contains(page.SearchString));

            // Prefer using StringAssert for conatains, matches etc.
            StringAssert.Contains(_driver.Title, page.SearchString);   
        }

        [TestMethod]
        public void HomePageAddTransactionVisibleOnPage()
        {
            var newTransaction = new Transaction() { Description = "TEST 01", Amount = 42 };

            var page = new
            {
                Url = "http://localhost:58733",
                Title = "Transactions App",
                DescriptionId = "description",
                AmountId = "amount",
                AddBtn = "input[value='Add']",
                TransactionsId = "transactions",
                TransactionsSelector = "#transactions li",
            };

            // Load home page.
            _driver.Url = page.Url;
            Assert.IsTrue(_driver.Title == page.Title, "Home page not found.");

            // Enter transaction data.
            var description = _driver.FindElement(By.Id(page.DescriptionId));
            description.SendKeys(newTransaction.Description);
            var amount = _driver.FindElement(By.Id(page.AmountId));
            amount.SendKeys(newTransaction.Amount.ToString());
            var addBtn = _driver.FindElement(By.CssSelector(page.AddBtn));

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

            wait.Until(c => c.FindElement(By.Id(page.TransactionsId)).Text != string.Empty);
            var beforeCount = _driver.FindElements(By.CssSelector(page.TransactionsSelector)).Count;
            
            // Add the new transaction.
            addBtn.Click();

            // Verify transaction appears in list.
            wait.Until(c => c.FindElements(By.CssSelector(page.TransactionsSelector)).Count > beforeCount);
            var transactionsList = _driver.FindElements(By.CssSelector(page.TransactionsSelector));
            var afterCount = transactionsList.Count;

            Assert.AreEqual(beforeCount + 1, afterCount, "New transaction not found in transactions list.");
            StringAssert.Contains(transactionsList[beforeCount].Text, newTransaction.Description);
            StringAssert.Contains(transactionsList[beforeCount].Text, newTransaction.Amount.ToString());
        }

        [TestMethod]
        public void WebApi_GetWithId_ReturnsJsonForTransaction()
        {
            var data = new
            {
                id = 1,
                expectedJson = JsonConvert.SerializeObject(new { Id = 1, Description = "DEF", Amount = 50 }),
                url = "http://localhost:58733/api/Transactions/1"
            };

            // Load URL.
            _driver.Url = data.url;
        }

        [TestMethod]
        public void WebApi_GetWithNoId_ReturnsJsonForAllTransactions()
        {
            var data = new
            {
                expectedCount = 5,
                url = "http://localhost:58733/api/Transactions"
            };

            // Load URL.
            _driver.Url = data.url;
        }
    }
}
