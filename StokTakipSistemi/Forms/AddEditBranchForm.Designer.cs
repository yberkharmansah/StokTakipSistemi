namespace StokTakipSistemi // Form1.cs ve AddEditBranchForm.cs ile aynı namespace olmalı
{
    partial class AddEditBranchForm // `partial` anahtar kelimesi çok önemli
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container(); // Bu satırı ekleyin veya kontrol edin
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 180); // Formun boyutlarını ayarlayın
            this.Text = "AddEditBranchForm"; // Formun başlığını ayarlayın
        }

        #endregion
    }
}