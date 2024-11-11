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
    public interface IBankService : IService, ITransaction
    {
        [OperationContract]
        Task<List<ClientDTO>> ListClients();
        [OperationContract]
        Task<bool> EnlistMoneyTransfer(int userSend, int userReceive, double amount);
    }
}
