using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class TransferDTO
    {
        public int Id { get; set; }          
        public int SenderId { get; set; }       
        public int ReceiverId { get; set; }     
        public double Amount { get; set; }       
        public string Status { get; set; }       
    }
}
