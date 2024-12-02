using Common.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IBookstoreService : ITransaction
    {
        Task<List<BookDTO>> ListAvailableItems();
        Task<double> GetItemPrice(int bookId);
        Task<List<PurchaseDTO>> GetPurchases();

    }
}
