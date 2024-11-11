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
    [ServiceContract]
    public interface IBookstoreService : IService, ITransaction
    {
        [OperationContract]
        Task<List<BookDTO>> ListAvailableItems();
        [OperationContract]
        Task<bool> EnlistPurchase(int bookId, int count);
        [OperationContract]
        Task<double> GetItemPrice(int bookId);

    }
}
