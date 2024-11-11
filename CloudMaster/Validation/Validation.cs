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
using Common.Dto;

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

        public async Task<List<BookDTO>> ListAvailableItems()
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

        public async Task<bool> EnlistPurchase(int bookId, int count)
        {
            if (bookId < 0 || count < 0)
            {
                return false;
            }

            try
            {
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<double> GetItemPrice(int bookId)
        {
            if (bookId < 0)
            {
                return -1;
            }

            try
            {
                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public async Task<List<ClientDTO>> ListClients()
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

        public async Task<bool> EnlistMoneyTransfer(int userSend, int userReceive, double amount)
        {
            if (userSend < 0 || userReceive < 0 || amount < 0)
            {
                return false;
            }


            try
            {
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #endregion

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }
    }
}
