using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplication1.Web.Controllers;
using WebApplication1.Web.Models;

namespace WebApplication1.Tests.Integration
{
    /// <summary>
    /// Represents integration tests for the WebApplication Web API.
    /// </summary>
    [TestClass]
    public class TransactionsControllerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public async Task GetAllTransactions()
        {
            var tc = new TransactionsController();
            var result = await tc.GetAllTransactions();

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count(), "Incorrect transaction count.");
        }

        [TestMethod()]
        public void GetValidId()
        {
            var expected = new Transaction { Id = 4, Description = "MNO", Amount = 200 };
            var tc = new TransactionsController();
            var result = tc.Get(expected.Id);

            Assert.IsNotNull(result);
            ValidateTransaction(expected, result);
        }

        [TestMethod()]
        [ExpectedException(typeof(HttpResponseException))]
        public void GetInvalidId()
        {
            const int id = 5;
            var tc = new TransactionsController();
            
            // Expect exception.
            var result = tc.Get(id);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod()]
        public async Task PostValidTransaction()
        {
            var newTransaction = new Transaction() { Id = 5, Description = "XYZ", Amount = 350 };
            var tc = new TransactionsController();
            var result = await tc.Post(newTransaction);

            Assert.IsNotNull(result);
            var transactions = result as Transaction[] ?? result.ToArray();
            Assert.AreEqual(6, transactions.Count(), "Incorrect transaction count.");
            ValidateTransaction(newTransaction, transactions.ElementAt(5));
        }

        [TestMethod()]
        public async Task PostTransactionEmptyDescription()
        {
            var newTransaction = new Transaction() { Description = string.Empty };
            Assert.AreEqual(string.Empty, newTransaction.Description);
            var tc = new TransactionsController();
            var result = await tc.Post(newTransaction);

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count(), "Empty transaction added.");
        }

        [TestMethod()]
        public async Task PostTransactionNullDescription()
        {
            var newTransaction = new Transaction();
            Assert.AreEqual(null, newTransaction.Description);
            var tc = new TransactionsController();
            var result = await tc.Post(newTransaction);

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count(), "Empty transaction added.");
        }

        [TestMethod()]
        public async Task PostNullTransaction()
        {
            Transaction newTransaction = null;
            var tc = new TransactionsController();
            // ReSharper disable once ExpressionIsAlwaysNull
            var result = await tc.Post(newTransaction);

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count(), "Empty transaction added.");
        }

        [TestMethod()]
        public async Task Post2Transactions()
        {
            var result = default(IEnumerable<Transaction>);
            var tc = new TransactionsController();
            var newTransactions = new List<Transaction>()
            {
                new Transaction() { Id = 5, Description = "Test 01", Amount = 350 },
                new Transaction() { Id = 6, Description = "Test 02", Amount = 500 }
            };

            foreach (var newTransaction in newTransactions)
            {
                result = await tc.Post(newTransaction);
            }

            if (result != null)
            {
                var transactions = result.ToArray();
                Assert.AreEqual(7, transactions.Count(), "Incorrect transaction count.");
                ValidateTransaction(newTransactions[1], transactions.ElementAt(6));
                PrintTransactions(transactions);
            }
        }

        private static void ValidateTransaction(Transaction expected, Transaction actual)
        {
            Assert.AreEqual(expected.Id, actual.Id, "Incorrect Id.");
            Assert.AreEqual(expected.Description, actual.Description, "Incorrect Description.");
            Assert.AreEqual(expected.Amount, actual.Amount, "Incorrect Amount.");
        }

        private void PrintTransactions(IEnumerable<Transaction> repository)
        {
            foreach (var item in repository)
            {
                TestContext.WriteLine("Id:{0}, Description:{1}, Amount:{2}", item.Id, item.Description, item.Amount);
            }
        }
    }
}
