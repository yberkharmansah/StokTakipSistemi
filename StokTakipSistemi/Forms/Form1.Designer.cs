namespace StokTakipSistemi // Form1.cs'deki namespace ile aynı olmalı
{
    partial class Form1 // 'partial class Form1' olmalı
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) // Bu satır doğru olmalı (önceki parantez hatasını düzelttik)
        {
            if (disposing && (components != null))
            {
                components.Dispose(); // Bu satırda 'object' hatası vardı, düzelmeli
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
            this.components = new System.ComponentModel.Container(); // Bu satırda hata yoktu
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font; // Bu satırda hata vardı, düzelmeli
            this.ClientSize = new System.Drawing.Size(800, 450); // Bu satırda hata vardı, düzelmeli
            this.Text = "Form1"; // Bu satırda hata vardı, düzelmeli
        }

        #endregion
    }
}