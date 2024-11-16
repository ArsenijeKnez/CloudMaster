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
    public interface IBankService : ITransaction
    {
        Task<List<ClientDTO>> ListClients();
        Task<bool> EnlistMoneyTransfer(int userSend, int userReceive, double amount);
    }
}
