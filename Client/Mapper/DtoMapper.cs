using Client.Models;
using Common.Dto;

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
    }
}
