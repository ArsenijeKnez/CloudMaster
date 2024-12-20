﻿using Client.Models;
using Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Controllers
{
    public class BookstoreController : Controller
    {
        private Communication _communication = new Communication();
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _books = await _communication.ListAvailableItemsAsync();
            return View(_books);

        }

        private static List<Book> _books = new List<Book>();
        private static List<Purchase> _purchases = new List<Purchase>();

        [HttpGet]
        public IActionResult EnlistPurchase()
        {
            ViewBag.Books = _books;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnlistPurchase(int clientId, int bookId, int quantity)
        {
            if (!await _communication.EnlistPurchase(clientId, bookId, quantity))
            {
                ModelState.AddModelError("", "Invalid purchase details.");
                ViewBag.Books = _books;
                return View();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SeePurchases()
        {
            var purchases = await _communication.SeeComittedPurchases();
            return View(purchases);
        }
    }
}
