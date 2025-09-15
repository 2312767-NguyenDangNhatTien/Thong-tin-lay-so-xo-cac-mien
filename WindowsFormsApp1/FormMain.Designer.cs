namespace WindowsFormsApp1
{
    partial class FormMain
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
            this.btnKiemTra = new System.Windows.Forms.Button();
            this.btnTraCuu = new System.Windows.Forms.Button();
            this.btnNhapKetQua = new System.Windows.Forms.Button();
            this.btnNapFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnKiemTra
            // 
            this.btnKiemTra.Location = new System.Drawing.Point(643, 339);
            this.btnKiemTra.Name = "btnKiemTra";
            this.btnKiemTra.Size = new System.Drawing.Size(132, 23);
            this.btnKiemTra.TabIndex = 2;
            this.btnKiemTra.Text = "Kiểm Tra Thưởng ";
            this.btnKiemTra.UseVisualStyleBackColor = true;
            this.btnKiemTra.Click += new System.EventHandler(this.btnTraCuuVeTrungThuong_Click);
            // 
            // btnTraCuu
            // 
            this.btnTraCuu.Location = new System.Drawing.Point(509, 339);
            this.btnTraCuu.Name = "btnTraCuu";
            this.btnTraCuu.Size = new System.Drawing.Size(75, 23);
            this.btnTraCuu.TabIndex = 1;
            this.btnTraCuu.Text = "Tra Cứu";
            this.btnTraCuu.UseVisualStyleBackColor = true;
            this.btnTraCuu.Click += new System.EventHandler(this.btnTraCuu_Click);
            // 
            // btnNhapKetQua
            // 
            this.btnNhapKetQua.Location = new System.Drawing.Point(13, 339);
            this.btnNhapKetQua.Name = "btnNhapKetQua";
            this.btnNhapKetQua.Size = new System.Drawing.Size(121, 23);
            this.btnNhapKetQua.TabIndex = 0;
            this.btnNhapKetQua.Text = "Nhập Kết Quả";
            this.btnNhapKetQua.UseVisualStyleBackColor = true;
            this.btnNhapKetQua.Click += new System.EventHandler(this.btnNhapKetQua_Click);
            // 
            // btnNapFile
            // 
            this.btnNapFile.Location = new System.Drawing.Point(175, 339);
            this.btnNapFile.Name = "btnNapFile";
            this.btnNapFile.Size = new System.Drawing.Size(75, 23);
            this.btnNapFile.TabIndex = 3;
            this.btnNapFile.Text = "Nạp File";
            this.btnNapFile.UseVisualStyleBackColor = true;
            this.btnNapFile.Click += new System.EventHandler(this.btnNapFile_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnKiemTra);
            this.Controls.Add(this.btnTraCuu);
            this.Controls.Add(this.btnNapFile);
            this.Controls.Add(this.btnNhapKetQua);
            this.Name = "FormMain";
            this.Text = "Bảng Chức Năng ";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnKiemTra;
        private System.Windows.Forms.Button btnTraCuu;
        private System.Windows.Forms.Button btnNhapKetQua;
        private System.Windows.Forms.Button btnNapFile;
    }
}