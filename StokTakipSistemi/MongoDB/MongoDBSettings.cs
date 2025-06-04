namespace StokTakipSistemi
{
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017"; // Varsayılan bağlantı adresi
        public string DatabaseName { get; set; } = "StokTakipDB"; // Veritabanı adı
        public string BranchesCollectionName { get; set; } = "Branches"; // Şubeler koleksiyon adı
        public string ProductsCollectionName { get; set; } = "Products"; // Ürünler koleksiyon adı
    }
}