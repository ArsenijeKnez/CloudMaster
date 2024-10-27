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
        public IActionResult Index()
        {
            return View(_clients);
        }

        private static List<BankClient> _clients = new List<BankClient> 
        {
            new BankClient { Id = 1, Name = "John Doe", Email = "john@example.com" },
            new BankClient { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
        };

        private static List<MoneyTransfer> _transfers = new List<MoneyTransfer>();


        [HttpGet]
        public IActionResult EnlistMoneyTransfer()
        {
            ViewBag.Clients = _clients; 
            return View();
        }

        [HttpPost]
        public IActionResult EnlistMoneyTransfer(int fromClientId, int toClientId, double amount)
        {
            if (fromClientId == toClientId || amount <= 0)
            {
                ModelState.AddModelError("", "Invalid transfer");
                ViewBag.Clients = _clients;
                return View();
            }

            var transfer = new MoneyTransfer
            {
                Id = _transfers.Count + 1,
                FromClientId = fromClientId,
                ToClientId = toClientId,
                Amount = amount,
            };

            _transfers.Add(transfer);
            return RedirectToAction("Index");
        }
    }
}
