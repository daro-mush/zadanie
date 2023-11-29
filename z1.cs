using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using CsvHelper;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;

namespace YourNamespace
{
    [ApiController]
    [Route("[controller]")]
    public class YourController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string productsFile = await DownloadFileAsync("https://rekturacjazadanie.blob.core.windows.net/zadanie/Products.csv");
            //
            

            var products = await ReadCsvAsync<Product>(productsFile);
            await SaveProductsAsync(products);
            string inventoryFile = await DownloadFileAsync("https://rekturacjazadanie.blob.core.windows.net/zadanie/Inventory.csv);
            var inventory = await ReadCsvAsync<Inventory>(inventoryFile);
            await SaveInventoryAsync(inventory);
            string pricesFile = await DownloadFileAsync("https://rekturacjazadanie.blob.core.windows.net/zadanie/Prices.csv");
            var prices = await ReadCsvAsync<Price>(pricesFile);
            await SavePricesAsync(prices);

            return Ok();
        }

        private async Task<string> DownloadFileAsync(string url)
        {
            using WebClient client = new();
            string fileName = Path.GetTempFileName();
            await client.DownloadFileTaskAsync(url, fileName);
            return fileName;
        }

        private async Task<List<T>> ReadCsvAsync<T>(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecordsAsync<T>();
            return await records.ToListAsync();
        }

        private async Task SaveProductsAsync(List<Product> products)
        {
            string connectionString = "YourConnectionString";
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync();
            foreach (var product in products)
            {
                // Sprawdź, czy produkt nie jest kablem i czy jest wysyłany w ciągu 24 godzin
                if (product.Type != "Cable" && product.ShippingTime <= 24)
                {
                    // Tylko wybrane kolumny i ich dane są zapisywane
                    string query = $"INSERT INTO Products (Id, Name, ShippingTime) VALUES (@Id, @Name, @ShippingTime)";
                    await connection.ExecuteAsync(query, product);
                }
            }
        }



        
        private async Task SaveInventoryAsync(List<Inventory> inventoryItems)
        {
            string connectionString = "YourConnectionString";
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync();
            foreach (var inventory in inventoryItems)
            {
                // Sprawdź, czy produkt nie jest kablem i czy jest wysyłany w ciągu 24 godzin
                if (inventory.ShippingTime <=24)
                {
                    // Tylko wybrane kolumny i ich dane są zapisywane
                    string query = $"INSERT INTO Inventory (product_id, qty, unit) VALUES (@Id, @qty, @Unit)";
                    await connection.ExecuteAsync(query, product);
                }

            }
        }
        // Implement SaveInventoryAsync and SavePricesAsync in a similar way
         private async Task SavePricesAsync(List<Price> prices)
        {
            string connectionString = "YourConnectionString";
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync();
            foreach (var price in prices)
            {
                
                
                    // Tylko wybrane kolumny i ich dane są zapisywane
                    string query = $"INSERT INTO Prices (product_id, qty, unit) VALUES (@Id, @qty, @Unit)";
                    await connection.ExecuteAsync(query, product);
                

            }
        }
    }
}