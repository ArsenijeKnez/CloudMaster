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

        Task<bool> EnlistPurchase(int clientId, int bookId, int count);

        Task<List<ClientDTO>> ListClients();

        Task<List<PurchaseDTO>> SeePurchases();

        Task<List<TransferDTO>> SeeTransfers();

    }
}
