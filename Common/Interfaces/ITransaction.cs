using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Dto;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interfaces
{
    public interface ITransaction: IService
    {
        Task<bool> Prepare(int id, int cientID, int bookId, int quantity, double price);
        Task<bool> Commit(int id);
        Task<bool> RollBack(int id);
    }
}
