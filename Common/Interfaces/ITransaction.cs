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
        Task<List<ITransactionDTO>> Prepare();
        Task<List<ITransactionDTO>> Commit();
        Task RollBack();
    }
}
