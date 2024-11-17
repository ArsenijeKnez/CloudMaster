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
            bool done = await _communication.EnlistMoneyTransfer(fromClientId, toClientId, amount);
            if (!done)
            {
                ModelState.AddModelError("", "Invalid transfer");
                ViewBag.Clients = _clients;
                return View();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PrepareMoneyTransfers()
        {
            var transfers = await _communication.PrepareTransfers();
            return View(transfers);
        }

        [HttpPost]
        public async Task<IActionResult> CommitMoneyTransfers()
        {
            var committedTransfers = await _communication.CommitTransfers();
            return View("CommittedMoneyTransfers", committedTransfers);
        }

        [HttpPost]
        public async Task<IActionResult> RollbackMoneyTransfers()
        {
            var rollbackResult = await _communication.RollbackTransfers();
            TempData["Message"] = rollbackResult ? "Rollback successful" : "Rollback failed";
            return RedirectToAction("PrepareMoneyTransfers");
        }
    }
}
