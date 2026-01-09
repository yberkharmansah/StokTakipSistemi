using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace StokTakipSistemi
{
       public class Branch
{
    public int Id { get; set; } // SQLite otomatik artan sayı yapar
    public string Name { get; set; }
    
    // Bir şubenin birden fazla ürünü olabilir
    public List<Product> Products { get; set; } = new List<Product>();
}
}