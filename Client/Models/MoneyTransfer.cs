using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Models
{
    public class MoneyTransfer
    {
        public int Id { get; set; }
        public int FromClientId { get; set; }
        public int ToClientId { get; set; }
        public double Amount { get; set; }
    }
}
