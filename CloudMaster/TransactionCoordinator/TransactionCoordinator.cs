using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common.Interfaces;
using Common.Dto;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace TransactionCoordinator
{
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
            var bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

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
            var bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

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
            var bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

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
            var bankProxy = ServiceProxy.Create<IBankService>(new Uri(bankPath), new ServicePartitionKey(2));

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
            var bankProxy = ServiceProxy.Create<IBankService>(new Uri(bankPath), new ServicePartitionKey(2));

            try
            {
                return await bankProxy.EnlistMoneyTransfer(userSend, userReceive, amount);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<ITransactionDTO>> PreparePurchases()
        {
            var bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

            try
            {
                var purchases = await bookstoreProxy.Prepare();
                return purchases.Cast<ITransactionDTO>().ToList(); 
            }
            catch (Exception)
            {
                return new List<ITransactionDTO>();
            }
        }

        public async Task<List<ITransactionDTO>> CommitPurchases()
        {
            var bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

            try
            {
                var purchases = await bookstoreProxy.Commit();
                return purchases.Cast<ITransactionDTO>().ToList(); 
            }
            catch (Exception)
            {
                return new List<ITransactionDTO>();
            }
        }

        public async Task<List<ITransactionDTO>> PrepareTransfers()
        {
            var bankProxy = ServiceProxy.Create<IBankService>(new Uri(bankPath), new ServicePartitionKey(2));

            try
            {
                var transfers = await bankProxy.Prepare();
                return transfers.Cast<ITransactionDTO>().ToList();  
            }
            catch (Exception)
            {
                return new List<ITransactionDTO>();
            }
        }

        public async Task<List<ITransactionDTO>> CommitTransfers()
        {
            var bankProxy = ServiceProxy.Create<IBankService>(new Uri(bankPath), new ServicePartitionKey(2));

            try
            {
                var transfers = await bankProxy.Commit();
                return transfers.Cast<ITransactionDTO>().ToList(); 
            }
            catch (Exception)
            {
                return new List<ITransactionDTO>();
            }
        }
        public async Task<bool> RollbackPurchases()
        {
            var bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

            try
            {
                await bookstoreProxy.RollBack();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RollbackTransfers()
        {
            var bankProxy = ServiceProxy.Create<IBankService>(new Uri(bankPath), new ServicePartitionKey(2));

            try
            {
                await bankProxy.RollBack();
                return true;
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
