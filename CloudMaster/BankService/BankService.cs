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

namespace BankService
{
    internal sealed class BankService : StatefulService, IBankService
    {
        public BankService(StatefulServiceContext context)
            : base(context) { }

        #region IBankImplementation

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


        public async Task<bool> EnlistMoneyTransfer(int userSend, int userReceive, double amount)
        {
            var transfersDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, TransferDTO>>("transfers");

            using (var tx = StateManager.CreateTransaction())
            {
                int transferId = (int)DateTime.UtcNow.Ticks; 
                var transfer = new TransferDTO
                {
                    Id = transferId,
                    SenderId = userSend,
                    ReceiverId = userReceive,
                    Amount = amount,
                    Status = "Pending"
                };

                await transfersDict.AddOrUpdateAsync(tx, transferId, transfer, (id, oldValue) => transfer);
                await tx.CommitAsync();
            }

            return true;
        }

        #endregion

        #region ITransaction

        public async Task<List<ITransactionDTO>> Prepare()
        {
            var preparedTransfers = new List<ITransactionDTO>(); 
            var transfersDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, TransferDTO>>("transfers");

            using (var tx = StateManager.CreateTransaction())
            {
                var pendingTransfers = await transfersDict.CreateEnumerableAsync(tx);
                using (var asyncEnumerator = pendingTransfers.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var transfer = asyncEnumerator.Current;
                        if (transfer.Value.Status == "Pending")
                        {
                            preparedTransfers.Add((ITransactionDTO)transfer.Value);
                        }
                    }
                }

                await tx.CommitAsync();
            }

            return preparedTransfers;
        }




        public async Task<List<ITransactionDTO>> Commit()
        {
            var committedTransfers = new List<ITransactionDTO>(); 
            var transfersDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, TransferDTO>>("transfers");

            using (var tx = StateManager.CreateTransaction())
            {
                var pendingTransfers = await transfersDict.CreateEnumerableAsync(tx);
                using (var asyncEnumerator = pendingTransfers.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var transfer = asyncEnumerator.Current;
                        if (transfer.Value.Status == "Pending")
                        {
                            var updatedTransfer = transfer.Value;
                            updatedTransfer.Status = "Committed";

                            await transfersDict.AddOrUpdateAsync(tx, updatedTransfer.Id, updatedTransfer, (id, oldValue) => updatedTransfer);
                            committedTransfers.Add((ITransactionDTO)updatedTransfer);
                        }
                    }
                }

                await tx.CommitAsync();
            }

            return committedTransfers;
        }



        public async Task RollBack()
        {
            var transfersDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, TransferDTO>>("transfers");

            using (var tx = StateManager.CreateTransaction())
            {
                var pendingTransfers = await transfersDict.CreateEnumerableAsync(tx);
                using (var asyncEnumerator = pendingTransfers.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var transfer = asyncEnumerator.Current;
                        if (transfer.Value.Status == "Pending")
                        {
                            var rolledBackTransfer = transfer.Value;
                            rolledBackTransfer.Status = "RolledBack";

                            await transfersDict.AddOrUpdateAsync(tx, rolledBackTransfer.Id, rolledBackTransfer, (id, oldValue) => rolledBackTransfer);
                        }
                    }
                }

                await tx.CommitAsync();
            }
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

