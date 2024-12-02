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
using Microsoft.ServiceFabric.Data.Collections;
using System.Diagnostics;

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

        public async Task<bool> EnlistPurchase(int cientID, int bookId, int quantity)
        {
            try
            {
                double price = await GetItemPrice(bookId);
                if (price == -1)
                    return false;
                int transferId = (int)DateTime.UtcNow.Ticks;

                bool res = await PrepareTransfer(transferId, cientID, bookId, quantity, price);
                if (!res)
                    return false;


                int purchaseId = (int)DateTime.UtcNow.Ticks;

                res = await PreparePurchase(purchaseId, cientID, bookId, quantity, price);
                if (!res)
                    return false;

                res = await CommitTransfer(transferId);
                if (!res)
                {
                    await RollbackTransfer(transferId);
                    return false;
                }

                res = await CommitPurchase(purchaseId);
                if (!res)
                {
                    await RollbackPurchase(purchaseId);
                    await RollbackTransfer(transferId);
                    return false;
                }

                return true;
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


        public async Task<bool> PreparePurchase(int purchaseId, int cientID, int bookId, int quantity, double price)
        {
            var bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

            try
            {
                return await bookstoreProxy.Prepare(purchaseId, cientID, bookId, quantity, price);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CommitPurchase(int id)
        {
            var bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

            try
            {
                return await bookstoreProxy.Commit(id);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> PrepareTransfer(int transferId, int cientID, int bookId, int quantity, double price)
        {
            var bankProxy = ServiceProxy.Create<IBankService>(new Uri(bankPath), new ServicePartitionKey(2));

            try
            {
                return await bankProxy.Prepare(transferId, cientID, bookId, quantity, price);         
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CommitTransfer(int id)
        {
            var bankProxy = ServiceProxy.Create<IBankService>(new Uri(bankPath), new ServicePartitionKey(2));

            try
            {
                return await bankProxy.Commit(id);
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> RollbackPurchase(int id)
        {
            var bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

            try
            {
                return await bookstoreProxy.RollBack(id);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RollbackTransfer(int id)
        {
            var bankProxy = ServiceProxy.Create<IBankService>(new Uri(bankPath), new ServicePartitionKey(2));

            try
            {
                return await bankProxy.RollBack(id);
            }
            catch (Exception)
            {
                return false;
            }
        }


        public async Task<List<TransferDTO>> GetTransfers()
        {
            var bankProxy = ServiceProxy.Create<IBankService>(new Uri(bankPath), new ServicePartitionKey(2));

            try
            {
                var transfers = await bankProxy.GetTransfers();
                return transfers.ToList();
            }
            catch (Exception)
            {
                return null!;
            }
        }

        public async Task<List<PurchaseDTO>> GetPurchases()
        {
            var bookstoreProxy = ServiceProxy.Create<IBookstoreService>(new Uri(bookstorePath), new ServicePartitionKey(1));

            try
            {
                var purchases = await bookstoreProxy.GetPurchases();
                return purchases.ToList();
            }
            catch (Exception)
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
