using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ITransactionDTO
    {
        int Id { get; set; }

        string Status { get; set; }
    }
}
