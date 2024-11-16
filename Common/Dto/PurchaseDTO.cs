using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class PurchaseDTO
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int Count { get; set; }
        public string Status { get; set; } 
    }
}
