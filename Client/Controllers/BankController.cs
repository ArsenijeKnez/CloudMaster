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


        [HttpGet]
        public IActionResult EnlistMoneyTransfer()
        {
            ViewBag.Clients = _clients;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnlistMoneyTransferAsync(int fromClientId, int toClientId, double amount)
        {
            if (fromClientId == toClientId || amount <= 0)
            {
                ModelState.AddModelError("", "Invalid transfer");
                ViewBag.Clients = _clients;
                return View();
            }

            bool done = await _communication.EnlistMoneyTransfer(fromClientId, toClientId, amount);
            if (!done)
            {
                ModelState.AddModelError("", "Invalid transfer");
                ViewBag.Clients = _clients;
                return View();
            }
            return RedirectToAction("Index");
        }
    }
}
