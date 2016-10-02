using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using WebApplication1.Web.Models;
using WebApplication1.Web.Repositories;

namespace WebApplication1.Web.Controllers
{
    public class TransactionsController : ApiController
    {
        private readonly ITransactionsRepository repo;

        public TransactionsController()
        {
            repo = new TransactionsRepository();
        }

        public TransactionsController(ITransactionsRepository repo)
        {
            this.repo = repo;
        }

        /// <example>GET: api/Transactions</example>
        /// <returns>All transactions.</returns>
        public Task<IEnumerable<Transaction>> GetAllTransactions()
        {
            return repo.GetAllTransactions();
        }

        /// <example>GET: api/Transactions/5</example>
        /// <returns>The transaction with the given ID.</returns>
        public Transaction Get(int id)
        {

            var t = repo.GetTransaction(id).Result;
            if (t == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return t;
        }

        /// <example>POST: api/Transactions</example>
        /// <summary>Adds posted transaction to the data store.</summary>
        /// <returns>All transactions after adding given transaction.</returns>
        public async Task<IEnumerable<Transaction>> Post([FromBody]Transaction t)
        {
            await repo.AddTransaction(t);
            return await repo.GetAllTransactions();
        }
    }
}
