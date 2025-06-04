using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MongoDB.Bson; // ObjectId için

namespace StokTakipSistemi
{
    // Daha önce tanımladığınız BranchStock sınıfı


    public partial class AddEditProductForm : Form
    {
        private TextBox txtProductName;
        private TextBox txtBarcode;
        private NumericUpDown numStock;
        private ComboBox cmbBranches; // Hangi şubeye stok ekleneceği seçimi için
        private Label lblBranchSelect; // Şube seçimi etiketi
        private Label lblStock; // Stok miktarı etiketi
        private Button btnSave;
        private Button btnCancel;

        public Product EditedProduct { get; private set; } // Düzenlenen veya eklenen ürünü döndürmek için
        private List<Branch> _availableBranches; // Ana formdan gelecek şube listesi
        private bool _isEditMode; // Düzenleme modunda mı ekleme modunda mı

        // Constructor for Add (Ekleme Modu)
        public AddEditProductForm(List<Branch> availableBranches)
        {
            InitializeComponent();
            _availableBranches = availableBranches;
            _isEditMode = false;
            InitializeFormComponents();
            this.Text = "Yeni Ürün Ekle";

            // Ekleme modunda stok ve şube seçimi görünür olmalı
            lblBranchSelect.Visible = true;
            cmbBranches.Visible = true;
            numStock.Visible = true;
            lblStock.Text = "Stok:"; // Etiketi uygun hale getir
            numStock.Enabled = true; // Stok miktarı girişi aktif
        }

        // Constructor for Edit (Düzenleme Modu)
        public AddEditProductForm(List<Branch> availableBranches, Product productToEdit)
        {
            InitializeComponent();
            _availableBranches = availableBranches;
            EditedProduct = productToEdit; // Düzenlenecek ürünü kaydet
            _isEditMode = true;
            InitializeFormComponents();
            this.Text = "Ürün Düzenle";
            PopulateFormWithProductData(productToEdit); // Mevcut veriyi doldur

            // Düzenleme modunda stok ve şube seçimi gizlenebilir veya devre dışı bırakılabilir
            // Çünkü stok ana ekrandan yönetilecek ve şube değişikliği ürün bazında değil.
            lblBranchSelect.Visible = false;
            cmbBranches.Visible = false;
            numStock.Visible = false; // Stok miktarını gizle
            lblStock.Text = "Mevcut Stok: (Ana ekrandan yönetilir)"; // Etiketi değiştir
            lblStock.Location = new Point(20, 80); // Konumunu ayarla (eğer stok miktarını gizlersen)
        }

        private void InitializeFormComponents()
        {
            // Form boyutunu ayarla
            this.Size = new Size(400, 230);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Ürün Adı
            Label lblProductName = new Label();
            lblProductName.Text = "Ürün Adı:";
            lblProductName.Location = new Point(20, 20);
            lblProductName.AutoSize = true;
            this.Controls.Add(lblProductName);

            txtProductName = new TextBox();
            txtProductName.Location = new Point(120, 17);
            txtProductName.Size = new Size(250, 25);
            this.Controls.Add(txtProductName);

            // Barkod
            Label lblBarcode = new Label();
            lblBarcode.Text = "Barkod:";
            lblBarcode.Location = new Point(20, 50);
            lblBarcode.AutoSize = true;
            this.Controls.Add(lblBarcode);

            txtBarcode = new TextBox();
            txtBarcode.Location = new Point(120, 47);
            txtBarcode.Size = new Size(250, 25);
            this.Controls.Add(txtBarcode);

            // Stok Miktarı (Hem başlangıç hem de düzenleme için etiketi başlangıçta oluştur)
            lblStock = new Label(); // Global olarak tanımlanan lblStock'u kullanıyoruz
            lblStock.Text = "Stok Miktarı:";
            lblStock.Location = new Point(20, 80);
            lblStock.AutoSize = true;
            this.Controls.Add(lblStock);

            numStock = new NumericUpDown();
            numStock.Location = new Point(120, 77);
            numStock.Size = new Size(100, 25);
            numStock.Minimum = 0; // Stok 0 olabilir
            numStock.Maximum = 100000;
            this.Controls.Add(numStock);

            // Şube Seçimi
            lblBranchSelect = new Label(); // Global olarak tanımlanan lblBranchSelect'i kullanıyoruz
            lblBranchSelect.Text = "Şube Seçin:";
            lblBranchSelect.Location = new Point(20, 110);
            lblBranchSelect.AutoSize = true;
            this.Controls.Add(lblBranchSelect);

            cmbBranches = new ComboBox();
            cmbBranches.Location = new Point(120, 107);
            cmbBranches.Size = new Size(250, 25);
            cmbBranches.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbBranches);

            // Şube ComboBox'ı doldur
            if (_availableBranches != null && _availableBranches.Any())
            {
                cmbBranches.DataSource = _availableBranches;
                cmbBranches.DisplayMember = "Name";
                cmbBranches.ValueMember = "Id";
                cmbBranches.SelectedIndex = 0; // İlk şubeyi varsayılan olarak seç
            }
            else
            {
                // Hiç şube yoksa uyarı göster ve şube seçimini devre dışı bırak
                lblBranchSelect.Text = "Önce Şube Ekleyin!";
                lblBranchSelect.ForeColor = Color.Red;
                cmbBranches.Enabled = false;
                numStock.Enabled = false; // Stok da eklenemez
            }

            // Kaydet ve İptal Butonları
            btnSave = new Button();
            btnSave.Text = "Kaydet";
            btnSave.Location = new Point(120, 150);
            btnSave.Size = new Size(100, 30);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button();
            btnCancel.Text = "İptal";
            btnCancel.Location = new Point(btnSave.Right + 10, 150);
            btnCancel.Size = new Size(100, 30);
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void PopulateFormWithProductData(Product product)
        {
            txtProductName.Text = product.ProductName;
            txtBarcode.Text = product.Barcode;

            // Düzenleme modunda stok ve şube seçimi kontrollerini gizle/devre dışı bırak
            // Çünkü bu form sadece ürün adı/barkodu gibi temel bilgileri düzenler,
            // stok miktarları ana ekrandan şube bazında güncellenir.
            numStock.Visible = false;
            cmbBranches.Visible = false;
            lblBranchSelect.Visible = false;
            lblStock.Text = "Mevcut Stok: (Ana ekrandan yönetilir)"; // Etiketi değiştir
        }


        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Ürün adı boş olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Barkod boş olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_isEditMode && cmbBranches.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir şube seçin veya önce şube ekleyin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isEditMode) // Düzenleme Modu
            {
                EditedProduct.ProductName = txtProductName.Text.Trim();
                EditedProduct.Barcode = txtBarcode.Text.Trim();
                // Stok düzenleme burada yapılmaz, ana ekrandan yapılır.
                // EditedProduct.BranchStocks listesi üzerinde değişiklik yapılmaz.
            }
            else // Ekleme Modu
            {
                EditedProduct = new Product
                {
                    ProductName = txtProductName.Text.Trim(),
                    Barcode = txtBarcode.Text.Trim(),
                    BranchStocks = new List<BranchStock>() // Yeni listeyi oluştur
                };

                // Başlangıç stoğunu seçilen şubeye BranchStock nesnesi olarak ekle
                if (cmbBranches.SelectedItem is Branch selectedBranch)
                {
                    EditedProduct.BranchStocks.Add(new BranchStock
                    {
                        BranchId = selectedBranch.Id,
                        Stock = (int)numStock.Value
                    });
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}