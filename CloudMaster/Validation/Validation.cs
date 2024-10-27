using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Common.Interfaces;

namespace Validation
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>

    internal sealed class Validation : StatelessService, IValidation
    {

        public Validation(StatelessServiceContext context)
            : base(context)
        { }

        #region IValidationImplementation

        public async Task<List<string>> ListAvailableItems()
        {
    
            try
            {
                return null;
            }
            catch (Exception e)
            {
                return null!;
            }
        }

        public async Task<string> EnlistPurchase(long? bookId, uint? count)
        {
            if (bookId is null || count is null)
            {
                return null!;
            }

            try
            {
                return null;
            }
            catch (Exception e)
            {
                return null!;
            }
        }

        public async Task<string> GetItemPrice(long? bookId)
        {
            if (bookId is null)
            {
                return null!;
            }

            try
            {
                return null;
            }
            catch (Exception e)
            {
                return null!;
            }
        }

        public async Task<List<string>> ListClients()
        {
            try
            {
                //return await bankProxy.ListClients();
                return null;

            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<string> EnlistMoneyTransfer(long? userSend, long? userReceive, double? amount)
        {
            if (userSend is null || userReceive is null || amount is null)
            {
                return null;
            }


            try
            {
                return null;
            }
            catch (Exception e)
            {
                return null!;
            }
        }

        #endregion

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }
    }
}
