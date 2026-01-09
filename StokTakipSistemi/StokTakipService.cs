using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace StokTakipSistemi
{
    public class StokTakipService
    {
        // 1. Yeni Şube Ekleme
        public void SubeEkle(string ad)
        {
            using (var db = new AppDbContext())
            {
                var yeniSube = new Branch { Name = ad };
                db.Branches.Add(yeniSube);
                db.SaveChanges();
            }
        }

        // 2. Şube Silme (Ürünler Otomatik Silinir)
        public void SubeSil(int subeId)
        {
            using (var db = new AppDbContext())
            {
                var sube = db.Branches.Find(subeId);
                if (sube != null)
                {
                    db.Branches.Remove(sube);
                    db.SaveChanges();
                }
            }
        }

        // 3. Tüm Şubeleri Getirme
        public List<Branch> SubeleriGetir()
        {
            using (var db = new AppDbContext())
            {
                return db.Branches.ToList();
            }
        }

        // 4. Şubeye Ürün Ekleme
        public void UrunEkle(int subeId, string urunAd, int adet, decimal fiyat)
        {
            using (var db = new AppDbContext())
            {
                var yeniUrun = new Product 
                { 
                    ProductName = urunAd, 
                    Quantity = adet, 
                    Price = fiyat, 
                    BranchId = subeId 
                };
                db.Products.Add(yeniUrun);
                db.SaveChanges();
            }
        }
    }
}