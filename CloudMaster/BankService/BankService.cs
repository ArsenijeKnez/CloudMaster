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
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace BankService
{
    internal sealed class BankService : StatefulService, IBankService
    {
        public BankService(StatefulServiceContext context)
            : base(context) { }

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

        public async Task<bool> Prepare()
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
                            return true;
                        }
                    }
                }

                await tx.CommitAsync();
            }

            return false;
        }


        public async Task Commit()
        {
            var clientsDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, ClientDTO>>("clients");
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
                            var sender = await clientsDict.TryGetValueAsync(tx, transfer.Value.SenderId);
                            var receiver = await clientsDict.TryGetValueAsync(tx, transfer.Value.ReceiverId);

                            if (sender.HasValue && receiver.HasValue)
                            {
                                var senderUpdated = sender.Value;
                                var receiverUpdated = receiver.Value;

                                senderUpdated.Balance -= transfer.Value.Amount;
                                receiverUpdated.Balance += transfer.Value.Amount;

                                await clientsDict.AddOrUpdateAsync(tx, senderUpdated.Id, senderUpdated, (id, oldValue) => senderUpdated);
                                await clientsDict.AddOrUpdateAsync(tx, receiverUpdated.Id, receiverUpdated, (id, oldValue) => receiverUpdated);

                                var updatedTransfer = transfer.Value;
                                updatedTransfer.Status = "Committed";
                                await transfersDict.AddOrUpdateAsync(tx, updatedTransfer.Id, updatedTransfer, (id, oldValue) => updatedTransfer);
                            }
                        }
                    }
                }

                await tx.CommitAsync();
            }
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

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }
    }
}

