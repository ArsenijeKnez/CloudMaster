using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Dto;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interfaces
{
    public interface IValidation : IService
    {
        Task<List<BookDTO>> ListAvailableItems();

        Task<bool> EnlistPurchase(int bookId, int count);

        Task<double> GetItemPrice(int bookId);

        Task<List<ClientDTO>> ListClients();

        Task<bool> EnlistMoneyTransfer(int userSend, int userReceive, double amount);

        Task<List<ITransactionDTO>> PreparePurchases();

        Task<List<ITransactionDTO>> CommitPurchases();

        Task<List<ITransactionDTO>> PrepareTransfers();

        Task<List<ITransactionDTO>> CommitTransfers();

        Task<bool> RollbackPurchases();

        Task<bool> RollbackTransfers();
    }
}
