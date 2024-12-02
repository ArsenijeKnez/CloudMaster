using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common.Interfaces;
using Common.Dto;
using System.Globalization;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System.Xml.Linq;
using System.Diagnostics;

namespace BankService
{
    internal sealed class BankService : StatefulService, IBankService
    {
        public BankService(StatefulServiceContext context)
            : base(context) { }

        public async Task SeedClientsAsync()
        {
           var clientsDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, ClientDTO>>("clients");

           using (var tx = StateManager.CreateTransaction())
           {
               var count = await clientsDict.GetCountAsync(tx);
               if (count == 0)
               {
                   var sampleClients = new List<ClientDTO>
               {
                   new ClientDTO { Id = 1, Name = "Nikola Petrović", Email = "nikola.petrovic@gmail.com", Balance = 1000.00 },
                   new ClientDTO { Id = 2, Name = "Jelena Marković", Email = "jelena.markovic@gmail.com", Balance = 750.50 },
                   new ClientDTO { Id = 3, Name = "Milan Jovanović", Email = "milan.jovanovic@gmail.com", Balance = 1200.75 },
                   new ClientDTO { Id = 4, Name = "Ana Nikolić", Email = "ana.nikolic@gmail.com", Balance = 500.00 },
                   new ClientDTO { Id = 5, Name = "Vladimir Ilić", Email = "vladimir.ilic@gmail.com", Balance = 850.20 }
               };

                   foreach (var client in sampleClients)
                   {
                       await clientsDict.AddAsync(tx, client.Id, client);
                   }

                   await tx.CommitAsync();
               }
           }
        }

        #region IBankImplementation

        public async Task<List<ClientDTO>> ListClients()
        {
            var clients = new List<ClientDTO>();
            var clientsDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, ClientDTO>>("clients");

            using (var tx = StateManager.CreateTransaction())
            {
                var allClients = await clientsDict.CreateEnumerableAsync(tx);
                using (var asyncEnumerator = allClients.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var kvp = asyncEnumerator.Current;
                        clients.Add(kvp.Value);
                    }
                }

                await tx.CommitAsync();
            }

            return clients;
        }

        public async Task<List<TransferDTO>> GetTransfers()
        {
            var allTransfers = new List<TransferDTO>();
            var transfersDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, TransferDTO>>("transfers");

            using (var tx = StateManager.CreateTransaction())
            {
                var committedTransfers = await transfersDict.CreateEnumerableAsync(tx);
                using (var asyncEnumerator = committedTransfers.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var transfer = asyncEnumerator.Current;
                        if (transfer.Value.Status == "Committed")
                        {
                            allTransfers.Add(transfer.Value);
                        }
                    }
                }
            }
            return allTransfers;
        }

        #endregion

        #region ITransaction

        public async Task<bool> Prepare(int transferId, int clientId, int bookId, int quantity, double price)
        {
            var clientsDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, ClientDTO>>("clients");

            double FullPrice = price * quantity;

            using (var tx = StateManager.CreateTransaction())
            {
                var client = await clientsDict.TryGetValueAsync(tx, bookId);
      
                if (client.HasValue)
                {
                    ClientDTO updatedClient = client.Value;
                    
                    if (updatedClient.Balance < FullPrice)
                        return false;
                }
                await tx.CommitAsync();
            }


            var transfersDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, TransferDTO>>("transfers");

            using (var tx = StateManager.CreateTransaction())
            {
                var transfer = new TransferDTO
                {
                    Id = transferId,
                    ClientId = clientId,
                    BookId = bookId,
                    Quantity = quantity,
                    Amount = FullPrice,
                    Status = "Pending"
                };

                await transfersDict.AddOrUpdateAsync(tx, transferId, transfer, (id, oldValue) => transfer);
                await tx.CommitAsync();
            }


            return true;
        }


        public async Task<bool> Commit(int id)
        {
            var transferDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, TransferDTO>>("transfers");
            double Price = 0;
            int ClientId = 0;

            using (var tx = StateManager.CreateTransaction())
            {
                var transfer = await transferDict.TryGetValueAsync(tx, id);

                if (transfer.HasValue)
                {
                    TransferDTO updatedTransfer = transfer.Value;

                    updatedTransfer.Status = "Committed";
                    Price = updatedTransfer.Amount;
                    ClientId = updatedTransfer.ClientId;    

                    await transferDict.AddOrUpdateAsync(tx, id, updatedTransfer, (id, oldValue) => updatedTransfer);
                }
                await tx.CommitAsync();
            }

            var clientsDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, ClientDTO>>("clients");

            using (var tx = StateManager.CreateTransaction())
            {
                var client = await clientsDict.TryGetValueAsync(tx, ClientId);

                if (client.HasValue)
                {
                    ClientDTO updatedClient = client.Value;

                    if (updatedClient.Balance < Price)
                        return false;

                    updatedClient.Balance -= Price;
                    await clientsDict.AddOrUpdateAsync(tx, ClientId, updatedClient, (id, oldValue) => updatedClient);
                }
                await tx.CommitAsync();
            }

            return true;    
        }


        public async Task<bool> RollBack(int id)
        {
            var transferDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, TransferDTO>>("transfers");
            double Price = 0;
            int ClientId = 0;

            using (var tx = StateManager.CreateTransaction())
            {
                var transfer = await transferDict.TryGetValueAsync(tx, id);

                if (transfer.HasValue)
                {
                    TransferDTO updatedTransfer = transfer.Value;

                    updatedTransfer.Status = "Rolledback";
                    Price = updatedTransfer.Amount;
                    ClientId = updatedTransfer.ClientId;

                    await transferDict.AddOrUpdateAsync(tx, id, updatedTransfer, (id, oldValue) => updatedTransfer);
                }
                await tx.CommitAsync();
            }

            var clientsDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, ClientDTO>>("clients");

            using (var tx = StateManager.CreateTransaction())
            {
                var client = await clientsDict.TryGetValueAsync(tx, ClientId);

                if (client.HasValue)
                {
                    ClientDTO updatedClient = client.Value;

                    updatedClient.Balance += Price;
                    await clientsDict.AddOrUpdateAsync(tx, ClientId, updatedClient, (id, oldValue) => updatedClient);
                }
                await tx.CommitAsync();
            }

            return true;
        }


        #endregion

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await SeedClientsAsync();
            while (!cancellationToken.IsCancellationRequested)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "BankService is running.");
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }


        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() => this.CreateServiceRemotingReplicaListeners();
    }
}

