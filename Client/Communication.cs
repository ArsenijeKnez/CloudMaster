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

        public async Task<bool> EnlistPurchase(int bookId, int count)
        {
            bool result = await _validationServices.EnlistPurchase(bookId, count);
            return result;
        }

        public async Task<double> GetItemPrice(int bookId)
        {
            return await _validationServices.GetItemPrice(bookId);
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

        public async Task<bool> EnlistMoneyTransfer(int userSend, int userReceive, double amount)
        {
            return await _validationServices.EnlistMoneyTransfer(userSend, userReceive, amount);
        }

    }
}
