StokTakipSistemi

StokTakipSistemi, farklı şubelerde ürün stoklarını takip etmek için tasarlanmış basit bir Windows Forms uygulamasıdır. Bu uygulama MongoDB veritabanı kullanarak ürün ve şube bilgilerini kalıcı olarak saklar.

Özellikler
Şube Yönetimi:
Yeni şubeler ekleme.
Mevcut şubeleri silme.
Şube seçimine göre ürünleri listeleme.
Ürün Yönetimi:
Yeni ürünler ekleme (ürün adı, barkod ve başlangıç stoğu ile).
Mevcut ürünleri düzenleme (ürün adı, barkod).
Ürünleri silme.
Stok Takibi:
Seçili şubeye göre ürün stoklarını görüntüleme.
Ürün stoklarını artırma veya azaltma.
Arama Özelliği:
Ürün adı veya barkod ile arama yapma.
Dinamik UI:
Uygulama penceresi yeniden boyutlandırıldığında UI öğelerinin (özellikle DataGridView) dinamik olarak ayarlanması.
MongoDB Entegrasyonu:
Veritabanı işlemleri için MongoDB kullanır.
Kurulum ve Çalıştırma
Projeyi yerel bilgisayarınızda kurmak ve çalıştırmak için aşağıdaki adımları izleyin:

Önkoşullar
.NET 8.0 SDK: Geliştirme ortamınızda .NET 8.0 kurulu olmalıdır.
MongoDB Sunucusu: Yerel olarak çalışan bir MongoDB sunucusuna ihtiyacınız var. MongoDB Community Edition'ı resmi web sitesinden indirebilir ve kurabilirsiniz.
Visual Studio (Önerilir): C# Windows Forms uygulamaları geliştirmek için Visual Studio 2022 veya üzeri önerilir.
Adımlar
Projeyi Klonlayın:
Bash

git clone https://github.com/yberkharmansah/StokTakipSistemi.git
Klasöre Girin:
Bash

cd StokTakipSistemi/StokTakipSistemi
MongoDB Bağlantı Ayarlarını Yapılandırın: StokTakipSistemi/MongoDB/MongoDBSettings.cs dosyasını açın. Varsayılan bağlantı dizesi mongodb://localhost:27017 olarak ayarlanmıştır. Eğer MongoDB sunucunuz farklı bir adreste veya portta çalışıyorsa bu ayarı güncelleyin.
C#

namespace StokTakipSistemi
{
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017"; // Bağlantı adresinizi buraya yazın
        public string DatabaseName { get; set; } = "StokTakipDB"; // Veritabanı adı
        public string BranchesCollectionName { get; set; } = "Branches"; // Şubeler koleksiyon adı
        public string ProductsCollectionName { get; set; } = "Products"; // Ürünler koleksiyon adı
    }
}
Projeyi Açın: Visual Studio'yu açın ve StokTakipSistemi.sln dosyasını (StokTakipSistemi klasörünün içinde) açın.
NuGet Paketlerini Geri Yükleyin: Visual Studio otomatik olarak eksik NuGet paketlerini geri yüklemelidir. Eğer yapmazsa, Çözüm Gezgini'nde projeye sağ tıklayıp "Restore NuGet Packages" seçeneğini seçebilirsiniz.
Uygulamayı Çalıştırın: Visual Studio'da F5 tuşuna basın veya "Başlat" butonuna tıklayarak uygulamayı derleyin ve çalıştırın.
Kullanım
Uygulama açıldığında:

İlk olarak şube eklemeniz istenecektir. "Yeni Şube Ekle" butonuna tıklayarak yeni şubeler oluşturun.
Şubeler eklendikten sonra, şube seçimi ComboBox'ından bir şube seçin.
"Yeni Ürün Ekle" butonuna tıklayarak seçili şubeye ürünler ekleyebilirsiniz.
Ürünleri listeleyebilir, düzenleyebilir, silebilir veya stok miktarını güncelleyebilirsiniz.
"Ürün Ara" kutusuna ürün adı veya barkod girerek arama yapabilirsiniz.
Proje Yapısı
StokTakipSistemi.csproj: Proje dosyası.
Program.cs: Uygulamanın başlangıç noktası.
Forms/: Windows Forms UI dosyalarını içerir.
Form1.cs: Ana uygulama penceresi.
AddEditBranchForm.cs: Şube ekleme/düzenleme dialogu.
AddEditProductForm.cs: Ürün ekleme/düzenleme dialogu.
Collections/: MongoDB koleksiyonları için model sınıflarını içerir.
Branch.cs: Şube modeli.
Product.cs: Ürün ve stok bilgileri için model.
MongoDB/: MongoDB servis ve ayarlarını içerir.
MongoDBService.cs: MongoDB ile etkileşim için servis sınıfı.
MongoDBSettings.cs: MongoDB bağlantı ayarları.
Katkıda Bulunma
Geliştirmeye katkıda bulunmak isterseniz, lütfen bir fork yapın ve pull request gönderin.
