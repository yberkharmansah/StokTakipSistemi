using MongoDB.Driver;
using MongoDB.Bson; // BsonDocument ve ObjectId için
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq; // OrderBy için

namespace StokTakipSistemi
{
    public class MongoDBService
    {
        private readonly IMongoCollection<Branch> _branchesCollection;
        private readonly IMongoCollection<Product> _productsCollection;

        public MongoDBService(MongoDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _branchesCollection = database.GetCollection<Branch>(settings.BranchesCollectionName);
            _productsCollection = database.GetCollection<Product>(settings.ProductsCollectionName);
        }

        public async Task<List<Branch>> GetBranchesAsync()
        {
            return await _branchesCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task InsertBranchAsync(Branch branch)
        {
            await _branchesCollection.InsertOneAsync(branch);
        }

        public async Task DeleteBranchAsync(string id)
        {
            await _branchesCollection.DeleteOneAsync(b => b.Id == id);
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _productsCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<List<Product>> GetProductsByBranchAsync(string branchId, string searchTerm = null)
{
    var filterBuilder = Builders<Product>.Filter;
    FilterDefinition<Product> filter = filterBuilder.ElemMatch(p => p.BranchStocks, bs => bs.BranchId == branchId);

    if (!string.IsNullOrEmpty(searchTerm))
    {
        // Hem ProductName hem de Barcode üzerinde arama yap
        var searchFilter = filterBuilder.Where(p =>
            p.ProductName.ToLower().Contains(searchTerm.ToLower()) ||
            p.Barcode.ToLower().Contains(searchTerm.ToLower()));

        // Var olan branchId filtresi ile yeni arama filtresini birleştir
        filter = filterBuilder.And(filter, searchFilter);
    }

    return await _productsCollection.Find(filter).ToListAsync();
}

        public async Task InsertProductAsync(Product product)
        {
            await _productsCollection.InsertOneAsync(product);
        }

        public async Task UpdateProductAsync(string id, Product updatedProduct)
        {
            await _productsCollection.ReplaceOneAsync(p => p.Id == id, updatedProduct);
        }

        // --- Stok miktarı güncelleme metodu (BranchStock listesi için düzenlendi) ---
        public async Task UpdateProductStockAsync(string productId, string branchId, int quantity, string operationType)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            UpdateDefinition<Product> update;

            // `$inc` operatörünü kullanırken, array içindeki belirli bir elemanın özelliğini
            // doğrudan güncelleyemezsiniz. Bunun yerine, ya elemanı `$pull` ve `$push` ile yeniden eklemeniz
            // ya da `$set` operatörünü kullanarak elemanın tamamını güncellemeniz gerekir.
            // Biz burada mevcut stoğu bulup yeni değeriyle güncelleme (find, update, replace) yaklaşımını kullanacağız.
            // Bu, MongoDB'nin array içindeki gömülü belgeleri güncellemek için en yaygın ve doğru yaklaşımlardan biridir.

            // 1. Ürünü ID'sine göre bul
            var product = await _productsCollection.Find(filter).FirstOrDefaultAsync();

            if (product == null)
            {
                throw new Exception($"Ürün bulunamadı: {productId}");
            }

            // 2. İlgili şube stoğunu bul
            var branchStock = product.BranchStocks.FirstOrDefault(bs => bs.BranchId == branchId);

            if (branchStock == null)
            {
                // Eğer şube için stok kaydı yoksa (genellikle ekleme modunda oluşur)
                // veya manuel olarak eklenmesi gerekiyorsa, yeni bir BranchStock ekleyebiliriz.
                // Bu senaryo, ürün eklenirken başlangıç stoğunun girildiği durumdan farklıdır.
                // Örneğin, bir ürünün sonradan yeni bir şubeye girişi yapılıyorsa.
                if (operationType == "add")
                {
                    product.BranchStocks.Add(new BranchStock { BranchId = branchId, Stock = quantity });
                }
                else
                {
                    // Azaltma işlemi için stok kaydı yoksa hata verebiliriz veya 0 olarak kabul edebiliriz.
                    throw new Exception($"Ürün {productId} için {branchId} şubesinde stok bulunamadı.");
                }
            }
            else
            {
                // 3. Stok miktarını güncelle
                if (operationType == "add")
                {
                    branchStock.Stock += quantity;
                }
                else // "decrease"
                {
                    branchStock.Stock -= quantity;
                    if (branchStock.Stock < 0) // Negatif stok olmaması için kontrol
                    {
                        branchStock.Stock = 0; // veya hata fırlatabiliriz
                        // Hata fırlatmak yerine 0'a çekmek daha kullanıcı dostu olabilir,
                        // ancak iş mantığınıza göre karar vermelisiniz.
                        // throw new Exception($"Stok miktarı {branchStock.Stock} altına düşemez.");
                    }
                }
            }

            // 4. Güncellenmiş ürünü veritabanında değiştir
            await _productsCollection.ReplaceOneAsync(filter, product);
        }

        public async Task DeleteProductAsync(string id)
        {
            await _productsCollection.DeleteOneAsync(p => p.Id == id);
        }

        // Bir ürünü belirli bir şubeden tamamen kaldırmak isterseniz (opsiyonel)
        public async Task RemoveProductStockFromBranchAsync(string productId, string branchId)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update.PullFilter(p => p.BranchStocks, bs => bs.BranchId == branchId);
            await _productsCollection.UpdateOneAsync(filter, update);
        }
    }
}