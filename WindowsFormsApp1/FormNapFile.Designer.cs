namespace WindowsFormsApp1
{
    partial class FormNapFile
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtThuMuc = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnNapFile = new System.Windows.Forms.Button();
            this.btnLamMoi = new System.Windows.Forms.Button();
            this.dgvKetQuaFile = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKetQuaFile)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(33, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(260, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Chọn Thư Mục Chứa File Kết Quả";
            // 
            // txtThuMuc
            // 
            this.txtThuMuc.Location = new System.Drawing.Point(335, 54);
            this.txtThuMuc.Name = "txtThuMuc";
            this.txtThuMuc.Size = new System.Drawing.Size(100, 22);
            this.txtThuMuc.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(513, 53);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            // 
            // btnNapFile
            // 
            this.btnNapFile.Location = new System.Drawing.Point(161, 146);
            this.btnNapFile.Name = "btnNapFile";
            this.btnNapFile.Size = new System.Drawing.Size(75, 23);
            this.btnNapFile.TabIndex = 3;
            this.btnNapFile.Text = "Nạp File";
            this.btnNapFile.UseVisualStyleBackColor = true;
            // 
            // btnLamMoi
            // 
            this.btnLamMoi.Location = new System.Drawing.Point(299, 146);
            this.btnLamMoi.Name = "btnLamMoi";
            this.btnLamMoi.Size = new System.Drawing.Size(75, 23);
            this.btnLamMoi.TabIndex = 3;
            this.btnLamMoi.Text = "MớiLàm ";
            this.btnLamMoi.UseVisualStyleBackColor = true;
            // 
            // dgvKetQuaFile
            // 
            this.dgvKetQuaFile.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvKetQuaFile.Location = new System.Drawing.Point(453, 146);
            this.dgvKetQuaFile.Name = "dgvKetQuaFile";
            this.dgvKetQuaFile.RowHeadersWidth = 51;
            this.dgvKetQuaFile.RowTemplate.Height = 24;
            this.dgvKetQuaFile.Size = new System.Drawing.Size(335, 301);
            this.dgvKetQuaFile.TabIndex = 4;
            // 
            // FormNapFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgvKetQuaFile);
            this.Controls.Add(this.btnLamMoi);
            this.Controls.Add(this.btnNapFile);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtThuMuc);
            this.Controls.Add(this.label1);
            this.Name = "FormNapFile";
            this.Text = "FormNapFile";
            ((System.ComponentModel.ISupportInitialize)(this.dgvKetQuaFile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtThuMuc;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnNapFile;
        private System.Windows.Forms.Button btnLamMoi;
        private System.Windows.Forms.DataGridView dgvKetQuaFile;
    }
}