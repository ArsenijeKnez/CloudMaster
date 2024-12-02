using Client.Mapper;
using Client.Models;
using Common.Dto;
using Common.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client
{
    internal sealed class Communication
    {
        private readonly IValidation _validationServices = ServiceProxy.Create<IValidation>(new Uri("fabric:/CloudMaster/Validation"));

        public async Task<List<Book>> ListAvailableItemsAsync()
        {
            List<BookDTO> booksDTO = await _validationServices.ListAvailableItems();
            if (booksDTO == null)
                return null;

            List<Book> books = new List<Book>();
            foreach (BookDTO bookDto in booksDTO)
            {
                books.Add(DtoMapper.ConvertToBook(bookDto));
            }

            return books;
        }

        public async Task<bool> EnlistPurchase(int clientId, int bookId, int count)
        {
            bool result = await _validationServices.EnlistPurchase(clientId, bookId, count);
            return result;
        }

        public async Task<List<BankClient>> ListClients()
        {
            List<ClientDTO> clientsDTO = await _validationServices.ListClients();
            if (clientsDTO == null)
                return null;

            List<BankClient> clients = new List<BankClient>();
            foreach (ClientDTO clientDto in clientsDTO)
            {
                clients.Add(DtoMapper.ConvertToClient(clientDto));
            }

            return clients;
        }


        public async Task<List<Purchase>> SeeComittedPurchases()
        {
            List<PurchaseDTO> purchasesDTO =  await _validationServices.SeePurchases();

            if (purchasesDTO == null)
                return null;

            List<Purchase> purchases = new List<Purchase>();
            foreach (PurchaseDTO purchaseDto in purchasesDTO)
            {
                purchases.Add(DtoMapper.ConvertToPurchase(purchaseDto));
            }

            return purchases;
        }


        public async Task<List<MoneyTransfer>> GetCommitedTransfers()
        {
            List<TransferDTO> transfersDTO = await _validationServices.SeeTransfers();

            if (transfersDTO == null)
                return null;

            List<MoneyTransfer> transfers = new List<MoneyTransfer>();
            foreach (TransferDTO transferDto in transfersDTO)
            {
                transfers.Add(DtoMapper.ConvertToMoneyTransfer(transferDto));
            }

            return transfers;
        }

    }
}
