using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver; // MongoDB driver'ı için

namespace StokTakipSistemi
{
    public partial class Form1 : Form
    {
        private MongoDBService _mongoDBService; // MongoDBService örneği
private bool _isLoadingInitialData = true; // Formun ilk yüklenmesi sırasında gereksiz olayları engellemek için
        // UI Bileşenleri tanımlamaları
        private ComboBox cmbBranches;
        private DataGridView dgvProducts;
        private Button btnAddProduct;
        private Button btnEditProduct;
        private Button btnDeleteProduct;
        private Button btnAddBranch; // Yeni şube ekle butonu
        private Button btnDeleteBranch; // Seçili şubeyi sil butonu
        private TextBox txtSearch;


        // Mevcut şubeler ve ürünler listesi (MongoDB'den gelecek)
        private List<Branch> _branches = new List<Branch>();
        private List<Product> _products = new List<Product>();


        public Form1()
        {
            InitializeComponent();

            // MongoDB servisini başlat
            var settings = new MongoDBSettings();
            _mongoDBService = new MongoDBService(settings);

            // Formun boyutunu ve başlığını ayarlayalım
            this.Text = "Stok Takip Sistemi";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeCustomComponents(); // UI bileşenlerini başlat

            // Form yüklendiğinde şubeleri dolduralım
            this.Load += Form1_Load; // Form yüklendiğinde çalışacak metot
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            _isLoadingInitialData = true; // <-- BU SATIRI EKLEYİN!
    await LoadBranchesAsync();
    _isLoadingInitialData = false; // <-- BU SATIRI EKLEYİN!
        }

        // InitializeCustomComponents() metodu (DEĞİŞTİRECEĞİNİZ KISIM)
        private void InitializeCustomComponents()
{
    // 1. Şube Seçimi (ComboBox)
    cmbBranches = new ComboBox();
    cmbBranches.Location = new Point(20, 20);
    cmbBranches.Size = new Size(200, 30);
    cmbBranches.DropDownStyle = ComboBoxStyle.DropDownList;
    cmbBranches.SelectedIndexChanged += CmbBranches_SelectedIndexChanged;
    cmbBranches.Anchor = AnchorStyles.Top | AnchorStyles.Left; // Üst ve sola sabit kalsın
    this.Controls.Add(cmbBranches);

    // Şube etiketini ekleyelim
    Label lblBranch = new Label();
    lblBranch.Text = "Şube Seçin:";
    lblBranch.Location = new Point(20, 5);
    lblBranch.AutoSize = true;
    lblBranch.Anchor = AnchorStyles.Top | AnchorStyles.Left; // Üst ve sola sabit kalsın
    this.Controls.Add(lblBranch);

    // Şube Ekle ve Sil Butonları
    btnAddBranch = new Button();
    btnAddBranch.Text = "Yeni Şube Ekle";
    btnAddBranch.Location = new Point(cmbBranches.Right + 20, 20);
    btnAddBranch.Size = new Size(120, 30);
    btnAddBranch.Click += BtnAddBranch_Click;
    btnAddBranch.Anchor = AnchorStyles.Top | AnchorStyles.Left; // Üst ve sola sabit kalsın
    this.Controls.Add(btnAddBranch);

    btnDeleteBranch = new Button();
    btnDeleteBranch.Text = "Seçili Şubeyi Sil";
    btnDeleteBranch.Location = new Point(btnAddBranch.Right + 10, 20);
    btnDeleteBranch.Size = new Size(150, 30);
    btnDeleteBranch.Click += BtnDeleteBranch_Click;
    btnDeleteBranch.Anchor = AnchorStyles.Top | AnchorStyles.Left; // Üst ve sola sabit kalsın
    this.Controls.Add(btnDeleteBranch);

    // Ürün Arama Bileşenleri
    // Arama bileşenlerini şube butonlarının sağından başlayarak konumlandırıyoruz.
    // Bu, form genişlediğinde sağa doğru yayılmalarına olanak tanır.
    int searchStartX = btnDeleteBranch.Right + 30; // btnDeleteBranch'ın sağından 30 piksel sonra
    int searchY = 20; // Şube butonlarıyla aynı Y hizası

    Label lblSearch = new Label();
    lblSearch.Text = "Ürün Ara:";
    lblSearch.Location = new Point(searchStartX, searchY + 5); // Label'ı dikey olarak biraz ortala
    lblSearch.AutoSize = true;
    lblSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left; // Sol üstte kalsın (bu durumda, göreceli sol)
    this.Controls.Add(lblSearch);

    txtSearch = new TextBox();
    txtSearch.Location = new Point(lblSearch.Right + 5, searchY);
    txtSearch.Size = new Size(200, 25);
    txtSearch.TextChanged += TxtSearch_TextChanged;
    txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; // Üstte kalsın, soldan ve sağdan genişlesin
    this.Controls.Add(txtSearch);

    Button btnSearch = new Button();
    btnSearch.Text = "Ara";
    btnSearch.Location = new Point(txtSearch.Right + 2, searchY);
    btnSearch.Size = new Size(60, 25);
    btnSearch.Click += BtnSearch_Click;
    btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right; // Üst ve sağa sabit kalsın
    this.Controls.Add(btnSearch);

    // 2. Ürün Listesi (DataGridView)
    // DataGridView'in başlangıç Y konumunu yukarıdaki elemanların altına göre ayarla.
    // Tüm üst kontrollerin en alt noktasını bulup 20 piksel boşluk bırakıyoruz.
    int topControlsBottom = Math.Max(cmbBranches.Bottom, btnDeleteBranch.Bottom);
    topControlsBottom = Math.Max(topControlsBottom, btnSearch.Bottom); // Arama butonunun altını da kontrol et

    dgvProducts = new DataGridView();
    dgvProducts.Location = new Point(20, topControlsBottom + 20); // 20 piksel boşluk bırak
    dgvProducts.Size = new Size(850, 400); // İlk boyutu ayarla, Anchor ile değişecek
    dgvProducts.AllowUserToAddRows = false;
    dgvProducts.AllowUserToDeleteRows = false;
    dgvProducts.ReadOnly = true;
    dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
    dgvProducts.MultiSelect = false;
    dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    dgvProducts.RowHeadersVisible = false;
    dgvProducts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right; // Tüm kenarlara demirlenerek dinamik büyüsün
    this.Controls.Add(dgvProducts);

    // Sütunları tanımla
    dgvProducts.Columns.Clear();
    dgvProducts.Columns.Add("Id", "ID");
    dgvProducts.Columns["Id"].Visible = false;
    dgvProducts.Columns.Add("ProductName", "Ürün Adı");
    dgvProducts.Columns.Add("Barcode", "Barkod");
    dgvProducts.Columns.Add("StockQuantity", "Stok Adedi");

    DataGridViewButtonColumn addStockColumn = new DataGridViewButtonColumn();
    addStockColumn.Name = "AddStock";
    addStockColumn.HeaderText = "Stok Ekle";
    addStockColumn.Text = "+";
    addStockColumn.UseColumnTextForButtonValue = true;
    dgvProducts.Columns.Add(addStockColumn);

    DataGridViewButtonColumn decreaseStockColumn = new DataGridViewButtonColumn();
    decreaseStockColumn.Name = "DecreaseStock";
    decreaseStockColumn.HeaderText = "Stok Düş";
    decreaseStockColumn.Text = "-";
    decreaseStockColumn.UseColumnTextForButtonValue = true;
    dgvProducts.Columns.Add(decreaseStockColumn);

    dgvProducts.CellContentClick += DgvProducts_CellContentClick;

    // 3. Buttonlar (Ürün Ekle, Düzenle, Sil)
    // Bu butonları dgvProducts'ın altına göre konumlandırıyoruz.
    int buttonY = dgvProducts.Bottom + 20;

    btnAddProduct = new Button();
    btnAddProduct.Text = "Yeni Ürün Ekle";
    btnAddProduct.Location = new Point(20, buttonY);
    btnAddProduct.Size = new Size(120, 35);
    btnAddProduct.Click += BtnAddProduct_Click;
    btnAddProduct.Anchor = AnchorStyles.Bottom | AnchorStyles.Left; // Alt ve sola demirlensin
    this.Controls.Add(btnAddProduct);

    btnEditProduct = new Button();
    btnEditProduct.Text = "Ürünü Düzenle";
    btnEditProduct.Location = new Point(btnAddProduct.Right + 10, buttonY);
    btnEditProduct.Size = new Size(120, 35);
    btnEditProduct.Click += BtnEditProduct_Click;
    btnEditProduct.Anchor = AnchorStyles.Bottom | AnchorStyles.Left; // Alt ve sola demirlensin
    this.Controls.Add(btnEditProduct);

    btnDeleteProduct = new Button();
    btnDeleteProduct.Text = "Ürünü Sil";
    btnDeleteProduct.Location = new Point(btnEditProduct.Right + 10, buttonY);
    btnDeleteProduct.Size = new Size(120, 35);
    btnDeleteProduct.Click += BtnDeleteProduct_Click;
    btnDeleteProduct.Anchor = AnchorStyles.Bottom | AnchorStyles.Left; // Alt ve sola demirlensin
    this.Controls.Add(btnDeleteProduct);

    // Formun minimum boyutunu ayarlayarak kontrollerin çok küçülmesini engelleyebiliriz.
    this.MinimumSize = new Size(920, 650); // Tüm elemanların rahat sığacağı bir minimum boyut
}
        // Şubeleri MongoDB'den yükle (asenkron)
        private async Task LoadBranchesAsync()
        {
            try
            {
                // Mevcut seçimi geçici olarak sakla
                string currentSelectedBranchId = (cmbBranches.SelectedItem as Branch)?.Id;

                // ComboBox'ın DataSource'unu ayarlamadan önce SelectedIndexChanged olayını geçici olarak devre dışı bırak
                cmbBranches.SelectedIndexChanged -= CmbBranches_SelectedIndexChanged;

                _branches = await _mongoDBService.GetBranchesAsync();

                cmbBranches.DataSource = _branches;
                cmbBranches.DisplayMember = "Name";
                cmbBranches.ValueMember = "Id";

                if (_branches.Any()) // Şubeler varsa
                {
                    // Eğer önceden seçili bir şube varsa onu korumaya çalış
                    if (!string.IsNullOrEmpty(currentSelectedBranchId) && _branches.Any(b => b.Id == currentSelectedBranchId))
                    {
                        cmbBranches.SelectedValue = currentSelectedBranchId;
                    }
                    else
                    {
                        // İlk şubeyi seç (bu, SelectedIndexChanged olayını tetikleyecek)
                        cmbBranches.SelectedIndex = 0;
                    }
                    // Ürünleri yükle (ilk açılışta veya şube değiştiğinde)
                    if (cmbBranches.SelectedItem is Branch selectedBranch)
                    {
                        await LoadProductsByBranchAsync(selectedBranch.Id);
                    }
                }
                else // Hiç şube yoksa
                {
                    MessageBox.Show("Veritabanında hiç şube bulunamadı. Lütfen 'Yeni Şube Ekle' özelliğini kullanarak şube ekleyin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cmbBranches.DataSource = null; // ComboBox'ı temizle
                    dgvProducts.Rows.Clear(); // DataGridView'i temizle
                }

                // Olayı tekrar etkinleştir
                cmbBranches.SelectedIndexChanged += CmbBranches_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şubeler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Şube seçimi değiştiğinde (asenkron)
         private async void CmbBranches_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Eğer ilk yükleme devam ediyorsa, bu olayı şimdilik yok say.
            // LoadBranchesAsync içindeki DataSource ataması ve SelectedIndex = 0;
            // bu olayı tetikleyebilir.
            if (_isLoadingInitialData) return; // <-- BU SATIR KALSIN!

            if (cmbBranches.SelectedItem is Branch selectedBranch)
            {
                // Seçili şube ID'sini Tag özelliğinde sakla
                cmbBranches.Tag = selectedBranch.Id;
                await LoadProductsByBranchAsync(selectedBranch.Id);
            }
            else
            {
                dgvProducts.Rows.Clear(); // Şube seçili değilse ürünleri temizle
            }
        }
        // Yeni Şube Ekle Butonu
        private async void BtnAddBranch_Click(object sender, EventArgs e)
        {
            using (AddEditBranchForm branchForm = new AddEditBranchForm())
            {
                if (branchForm.ShowDialog() == DialogResult.OK)
                {
                    string newBranchName = branchForm.BranchName;
                    if (!string.IsNullOrEmpty(newBranchName))
                    {
                        try
                        {
                            Branch newBranch = new Branch { Name = newBranchName };
                            await _mongoDBService.InsertBranchAsync(newBranch);
                            MessageBox.Show($"{newBranchName} şubesi başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadBranchesAsync(); // Şubeleri yeniden yükle
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Şube eklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        // Seçili Şubeyi Sil Butonu
        private async void BtnDeleteBranch_Click(object sender, EventArgs e)
        {
            // ComboBox'tan seçili şubeyi al
            if (cmbBranches.SelectedItem is Branch selectedBranch)
            {
                // Kullanıcıdan silme onayı al
                DialogResult dialogResult = MessageBox.Show(
                    $"'{selectedBranch.Name}' şubesini silmek istediğinizden emin misiniz?\n" +
                    "Bu şubeye ait tüm ürün stok bilgileri de etkilenecektir.",
                    "Şube Silme Onayı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        // MongoDB'den şubeyi sil
                        await _mongoDBService.DeleteBranchAsync(selectedBranch.Id);

                        // Bu şubeye ait ürünlerin stok bilgilerini de güncellemek (silmek) gerekir.
                        // Her üründeki BranchStocks listesinden ilgili BranchStock elemanını silmeliyiz.
                        // Bunun için _mongoDBService'de özel bir metot çağıralım:
                        await _mongoDBService.RemoveProductStockFromBranchAsync(null, selectedBranch.Id); // branchId'ye göre tüm ürünleri etkiler

                        MessageBox.Show($"'{selectedBranch.Name}' şubesi başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Şubeleri yeniden yükle ve DataGridView'i temizle
                        await LoadBranchesAsync();
                        dgvProducts.Rows.Clear(); // Şube silindiği için ürün listesini temizle
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Şube silinirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek istediğiniz bir şube seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        // Şubeye göre ürünleri MongoDB'den yükle (asenkron)
        private async Task LoadProductsByBranchAsync(string branchId, string searchTerm = null)
{
    dgvProducts.Rows.Clear(); // Önceki verileri temizle
    try
    {
        // Tüm ürünleri çekelim ve sonra filtreleyelim (Performans için MongoDB tarafında filtrelemek daha iyi olabilir)
        // Ancak bu örnek için önce tüm ürünleri çekip bellekte filtreleyelim.
        _products = await _mongoDBService.GetProductsByBranchAsync(branchId);

        IEnumerable<Product> filteredProducts = _products;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower(); // Büyük/küçük harf duyarsız arama için
            filteredProducts = _products.Where(p =>
                p.ProductName.ToLower().Contains(searchTerm) ||
                p.Barcode.ToLower().Contains(searchTerm));
        }

        foreach (var product in filteredProducts)
        {
            var branchStock = product.BranchStocks.FirstOrDefault(bs => bs.BranchId == branchId);
            int stockQuantity = branchStock?.Stock ?? 0;

            dgvProducts.Rows.Add(product.Id, product.ProductName, product.Barcode, stockQuantity);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Ürünler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
        private async void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // ComboBox'tan seçili şubeyi al
            if (cmbBranches.SelectedItem is Branch selectedBranch)
            {
                string searchTerm = ((TextBox)sender).Text;
                await LoadProductsByBranchAsync(selectedBranch.Id, searchTerm);
            }
        }
    private async void BtnSearch_Click(object sender, EventArgs e)
{
    // ComboBox'tan seçili şubeyi al
    if (cmbBranches.SelectedItem is Branch selectedBranch)
    {
        string searchTerm = txtSearch.Text; // txtSearch'in adını doğru yazdığınızdan emin olun
        await LoadProductsByBranchAsync(selectedBranch.Id, searchTerm);
    }
}
        // DataGridView'deki Stok Ekle/Düş Butonlarına Tıklama Olayı (asenkron)
        private async void DgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var senderGrid = (DataGridView)sender;

                if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                    e.RowIndex < senderGrid.Rows.Count)
                {
                    string productId = dgvProducts.Rows[e.RowIndex].Cells["Id"].Value?.ToString();
                    string productName = dgvProducts.Rows[e.RowIndex].Cells["ProductName"].Value?.ToString();

                    // Mevcut stok miktarını BranchStocks listesinden al
                    // Önce ürünü _products listesinden bul
                    var productInMem = _products.FirstOrDefault(p => p.Id == productId);
                    int currentStock = 0;
                    string selectedBranchId = (cmbBranches.SelectedItem as Branch)?.Id;

                    if (productInMem != null && !string.IsNullOrEmpty(selectedBranchId))
                    {
                        // Seçili şubeye ait BranchStock nesnesini bul ve stoğunu al
                        currentStock = productInMem.BranchStocks.FirstOrDefault(bs => bs.BranchId == selectedBranchId)?.Stock ?? 0;
                    }
                    else
                    {
                        MessageBox.Show("Ürün veya şube bilgisi eksik.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (string.IsNullOrEmpty(productId)) return;

                    if (string.IsNullOrEmpty(selectedBranchId))
                    {
                        MessageBox.Show("Lütfen bir şube seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (senderGrid.Columns[e.ColumnIndex].Name == "AddStock")
                    {
                        await ShowStockUpdateDialogAsync(productId, productName, selectedBranchId, currentStock, "add");
                    }
                    else if (senderGrid.Columns[e.ColumnIndex].Name == "DecreaseStock")
                    {
                        await ShowStockUpdateDialogAsync(productId, productName, selectedBranchId, currentStock, "decrease");
                    }
                }
            }
        }

        // Stok güncelleme dialogu (asenkron)
        private async Task ShowStockUpdateDialogAsync(string productId, string productName, string branchId, int currentStock, string operationType)
        {
            using (Form inputForm = new Form())
            {
                inputForm.Text = (operationType == "add" ? "Stok Ekle" : "Stok Düş");
                inputForm.Size = new Size(300, 180); // Form boyutunu artırdım
                inputForm.StartPosition = FormStartPosition.CenterParent;

                Label lblInfo = new Label();
                lblInfo.Text = $"Ürün: {productName}\nMevcut Stok: {currentStock}";
                lblInfo.Location = new Point(10, 10);
                lblInfo.AutoSize = true;
                inputForm.Controls.Add(lblInfo);

                Label lblQuantity = new Label();
                lblQuantity.Text = "Miktar:";
                lblQuantity.Location = new Point(10, lblInfo.Bottom + 10);
                lblQuantity.AutoSize = true;
                inputForm.Controls.Add(lblQuantity);

                NumericUpDown numQuantity = new NumericUpDown();
                numQuantity.Location = new Point(10, lblQuantity.Bottom + 5);
                numQuantity.Minimum = 1;
                numQuantity.Maximum = (operationType == "decrease" ? currentStock : 100000); // Max. değeri güncel stok veya yüksek bir limit
                numQuantity.Size = new Size(100, 25);
                inputForm.Controls.Add(numQuantity);

                Button btnOk = new Button();
                btnOk.Text = "Tamam";
                btnOk.Location = new Point(10, numQuantity.Bottom + 10);
                btnOk.DialogResult = DialogResult.OK;
                inputForm.Controls.Add(btnOk);

                Button btnCancel = new Button();
                btnCancel.Text = "İptal";
                btnCancel.Location = new Point(btnOk.Right + 10, numQuantity.Bottom + 10);
                btnCancel.DialogResult = DialogResult.Cancel;
                inputForm.Controls.Add(btnCancel);

                inputForm.AcceptButton = btnOk;
                inputForm.CancelButton = btnCancel;

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    int quantity = (int)numQuantity.Value;
                    await UpdateProductStockAsync(productId, branchId, quantity, operationType);
                }
            }
        }

        // Stok güncelleme metodu (MongoDB ile entegre)
        private async Task UpdateProductStockAsync(string productId, string branchId, int quantity, string operationType)
        {
            try
            {
                // Dikkat: MongoDBService'de zaten negatif stok kontrolü var.
                // Burada tekrar yapmaya gerek olmayabilir, ancak UI tarafında kullanıcıya
                // anında geri bildirim vermek için mantıklı olabilir.
                // Eğer _mongoDBService'deki kontrol yeterliyse bu kısmı kaldırabilirsiniz.
                if (operationType == "decrease")
                {
                    var productInMem = _products.FirstOrDefault(p => p.Id == productId);
                    if (productInMem != null)
                    {
                        var branchStock = productInMem.BranchStocks.FirstOrDefault(bs => bs.BranchId == branchId);
                        if (branchStock != null && branchStock.Stock - quantity < 0)
                        {
                            MessageBox.Show("Stok miktarı sıfırın altına düşemez!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                await _mongoDBService.UpdateProductStockAsync(productId, branchId, quantity, operationType);

                MessageBox.Show("Stok başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadProductsByBranchAsync(branchId); // Güncel stoğu göstermek için listeyi yenile
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Stok güncellenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Yeni Ürün Ekle Butonu
        private async void BtnAddProduct_Click(object sender, EventArgs e)
        {
            if (!_branches.Any()) // Hiç şube yoksa ürün ekletme
            {
                MessageBox.Show("Ürün eklemek için öncelikle en az bir şube eklemelisiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // AddEditProductForm'u yeni ürün ekleme modunda aç
            using (AddEditProductForm productForm = new AddEditProductForm(_branches))
            {
                if (productForm.ShowDialog() == DialogResult.OK)
                {
                    Product newProduct = productForm.EditedProduct;
                    if (newProduct != null)
                    {
                        try
                        {
                            // Barkodun benzersizliğini kontrol et (önerilir)
                            var allProducts = await _mongoDBService.GetProductsAsync();
                            if (allProducts.Any(p => p.Barcode == newProduct.Barcode))
                            {
                                MessageBox.Show("Bu barkoda sahip bir ürün zaten mevcut.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            await _mongoDBService.InsertProductAsync(newProduct);
                            MessageBox.Show($"{newProduct.ProductName} adlı ürün başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Seçili şubeye göre ürün listesini yeniden yükle
                            if (cmbBranches.SelectedItem is Branch selectedBranch)
                            {
                                await LoadProductsByBranchAsync(selectedBranch.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ürün eklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        // Ürün Düzenle Butonu
        private async void BtnEditProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                string productId = dgvProducts.SelectedRows[0].Cells["Id"].Value?.ToString();

                // _products listesinden ürünü çek (MongoDB'den çekmeye gerek yok, zaten bellekte var)
                Product productToEdit = _products.FirstOrDefault(p => p.Id == productId);

                if (productToEdit == null)
                {
                    MessageBox.Show("Düzenlenecek ürün bulunamadı. Lütfen sayfayı yenileyip tekrar deneyin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // AddEditProductForm'u düzenleme modunda aç
                using (AddEditProductForm productForm = new AddEditProductForm(_branches, productToEdit))
                {
                    if (productForm.ShowDialog() == DialogResult.OK)
                    {
                        Product updatedProduct = productForm.EditedProduct;
                        if (updatedProduct != null)
                        {
                            try
                            {
                                // Barkodun benzersizliğini kontrol et (kendisi hariç)
                                var allProducts = await _mongoDBService.GetProductsAsync();
                                if (allProducts.Any(p => p.Barcode == updatedProduct.Barcode && p.Id != updatedProduct.Id))
                                {
                                    MessageBox.Show("Bu barkoda sahip başka bir ürün zaten mevcut.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                await _mongoDBService.UpdateProductAsync(updatedProduct.Id, updatedProduct);
                                MessageBox.Show($"{updatedProduct.ProductName} adlı ürün başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Seçili şubeye göre ürün listesini yeniden yükle
                                if (cmbBranches.SelectedItem is Branch selectedBranch)
                                {
                                    await LoadProductsByBranchAsync(selectedBranch.Id);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ürün güncellenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen düzenlemek istediğiniz ürünü seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Ürün Sil Butonu (MongoDB ile entegre)
        private async void BtnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                string productId = dgvProducts.SelectedRows[0].Cells["Id"].Value?.ToString();
                string productName = dgvProducts.SelectedRows[0].Cells["ProductName"].Value?.ToString();

                DialogResult dialogResult = MessageBox.Show($"Seçili ürünü ({productName}) silmek istediğinizden emin misiniz?", "Ürün Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        await _mongoDBService.DeleteProductAsync(productId);
                        MessageBox.Show("Ürün başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // DataGridView'i yenile
                        string selectedBranchId = (cmbBranches.SelectedItem as Branch)?.Id;
                        if (!string.IsNullOrEmpty(selectedBranchId))
                        {
                            await LoadProductsByBranchAsync(selectedBranchId);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ürün silinirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek istediğiniz ürünü seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}