using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Models;

namespace Client.Controllers
{
    public class BankController : Controller
    {
        private Communication _communication = new Communication();
        public async Task<IActionResult> IndexAsync()
        {
            _clients = await _communication.ListClients();
            return View(_clients);
        }

        private static List<BankClient> _clients = new List<BankClient>();
        private static List<MoneyTransfer> _transfers = new List<MoneyTransfer>();

        public async Task<IActionResult> SeeMoneyTransfers()
        {
            var transfers = await _communication.GetCommitedTransfers();
            return View(transfers);
        }
    }
}
