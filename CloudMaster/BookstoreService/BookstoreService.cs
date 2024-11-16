using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common.Interfaces;
using Common.Dto;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace BookstoreService
{
    internal sealed class BookstoreService : StatefulService, IBookstoreService
    {
        public BookstoreService(StatefulServiceContext context)
            : base(context) { }

        #region IBookstoreService Implementation

        private async Task SeedBooksAsync()
        {
            var booksDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, BookDTO>>("books");

            using (var tx = StateManager.CreateTransaction())
            {
                var itemCount = await booksDict.GetCountAsync(tx);
                if (itemCount == 0)
                {
                    var books = new List<BookDTO>
            {
                new BookDTO { Id = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Price = 10.99, Stock = 100 },
                new BookDTO { Id = 2, Title = "1984", Author = "George Orwell", Price = 8.99, Stock = 150 },
                new BookDTO { Id = 3, Title = "To Kill a Mockingbird", Author = "Harper Lee", Price = 12.50, Stock = 80 },
                new BookDTO { Id = 4, Title = "Moby Dick", Author = "Herman Melville", Price = 15.75, Stock = 60 },
                new BookDTO { Id = 5, Title = "Pride and Prejudice", Author = "Jane Austen", Price = 9.99, Stock = 120 }
            };

                    foreach (var book in books)
                    {
                        await booksDict.AddAsync(tx, book.Id, book);
                    }

                    await tx.CommitAsync();
                }
            }
        }

        public async Task<List<BookDTO>> ListAvailableItems()
        {
            var books = new List<BookDTO>();
            var booksDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, BookDTO>>("books");

            using (var tx = StateManager.CreateTransaction())
            {
                var allBooks = await booksDict.CreateEnumerableAsync(tx);
                using (var asyncEnumerator = allBooks.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var kvp = asyncEnumerator.Current;
                        books.Add(kvp.Value);
                    }
                }

                await tx.CommitAsync();
            }

            return books;
        }

        public async Task<bool> EnlistPurchase(int bookId, int count)
        {
            var purchasesDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, PurchaseDTO>>("purchases");

            using (var tx = StateManager.CreateTransaction())
            {
                var book = await StateManager.GetOrAddAsync<IReliableDictionary<int, BookDTO>>("books");
                var bookEntry = await book.TryGetValueAsync(tx, bookId);

                if (!bookEntry.HasValue || bookEntry.Value.Stock < count)
                {
                    return false; 
                }

                int purchaseId = (int)DateTime.UtcNow.Ticks;
                var purchase = new PurchaseDTO
                {
                    Id = purchaseId,
                    BookId = bookId,
                    Count = count,
                    Status = "Pending"
                };

                await purchasesDict.AddOrUpdateAsync(tx, purchaseId, purchase, (id, oldValue) => purchase);

                var updatedBook = bookEntry.Value;
                updatedBook.Stock -= count;

                await book.AddOrUpdateAsync(tx, updatedBook.Id, updatedBook, (id, oldValue) => updatedBook);

                await tx.CommitAsync();
            }

            return true;
        }

        public async Task<double> GetItemPrice(int bookId)
        {
            var booksDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, BookDTO>>("books");

            using (var tx = StateManager.CreateTransaction())
            {
                var book = await booksDict.TryGetValueAsync(tx, bookId);
                if (book.HasValue)
                {
                    return book.Value.Price;
                }
                return 0.0; 
            }
        }

        #endregion

        #region ITransaction Implementation

        public async Task<List<ITransactionDTO>> Prepare()
        {
            var preparedPurchases = new List<ITransactionDTO>(); 
            var purchasesDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, PurchaseDTO>>("purchases");

            using (var tx = StateManager.CreateTransaction())
            {
                var pendingPurchases = await purchasesDict.CreateEnumerableAsync(tx);
                using (var asyncEnumerator = pendingPurchases.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var purchase = asyncEnumerator.Current;
                        if (purchase.Value.Status == "Pending")
                        {
                            preparedPurchases.Add((ITransactionDTO)purchase.Value);
                        }
                    }
                }

                await tx.CommitAsync();
            }

            return preparedPurchases;
        }



        public async Task<List<ITransactionDTO>> Commit()
        {
            var committedPurchases = new List<ITransactionDTO>(); 
            var purchasesDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, PurchaseDTO>>("purchases");

            using (var tx = StateManager.CreateTransaction())
            {
                var pendingPurchases = await purchasesDict.CreateEnumerableAsync(tx);
                using (var asyncEnumerator = pendingPurchases.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var purchase = asyncEnumerator.Current;
                        if (purchase.Value.Status == "Pending")
                        {
                            var updatedPurchase = purchase.Value;
                            updatedPurchase.Status = "Committed";

                            await purchasesDict.AddOrUpdateAsync(tx, updatedPurchase.Id, updatedPurchase, (id, oldValue) => updatedPurchase);
                            committedPurchases.Add((ITransactionDTO)updatedPurchase);
                        }
                    }
                }

                await tx.CommitAsync();
            }

            return committedPurchases;
        }



        public async Task RollBack()
        {
            var purchasesDict = await StateManager.GetOrAddAsync<IReliableDictionary<int, PurchaseDTO>>("purchases");

            using (var tx = StateManager.CreateTransaction())
            {
                var pendingPurchases = await purchasesDict.CreateEnumerableAsync(tx);
                using (var asyncEnumerator = pendingPurchases.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var purchase = asyncEnumerator.Current;
                        if (purchase.Value.Status == "Pending")
                        {
                            var rolledBackPurchase = purchase.Value;
                            rolledBackPurchase.Status = "RolledBack";

                            await purchasesDict.AddOrUpdateAsync(tx, rolledBackPurchase.Id, rolledBackPurchase, (id, oldValue) => rolledBackPurchase);
                        }
                    }
                }

                await tx.CommitAsync();
            }
        }

        #endregion


        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() => this.CreateServiceRemotingReplicaListeners();

        /// <summary>
        /// This is the main entry point for your service replica.

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await SeedBooksAsync();
            while (!cancellationToken.IsCancellationRequested)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "BookstoreService is running.");
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }

    }
}

