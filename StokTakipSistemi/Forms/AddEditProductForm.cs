using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StokTakipSistemi
{
    public partial class AddEditProductForm : Form
    {
        private TextBox txtProductName;
        private TextBox txtBarcode;
        private NumericUpDown numStock;
        private NumericUpDown numPrice; // Fiyat için eklendi
        private ComboBox cmbBranches;
        private Label lblBranchSelect;
        private Label lblStock;
        private Button btnSave;
        private Button btnCancel;

        public Product EditedProduct { get; private set; }
        private List<Branch> _availableBranches;
        private bool _isEditMode;

        // Ekleme Modu
        public AddEditProductForm(List<Branch> availableBranches)
        {
            InitializeComponent();
            _availableBranches = availableBranches;
            _isEditMode = false;
            InitializeFormComponents();
            this.Text = "Yeni Ürün Ekle";
        }

        // Düzenleme Modu
        public AddEditProductForm(List<Branch> availableBranches, Product productToEdit)
        {
            InitializeComponent();
            _availableBranches = availableBranches;
            EditedProduct = productToEdit;
            _isEditMode = true;
            InitializeFormComponents();
            this.Text = "Ürün Düzenle";
            PopulateFormWithProductData(productToEdit);
        }

        private void InitializeFormComponents()
        {
            this.Size = new Size(400, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Ürün Adı
            AddLabel("Ürün Adı:", 20);
            txtProductName = new TextBox { Location = new Point(120, 17), Size = new Size(240, 25) };
            this.Controls.Add(txtProductName);

            // Barkod
            AddLabel("Barkod:", 50);
            txtBarcode = new TextBox { Location = new Point(120, 47), Size = new Size(240, 25) };
            this.Controls.Add(txtBarcode);

            // Fiyat
            AddLabel("Fiyat (TL):", 80);
            numPrice = new NumericUpDown { Location = new Point(120, 77), Size = new Size(100, 25), DecimalPlaces = 2, Maximum = 1000000 };
            this.Controls.Add(numPrice);

            // Stok Miktarı
            lblStock = AddLabel("Stok Miktarı:", 110);
            numStock = new NumericUpDown { Location = new Point(120, 107), Size = new Size(100, 25), Maximum = 100000 };
            this.Controls.Add(numStock);

            // Şube Seçimi
            lblBranchSelect = AddLabel("Şube Seçin:", 140);
            cmbBranches = new ComboBox { Location = new Point(120, 137), Size = new Size(240, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbBranches);

            // Şube Listesini Doldur
            if (_availableBranches != null && _availableBranches.Any())
            {
                cmbBranches.DataSource = new BindingSource(_availableBranches, null);
                cmbBranches.DisplayMember = "Name";
                cmbBranches.ValueMember = "Id";
            }

            // Kaydet / İptal
            btnSave = new Button { Text = "Kaydet", Location = new Point(120, 190), Size = new Size(100, 35) };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button { Text = "İptal", Location = new Point(230, 190), Size = new Size(100, 35) };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private Label AddLabel(string text, int y)
        {
            Label lbl = new Label { Text = text, Location = new Point(20, y + 3), AutoSize = true };
            this.Controls.Add(lbl);
            return lbl;
        }

        private void PopulateFormWithProductData(Product product)
        {
            txtProductName.Text = product.ProductName;
            txtBarcode.Text = product.Barcode;
            numPrice.Value = product.Price;
            numStock.Value = product.Quantity;
            
            if (cmbBranches.Items.Count > 0)
                cmbBranches.SelectedValue = product.BranchId;

            // Düzenleme modunda şube değiştirmeyi istersen açık bırakabilirsin
            // İstersen: cmbBranches.Enabled = false; 
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductName.Text) || string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.");
                return;
            }

            if (cmbBranches.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bir şube seçin.");
                return;
            }

            if (!_isEditMode)
            {
                EditedProduct = new Product();
            }

            EditedProduct.ProductName = txtProductName.Text.Trim();
            EditedProduct.Barcode = txtBarcode.Text.Trim();
            EditedProduct.Price = numPrice.Value;
            EditedProduct.Quantity = (int)numStock.Value;
            EditedProduct.BranchId = (int)cmbBranches.SelectedValue;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}