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

            List<Book> books = new List<Book>();
            foreach (BookDTO bookDto in booksDTO) {

                books.Add(DtoMapper.ConvertToBook(bookDto));
            }

            return books;
        }

        public async Task<bool> EnlistPurchase(long? bookId, int? count)
        {
           bool result =  await _validationServices.EnlistPurchase(bookId, count);
           return result;
        }

        public async Task<string> GetItemPrice(long? bookId)
        {
            if (bookId is null)
            {
                return null!;
            }

            try
            {
                return null;
            }
            catch (Exception e)
            {
                return null!;
            }
        }

        public async Task<List<BankClient>> ListClients()
        {
            List<ClientDTO> clientsDTO = await _validationServices.ListClients();

            List<BankClient> clients = new List<BankClient>();
            foreach (ClientDTO clientDto in clientsDTO)
            {

                clients.Add(DtoMapper.ConvertToClient(clientDto));
            }

            return clients;
        }

        public async Task<string> EnlistMoneyTransfer(long? userSend, long? userReceive, double? amount)
        {
            if (userSend is null || userReceive is null || amount is null)
            {
                return null;
            }


            try
            {
                return null;
            }
            catch (Exception e)
            {
                return null!;
            }
        }
    
    }
}
