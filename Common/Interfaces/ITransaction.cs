using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interfaces
{
    public interface ITransaction: IService
    {
        Task<bool> Prepare();
        Task Commit();
        Task RollBack();
    }
}
