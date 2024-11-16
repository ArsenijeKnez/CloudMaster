﻿using Common.Dto;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ITransactionCoordinator : IService
    {
        Task<List<BookDTO>> ListAvailableItems();

        Task<bool> EnlistPurchase(int bookId, int count);

        Task<double> GetItemPrice(int bookId);

        Task<List<ClientDTO>> ListClients();

        Task<bool> EnlistMoneyTransfer(int userSend, int userReceive, double amount);

        Task<List<ITransactionDTO>> PrepareTransfers();

        Task<List<ITransactionDTO>> CommitTransfers();

        Task<bool> RollbackTransfers();

        Task<List<ITransactionDTO>> PreparePurchases();

        Task<List<ITransactionDTO>> CommitPurchases();

        Task<bool> RollbackPurchases();
    }
}
