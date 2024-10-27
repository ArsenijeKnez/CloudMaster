﻿using Client.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Controllers
{
    public class BookstoreController : Controller
    {
        public IActionResult Index()
        {
            return View(_books);
        }

        private static List<Book> _books = new List<Book>
    {
        new Book { Id = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Price = 10.99, Stock = 10 },
        new Book { Id = 2, Title = "1984", Author = "George Orwell", Price = 8.99, Stock = 5 }
    };

        private static List<Purchase> _purchases = new List<Purchase>();

        [HttpGet]
        public IActionResult EnlistPurchase()
        {
            ViewBag.Books = _books; 
            return View();
        }

        [HttpPost]
        public IActionResult EnlistPurchase(int bookId, int quantity)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);

            if (book == null || quantity <= 0 || quantity > book.Stock)
            {
                ModelState.AddModelError("", "Invalid purchase details.");
                ViewBag.Books = _books;
                return View();
            }

            var purchase = new Purchase
            {
                Id = _purchases.Count + 1,
                BookId = bookId,
                Quantity = quantity,
                PurchaseDate = DateTime.Now
            };

            _purchases.Add(purchase);
            book.Stock -= quantity;

            return RedirectToAction("ListAvailableItems");
        }

        [HttpGet]
        public IActionResult GetItemPrice(int id)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound("Book not found.");
            }

            return Json(new { price = book.Price });
        }
    }
}
