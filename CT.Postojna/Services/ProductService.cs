using CT.Postojna.Models;

namespace CT.Postojna.Services
{
    public class ProductService
    {
        public static IEnumerable<Product> GenerateProducts() => Enumerable.Range(1, 100).Select(i => new Product
        {
            Id = i,
            Name = $"Product {i}",
            Description = $"Description for product {i}",
            Price = Random.Shared.Next(1, 9999) * 1.23m,
            Quantity = Random.Shared.Next(1, 9999),
            Released = DateTime.Now.AddDays(-Random.Shared.Next(1,3000)),
            Discontinued = i % 4 == 0,
            CategoryId = i % 5 == 0 ? (int?)null : i % 5,
            CategoryName = i % 5 == 0 ? string.Empty : $"Category {i % 5}"
        });
    }
}
