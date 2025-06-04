using System;
using System.Drawing;
using System.Windows.Forms;

namespace StokTakipSistemi
{
    public partial class AddEditBranchForm : Form
    {
        private TextBox txtBranchName;
        private Button btnSave;
        private Button btnCancel;

        public string BranchName { get; private set; } // Eklenen/Düzenlenen şube adını almak için

        public AddEditBranchForm(string currentBranchName = "")
        {
            InitializeComponent();
            this.Text = "Şube Ekle/Düzenle";
            this.Size = new Size(350, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Boyutlandırılamaz
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false; // Görev çubuğunda gösterme

            InitializeFormComponents(currentBranchName);
        }

        private void InitializeFormComponents(string currentBranchName)
        {
            Label lblName = new Label();
            lblName.Text = "Şube Adı:";
            lblName.Location = new Point(20, 20);
            lblName.AutoSize = true;
            this.Controls.Add(lblName);

            txtBranchName = new TextBox();
            txtBranchName.Location = new Point(20, 45);
            txtBranchName.Size = new Size(280, 25);
            txtBranchName.Text = currentBranchName; // Mevcut şube adını doldur (düzenleme için)
            this.Controls.Add(txtBranchName);

            btnSave = new Button();
            btnSave.Text = "Kaydet";
            btnSave.Location = new Point(20, 90);
            btnSave.Size = new Size(100, 30);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button();
            btnCancel.Text = "İptal";
            btnCancel.Location = new Point(btnSave.Right + 10, 90);
            btnCancel.Size = new Size(100, 30);
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBranchName.Text))
            {
                MessageBox.Show("Şube adı boş olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            BranchName = txtBranchName.Text.Trim();
            this.DialogResult = DialogResult.OK; // Diyalogu OK olarak kapat
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; // Diyalogu Cancel olarak kapat
            this.Close();
        }
    }
}