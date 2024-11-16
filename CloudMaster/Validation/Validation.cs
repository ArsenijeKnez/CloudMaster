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
using Microsoft.ServiceFabric.Services.Client;

namespace Validation
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>

    internal sealed class Validation : StatelessService, IValidation
    {
        private readonly string transactionCoordinatorPath = @"fabric:/CloudMaster/TransactionCoordinator";

        public Validation(StatelessServiceContext context)
            : base(context)
        { }

        #region IValidationImplementation

        public async Task<List<BookDTO>> ListAvailableItems()
        {
            ITransactionCoordinator transactionCoordinatorProxy = ServiceProxy.Create<ITransactionCoordinator>(new Uri(transactionCoordinatorPath));

            try
            {
                return await transactionCoordinatorProxy.ListAvailableItems();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in ListAvailableItems: {e.Message}");
                return null!;
            }
        }

        public async Task<bool> EnlistPurchase(int bookId, int count)
        {
            if (bookId <= 0 || count <= 0)
            {
                Console.WriteLine("Invalid parameters: bookId and count must be greater than 0.");
                return false;
            }

            ITransactionCoordinator transactionCoordinatorProxy = ServiceProxy.Create<ITransactionCoordinator>(new Uri(transactionCoordinatorPath));

            try
            {
                return await transactionCoordinatorProxy.EnlistPurchase(bookId, count);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in EnlistPurchase: {e.Message}");
                return false;
            }
        }

        public async Task<double> GetItemPrice(int bookId)
        {
            if (bookId <= 0)
            {
                Console.WriteLine("Invalid parameter: bookId must be greater than 0.");
                return -1;
            }

            ITransactionCoordinator transactionCoordinatorProxy = ServiceProxy.Create<ITransactionCoordinator>(new Uri(transactionCoordinatorPath));

            try
            {
                return await transactionCoordinatorProxy.GetItemPrice(bookId);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in GetItemPrice: {e.Message}");
                return -1;
            }
        }

        public async Task<List<ClientDTO>> ListClients()
        {
            ITransactionCoordinator transactionCoordinatorProxy = ServiceProxy.Create<ITransactionCoordinator>(new Uri(transactionCoordinatorPath));

            try
            {
                return await transactionCoordinatorProxy.ListClients();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in ListClients: {e.Message}");
                return null;
            }
        }

        public async Task<bool> EnlistMoneyTransfer(int userSend, int userReceive, double amount)
        {
            if (userSend <= 0 || userReceive <= 0 || amount <= 0)
            {
                Console.WriteLine("Invalid parameters: userSend, userReceive, and amount must be greater than 0.");
                return false;
            }

            ITransactionCoordinator transactionCoordinatorProxy = ServiceProxy.Create<ITransactionCoordinator>(new Uri(transactionCoordinatorPath));

            try
            {
                return await transactionCoordinatorProxy.EnlistMoneyTransfer(userSend, userReceive, amount);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in EnlistMoneyTransfer: {e.Message}");
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
