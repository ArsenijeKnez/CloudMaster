using Common.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ITransactionCoordinator : IService
    {
        Task<List<BookDTO>> ListAvailableItems();

        Task<bool> EnlistPurchase(int cientID, int bookId, int quantity);

        Task<List<ClientDTO>> ListClients();

        Task<List<TransferDTO>> GetTransfers();

        Task<List<PurchaseDTO>> GetPurchases();

    }
}
