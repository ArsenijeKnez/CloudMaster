using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Common.Interfaces;
using Common.Dto;

namespace TransactionCoordinator
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class TransactionCoordinator : StatelessService, ITransactionCoordinator
    {
        private readonly string bookstorePath = @"fabric:/CloudMaster/BookstoreService";
        private readonly string bankPath = @"fabric:/CloudMaster/BankService";

        public TransactionCoordinator(StatelessServiceContext context)
            : base(context)
        { }

        #region ITransactionCoordinatorImplementation

        public async Task<List<BookDTO>> ListAvailableItems()
        {
            IBookstoreService bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

            try
            {
                return await bookstoreProxy.ListAvailableItems();
            }
            catch (Exception)
            {
                return null!;
            }
        }

        public async Task<bool> EnlistPurchase(int bookId, int count)
        {
            IBookstoreService bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

            try
            {
                return await bookstoreProxy.EnlistPurchase(bookId, count);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<double> GetItemPrice(int bookId)
        {
            IBookstoreService bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

            try
            {
                return await bookstoreProxy.GetItemPrice(bookId);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<List<ClientDTO>> ListClients()
        {
            IBankService bankProxy = ServiceProxy.Create<IBankService>(new Uri(bankPath), new ServicePartitionKey(2));

            try
            {
                return await bankProxy.ListClients();
            }
            catch (Exception)
            {
                return null!;
            }
        }

        public async Task<bool> EnlistMoneyTransfer(int userSend, int userReceive, double amount)
        {
            IBankService bankProxy = ServiceProxy.Create<IBankService>(new Uri(bankPath), new ServicePartitionKey(2));

            try
            {
                return await bankProxy.EnlistMoneyTransfer(userSend, userReceive, amount);
            }
            catch (Exception)
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
