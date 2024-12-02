using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class TransferDTO
    {
        public int Id { get; set; }          
        public int ClientId { get; set; }

        public int BookId { get; set; }

        public int Quantity { get; set; }     
        public double Amount { get; set; }       
        public string Status { get; set; }       
    }
}
