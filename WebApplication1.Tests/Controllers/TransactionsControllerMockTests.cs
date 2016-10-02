using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApplication1.Web.Controllers;
using WebApplication1.Web.Models;
using WebApplication1.Web.Repositories;

namespace WebApplication1.Tests.Controllers
{
    [TestClass()]
    public class TransactionsControllerMockTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GetAllTransactions()
        {
            // Arrange
            var transactions = GetTransactions();
            var repository = new Mock<ITransactionsRepository>();
            repository.Setup(
                r => r.GetAllTransactions())
                .Returns(Task.FromResult<IEnumerable<Transaction>>(transactions));
            var controller = new TransactionsController(repository.Object);

            // Act
            var result = controller.GetAllTransactions();

            // Assert
            Assert.IsNotNull(result);
            repository.Verify(r => r.GetAllTransactions());
            Assert.AreEqual(transactions.Count, result.Result.Count());
        }

        [TestMethod]
        public void WhenGetByIdIsCalledUnderLyingServiceIsCalled()
        {
            var transaction = new Transaction { Id = 1, Description = "TEST 01", Amount = 99 };
            var service = new Mock<ITransactionsRepository>();
            service.Setup(
                x => x.GetTransaction(1))
                .Returns(Task.FromResult(transaction));
            var controller = new TransactionsController(service.Object);

            var result = controller.Get(1);

            Assert.AreEqual(transaction, result);
        }

        [TestMethod]
        public void WhenIdCalledByGetTransactionIsNotFound404IsThrown()
        {
            Transaction transaction = null;
            var service = new Mock<ITransactionsRepository>();
            service.Setup(
                x => x.GetTransaction(1))
                // ReSharper disable once ExpressionIsAlwaysNull
                .Returns(Task.FromResult(transaction));
            var controller = new TransactionsController(service.Object);

            try
            {
                controller.Get(1);
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, ex.Response.StatusCode);
            }
        }

        [TestMethod]
        public void GetAllTransactionsNullRepository()
        {
            // Arrange
            var transactions = default(List<Transaction>);
            var repository = new Mock<ITransactionsRepository>();
            repository.Setup(
                r => r.GetAllTransactions())
                // ReSharper disable once ExpressionIsAlwaysNull
                .Returns(Task.FromResult<IEnumerable<Transaction>>(transactions));
            var controller = new TransactionsController(repository.Object);

            // Act
            var result = controller.GetAllTransactions();

            Assert.IsNotNull(result);
            repository.VerifyAll();
            Assert.IsNull(result.Result);
        }

        [TestMethod]
        public void GetAllTransactionsEmptyRepository()
        {
            // Arrange
            var transactions = default(List<Transaction>); 
            var repository = new Mock<ITransactionsRepository>();
            repository.Setup(
                r => r.GetAllTransactions())
                // ReSharper disable once ExpressionIsAlwaysNull
                .Returns(Task.FromResult<IEnumerable<Transaction>>(transactions));
            var controller = new TransactionsController(repository.Object);

            // Act
            var result = controller.GetAllTransactions();

            // Assert
            Assert.IsNotNull(result);
            repository.Verify(r => r.GetAllTransactions());
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.AreEqual(transactions, result.Result);
        }

        [TestMethod()]
        public void GetValidInput()
        {
            // Arrange
            var id = 1;
            var transactions = GetTransactions();
            var repository = new Mock<ITransactionsRepository>();
            repository.Setup(
                r => r.GetTransaction(id))
                .Returns(Task.FromResult(transactions.FirstOrDefault(x => x.Id == id)));
            var controller = new TransactionsController(repository.Object);

            // Act
            var result = controller.Get(id);

            // Assert
            Assert.IsNotNull(result);
            repository.Verify(r => r.GetTransaction(id));
            ValidateTransaction(transactions[id], result);
        }

        [TestMethod()]
        [ExpectedException(typeof(HttpResponseException))]
        public void GetInvalidInput()
        {
            // Arrange
            var id = 5;
            var transactions = GetTransactions();
            var repository = new Mock<ITransactionsRepository>();
            repository.Setup(
                r => r.GetTransaction(id))
                .Returns(Task.FromResult(transactions.FirstOrDefault(x => x.Id == id)));
            var controller = new TransactionsController(repository.Object);

            // Act - Exception expected
            var result = controller.Get(id); // Invalid Id value 5; mock returns null.

            // Assert
            repository.Verify(r => r.GetTransaction(3), Times.Never);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        [ExpectedException(typeof(HttpResponseException))]
        public void GetEmptyRepository()
        {
            // Arrange
            var id = 0;
            var transactions = new List<Transaction>() { null };
            var repository = new Mock<ITransactionsRepository>();
            repository.Setup(
                r => r.GetTransaction(id))
                .Returns(Task.FromResult(transactions.FirstOrDefault(x => x.Id == id)));
            var controller = new TransactionsController(repository.Object);

            // Act - Exception expected.
            var result = controller.Get(id); // Repository is empty.

            // Assert
            Assert.IsNotNull(result);
            repository.Verify(r => r.GetTransaction(id), Times.Never);
        }

        [TestMethod]
        public void GetNullRepository()
        {
            // Arrange
            const int id = 0;
            var transactions = default(List<Transaction>);
            var repository = new Mock<ITransactionsRepository>();
            repository.Setup(
                r => r.GetTransaction(id))
                // ReSharper disable once AssignNullToNotNullAttribute
                .Returns(Task.FromResult(transactions.FirstOrDefault(x => x.Id == id)));
            var controller = new TransactionsController(repository.Object);

            // Act - Exception expected.
            var result = controller.Get(id); // Repository is empty.

            // Assert
            Assert.IsNotNull(result);
            repository.Verify(r => r.GetTransaction(id), Times.Never);
        }

        [TestMethod]
        public async Task PostValidInput()
        {
            // Arrange
            var newTransaction = new Transaction { Id = 0, Description = "Demo5", Amount = 599 };
            var transactions = GetTransactions();
            var repository = new Mock<ITransactionsRepository>();
            repository.Setup(
                r => r.AddTransaction(newTransaction))
                .Returns(() => Task.Run(() =>
                  {
                      newTransaction.Id = transactions.Max(x => x.Id) + 1;
                      transactions.Add(newTransaction);
                  }));
            repository.Setup(
                r => r.GetAllTransactions())
                .Returns(Task.FromResult<IEnumerable<Transaction>>(transactions));
            var controller = new TransactionsController(repository.Object);

            // Act
            var result = await controller.Post(newTransaction);

            // Assert
            repository.VerifyAll();
            var arr = result.ToArray();
            Assert.AreEqual(transactions.Count, arr.Length);
            ValidateTransaction(newTransaction, arr.ElementAt(5));
        }

        [TestMethod]
        public async Task PostEmptyRepository()
        {
            // Arrange
            var newTransaction = new Transaction { Id = 0, Description = "Demo5", Amount = 599 };
            var transactions = new List<Transaction>() { null };
            var repository = new Mock<ITransactionsRepository>();
            repository.Setup(
                r => r.AddTransaction(newTransaction))
                .Returns(() => Task.Run(() =>
                {
                    newTransaction.Id = transactions.Max(x => x.Id) + 1;
                    transactions.Add(newTransaction);
                }));
            repository.Setup(
                r => r.GetAllTransactions())
                .Returns(Task.FromResult<IEnumerable<Transaction>>(transactions));
            var controller = new TransactionsController(repository.Object);

            // Act
            var result = await controller.Post(newTransaction);

            // Assert
            repository.VerifyAll();
            var enumerable = result.ToArray();
            Assert.AreEqual(transactions.Count, enumerable.Length);
            ValidateTransaction(newTransaction, enumerable.ElementAt(5));
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
        public async Task PostNullRepository()
        {
            // Arrange
            var newTransaction = new Transaction { Id = 0, Description = "Demo5", Amount = 599 };
            var transactions = default(List<Transaction>);
            var repository = new Mock<ITransactionsRepository>();
            repository.Setup(
                r => r.AddTransaction(newTransaction))
                .Returns(() => Task.Run(() =>
                {
                    newTransaction.Id = transactions.Max(x => x.Id) + 1;
                    // ReSharper disable once PossibleNullReferenceException
                    transactions.Add(newTransaction);
                }));
            repository.Setup(
                r => r.GetAllTransactions())
                .Returns(Task.FromResult<IEnumerable<Transaction>>(transactions));
            var controller = new TransactionsController(repository.Object);

            // Act
            var result = await controller.Post(newTransaction);

            // Assert
            repository.VerifyAll();
            var enumerable = result.ToArray();
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual(transactions.Count, enumerable.Length);
            ValidateTransaction(newTransaction, enumerable.ElementAt(5));
        }

        private static List<Transaction> GetTransactions()
        {
            return new List<Transaction>()
            {
                new Transaction {Id = 0, Description = "Demo0", Amount = 0 },
                new Transaction {Id = 1, Description = "Demo1", Amount = -1 },
                new Transaction {Id = 2, Description = "Demo2", Amount = 25 },
                new Transaction {Id = 3, Description = "Demo3", Amount = 350 },
                new Transaction {Id = 4, Description = "Demo4", Amount =  42},
            };
        }

        private static void ValidateTransaction(Transaction expected, Transaction actual)
        {
            Assert.AreEqual(expected.Id, actual.Id, "Incorrect Id");
            Assert.AreEqual(expected.Description, actual.Description, "Incorrect Description");
            Assert.AreEqual(expected.Amount, actual.Amount, "Incorrect Amount");
        }
    }
}