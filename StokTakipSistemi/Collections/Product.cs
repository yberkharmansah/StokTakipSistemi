using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic; // Bu using ifadesi kesinlikle olmalı

namespace StokTakipSistemi
{
   public class BranchStock
{
    public string BranchId { get; set; }
    public int Stock { get; set; }
}

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("productName")]
    public string ProductName { get; set; } = string.Empty;

    [BsonElement("barcode")]
    public string Barcode { get; set; } = string.Empty;

    [BsonElement("branchStocks")]
    public List<BranchStock> BranchStocks { get; set; } = new List<BranchStock>();
}
}