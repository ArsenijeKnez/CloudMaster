using Client.Models;
using Common.Dto;
using System;

namespace Client.Mapper
{
    public static class DtoMapper
    {
        public static Book ConvertToBook(BookDTO bookDTO)
        {
            return new Book
            {
                Id = bookDTO.Id,
                Title = bookDTO.Title,
                Author = bookDTO.Author,
                Price = bookDTO.Price
            };
        }

        public static BankClient ConvertToClient(ClientDTO clientDTO)
        {
            return new BankClient
            {
                Id = clientDTO.Id,
                Name = clientDTO.Name,
                Email = clientDTO.Email
            };
        }

        public static MoneyTransfer ConvertToMoneyTransfer(TransferDTO transferDTO)
        {
            return new MoneyTransfer
            {
                Id = transferDTO.Id,
                FromClientId = transferDTO.SenderId,
                ToClientId = transferDTO.ReceiverId,
                Amount = transferDTO.Amount
            };
        }

        public static Purchase ConvertToPurchase(PurchaseDTO purchaseDTO)
        {;

            return new Purchase
            {
                Id = purchaseDTO.Id,
                BookId = purchaseDTO.BookId,
                Quantity = purchaseDTO.Count
            };
        }
    }
}
