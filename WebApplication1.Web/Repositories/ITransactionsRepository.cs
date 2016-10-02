using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Web.Models;

namespace WebApplication1.Web.Repositories
{
    public interface ITransactionsRepository
    {
        Task<IEnumerable<Transaction>> GetAllTransactions();
        Task<Transaction> GetTransaction(int id);
        Task AddTransaction(Transaction t);
    }
}
