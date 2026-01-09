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
    public int Id { get; set; }
    public string ProductName { get; set; } // Name yerine ProductName yaptık
    public string Barcode { get; set; }      // Barcode ekledik
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    
    public int BranchId { get; set; }
    public Branch Branch { get; set; }
}
}