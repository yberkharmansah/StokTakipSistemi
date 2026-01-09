using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace StokTakipSistemi
{
    public partial class Form1 : Form
    {
        // UI Bileşenleri
        private ComboBox cmbBranches;
        private DataGridView dgvProducts;
        private Button btnAddProduct;
        private Button btnEditProduct;
        private Button btnDeleteProduct;
        private Button btnAddBranch;
        private Button btnDeleteBranch;
        private TextBox txtSearch;

        private List<Branch> _branches = new List<Branch>();
        private bool _isLoading = false;

        public Form1()
        {
            // Designer dosyasındaki InitializeComponent çağrılır.
            // Eğer orada butonlar tanımlıysa, InitializeCustomComponents ile çakışmaması için
            // Designer dosyasını (Form1.Designer.cs) temiz tutmalısınız.
            InitializeComponent();

            this.Text = "Stok Takip Sistemi - SQLite";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeCustomComponents();
            
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadBranches();
        }

        private void InitializeCustomComponents()
        {
            this.Controls.Clear(); // Çift buton oluşmaması için önce formu temizliyoruz.

            // 1. Şube Paneli
            Label lblBranch = new Label { Text = "Şube Seçin:", Location = new Point(20, 10), AutoSize = true };
            this.Controls.Add(lblBranch);

            cmbBranches = new ComboBox { Location = new Point(20, 30), Size = new Size(200, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbBranches.SelectedIndexChanged += CmbBranches_SelectedIndexChanged;
            this.Controls.Add(cmbBranches);

            btnAddBranch = new Button { Text = "Yeni Şube Ekle", Location = new Point(230, 28), Size = new Size(120, 30) };
            btnAddBranch.Click += BtnAddBranch_Click;
            this.Controls.Add(btnAddBranch);

            btnDeleteBranch = new Button { Text = "Seçili Şubeyi Sil", Location = new Point(355, 28), Size = new Size(130, 30) };
            btnDeleteBranch.Click += BtnDeleteBranch_Click;
            this.Controls.Add(btnDeleteBranch);

            // 2. Arama Paneli
            Label lblSearch = new Label { Text = "Ürün Ara:", Location = new Point(550, 33), AutoSize = true };
            this.Controls.Add(lblSearch);

            txtSearch = new TextBox { Location = new Point(615, 30), Size = new Size(200, 25) };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            this.Controls.Add(txtSearch);

            // 3. Veri Tablosu (DataGridView)
            dgvProducts = new DataGridView
            {
                Location = new Point(20, 80),
                Size = new Size(890, 430),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(dgvProducts);

            // Kolonları oluştur
            dgvProducts.Columns.Add("Id", "ID");
            dgvProducts.Columns["Id"].Visible = false;
            dgvProducts.Columns.Add("ProductName", "Ürün Adı");
            dgvProducts.Columns.Add("Barcode", "Barkod");
            dgvProducts.Columns.Add("Price", "Fiyat");
            dgvProducts.Columns.Add("Quantity", "Stok");

            // Stok butonları
            AddGridButton("+", "AddStock", "Stok Ekle");
            AddGridButton("-", "DecreaseStock", "Stok Düş");

            dgvProducts.CellContentClick += DgvProducts_CellContentClick;

            // 4. Alt Butonlar
            int btnY = 530;
            btnAddProduct = new Button { Text = "Yeni Ürün Ekle", Location = new Point(20, btnY), Size = new Size(130, 40), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnAddProduct.Click += BtnAddProduct_Click;
            this.Controls.Add(btnAddProduct);

            btnEditProduct = new Button { Text = "Ürünü Düzenle", Location = new Point(160, btnY), Size = new Size(130, 40), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnEditProduct.Click += BtnEditProduct_Click;
            this.Controls.Add(btnEditProduct);

            btnDeleteProduct = new Button { Text = "Ürünü Sil", Location = new Point(300, btnY), Size = new Size(130, 40), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnDeleteProduct.Click += BtnDeleteProduct_Click;
            this.Controls.Add(btnDeleteProduct);
        }

        private void AddGridButton(string text, string name, string header)
        {
            DataGridViewButtonColumn btn = new DataGridViewButtonColumn
            {
                Name = name,
                HeaderText = header,
                Text = text,
                UseColumnTextForButtonValue = true,
                Width = 70
            };
            dgvProducts.Columns.Add(btn);
        }

        private void LoadBranches()
        {
            _isLoading = true;
            using (var db = new AppDbContext())
            {
                _branches = db.Branches.ToList();
                cmbBranches.DataSource = null;
                cmbBranches.DataSource = _branches;
                cmbBranches.DisplayMember = "Name";
                cmbBranches.ValueMember = "Id";
            }
            _isLoading = false;

            if (_branches.Any())
                LoadProducts();
        }

        private void LoadProducts(string searchTerm = null)
        {
            if (cmbBranches.SelectedValue == null) return;

            int branchId = (int)cmbBranches.SelectedValue;
            dgvProducts.Rows.Clear();

            using (var db = new AppDbContext())
            {
                var query = db.Products.Where(p => p.BranchId == branchId);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(p => p.ProductName.ToLower().Contains(searchTerm) || p.Barcode.Contains(searchTerm));
                }

                var products = query.ToList();
                foreach (var p in products)
                {
                    dgvProducts.Rows.Add(p.Id, p.ProductName, p.Barcode, p.Price.ToString("C2"), p.Quantity);
                }
            }
        }

        private void CmbBranches_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoading) LoadProducts();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadProducts(txtSearch.Text);
        }

        private void BtnAddBranch_Click(object sender, EventArgs e)
        {
            using (var branchForm = new AddEditBranchForm())
            {
                if (branchForm.ShowDialog() == DialogResult.OK)
                {
                    using (var db = new AppDbContext())
                    {
                        db.Branches.Add(new Branch { Name = branchForm.BranchName });
                        db.SaveChanges();
                    }
                    LoadBranches();
                }
            }
        }

        private void BtnDeleteBranch_Click(object sender, EventArgs e)
        {
            if (cmbBranches.SelectedItem is Branch selected)
            {
                var result = MessageBox.Show($"{selected.Name} şubesini ve içindeki tüm ürünleri silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    using (var db = new AppDbContext())
                    {
                        db.Branches.Remove(selected);
                        db.SaveChanges();
                    }
                    LoadBranches();
                }
            }
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            if (cmbBranches.SelectedItem == null) return;

            using (var form = new AddEditProductForm(_branches))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    using (var db = new AppDbContext())
                    {
                        db.Products.Add(form.EditedProduct);
                        db.SaveChanges();
                    }
                    LoadProducts();
                }
            }
        }

        private void BtnEditProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) return;

            int productId = (int)dgvProducts.SelectedRows[0].Cells["Id"].Value;
            using (var db = new AppDbContext())
            {
                var product = db.Products.Find(productId);
                using (var form = new AddEditProductForm(_branches, product))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        db.Entry(product).State = EntityState.Modified;
                        db.SaveChanges();
                        LoadProducts();
                    }
                }
            }
        }

        private void BtnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) return;

            int productId = (int)dgvProducts.SelectedRows[0].Cells["Id"].Value;
            if (MessageBox.Show("Ürünü silmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (var db = new AppDbContext())
                {
                    var product = db.Products.Find(productId);
                    db.Products.Remove(product);
                    db.SaveChanges();
                }
                LoadProducts();
            }
        }

        private void DgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
{
    if (e.RowIndex < 0) return;

    int productId = (int)dgvProducts.Rows[e.RowIndex].Cells["Id"].Value;
    string productName = dgvProducts.Rows[e.RowIndex].Cells["ProductName"].Value.ToString();
    string colName = dgvProducts.Columns[e.ColumnIndex].Name;

    // Sadece + veya - butonlarına tıklandığında işlem yap
    if (colName == "AddStock" || colName == "DecreaseStock")
    {
        string islemTipi = colName == "AddStock" ? "Eklenecek" : "Düşülecek";
        
        // Kullanıcıdan miktar almak için küçük bir form oluşturuyoruz
        using (Form miktarForm = new Form())
        {
            miktarForm.Text = "Stok Miktarı Girin";
            miktarForm.Size = new Size(250, 150);
            miktarForm.StartPosition = FormStartPosition.CenterParent;
            miktarForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            miktarForm.MaximizeBox = false;

            Label lbl = new Label { Text = $"{productName}\n{islemTipi} Miktar:", Location = new Point(10, 10), AutoSize = true };
            NumericUpDown numMiktar = new NumericUpDown { Location = new Point(10, 50), Width = 100, Minimum = 1, Maximum = 10000 };
            Button btnOnay = new Button { Text = "Tamam", Location = new Point(10, 80), DialogResult = DialogResult.OK };
            
            miktarForm.Controls.AddRange(new Control[] { lbl, numMiktar, btnOnay });
            miktarForm.AcceptButton = btnOnay;

            if (miktarForm.ShowDialog() == DialogResult.OK)
            {
                int girilenMiktar = (int)numMiktar.Value;

                using (var db = new AppDbContext())
                {
                    var product = db.Products.Find(productId);
                    if (product != null)
                    {
                        if (colName == "AddStock")
                        {
                            product.Quantity += girilenMiktar;
                        }
                        else // DecreaseStock
                        {
                            if (product.Quantity >= girilenMiktar)
                            {
                                product.Quantity -= girilenMiktar;
                            }
                            else
                            {
                                MessageBox.Show("Yetersiz stok! Mevcut stoktan daha fazla düşüş yapamazsınız.", "Uyarı");
                                return;
                            }
                        }
                        db.SaveChanges();
                    }
                }
                LoadProducts(); // Tabloyu yenile
            }
        }
    }
}
    }
}