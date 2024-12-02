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

        public async Task<bool> EnlistPurchase(int clientId, int bookId, int count)
        {
            if (bookId <= 0 || count <= 0)
            {
                Console.WriteLine("Invalid parameters: bookId and count must be greater than 0.");
                return false;
            }

            ITransactionCoordinator transactionCoordinatorProxy = ServiceProxy.Create<ITransactionCoordinator>(new Uri(transactionCoordinatorPath));

            try
            {
                return await transactionCoordinatorProxy.EnlistPurchase(clientId, bookId, count);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in EnlistPurchase: {e.Message}");
                return false;
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


        public async Task<List<PurchaseDTO>> SeePurchases()
        {
            ITransactionCoordinator transactionCoordinatorProxy = ServiceProxy.Create<ITransactionCoordinator>(new Uri(transactionCoordinatorPath));

            try
            {
                return await transactionCoordinatorProxy.GetPurchases();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in CommitPurchases: {e.Message}");
                return null!;
            }
        }

        public async Task<List<TransferDTO>> SeeTransfers()
        {
            ITransactionCoordinator transactionCoordinatorProxy = ServiceProxy.Create<ITransactionCoordinator>(new Uri(transactionCoordinatorPath));

            try
            {
                return await transactionCoordinatorProxy.GetTransfers();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in PrepareTransfers: {e.Message}");
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
