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
    public interface ITransaction<T>: IService
    {
        Task<List<T>> Prepare();
        Task<List<T>> Commit();
        Task RollBack();
    }
}
