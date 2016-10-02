using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Web.Models;

namespace WebApplication1.Web.Repositories
{
    /// <summary>
    /// This class is simulating interacting with a database.
    /// </summary>
    public class TransactionsRepository : ITransactionsRepository
    {
        List<Transaction> transactions = new List<Transaction>()
        {
            new Transaction() { Id = 0, Description = "ABC", Amount = 100 },
            new Transaction() { Id = 1, Description = "DEF", Amount = 50 },
            new Transaction() { Id = 2, Description = "GHI", Amount = 5 },
            new Transaction() { Id = 3, Description = "JKL", Amount = 90 },
            new Transaction() { Id = 4, Description = "MNO", Amount = 200 }
        };        

        public async Task<IEnumerable<Transaction>> GetAllTransactions()
        {
            await Task.Delay(500); // simulate network IO delay
            return transactions;
        }

        public async Task<Transaction> GetTransaction(int id)
        {
            await Task.Delay(500); // simulate network IO delay
            return transactions.FirstOrDefault(x => x.Id == id);
        }

        /// <returns>If transaction was succesfully added, true; otherwise, false.</returns>
        public async Task AddTransaction(Transaction t)
        {
            t.Id = transactions.Max(x => x.Id) + 1;
            await Task.Delay(1000); // simulate network IO delay
            transactions.Add(t);
        }        
    }
}