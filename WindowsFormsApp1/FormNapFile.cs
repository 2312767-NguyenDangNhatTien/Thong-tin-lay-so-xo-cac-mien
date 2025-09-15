using System;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class FormNapFile : Form
    {
        private XoSoManager manager;

        public FormNapFile(XoSoManager manager)
        {
            InitializeComponent();
            this.manager = manager;
            txtThuMuc.ReadOnly = true;
            dgvKetQuaFile.Columns.Clear();
            dgvKetQuaFile.Columns.Add("FileName", "Tên File");
            dgvKetQuaFile.Columns.Add("Mien", "Miền");
            dgvKetQuaFile.Columns.Add("Ngay", "Ngày");
            dgvKetQuaFile.Columns.Add("TrangThai", "Trạng thái");
            dgvKetQuaFile.Columns.Add("GhiChu", "Ghi chú");
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtThuMuc.Text = dlg.SelectedPath;
                }
            }
        }

        private void btnNapFile_Click(object sender, EventArgs e)
        {
            dgvKetQuaFile.Rows.Clear();
            string thuMuc = txtThuMuc.Text.Trim();
            if (!Directory.Exists(thuMuc))
            {
                MessageBox.Show("Thư mục không tồn tại!");
                return;
            }
            string[] files = Directory.GetFiles(thuMuc, "*.txt");
            foreach (var file in files)
            {
                string tenFile = Path.GetFileNameWithoutExtension(file);
                string mien = "", trangThai = "Thành công", ghiChu = "", ngayStr = "";
                try
                {
                    var parts = tenFile.Split('_');
                    if (parts.Length == 2)
                    {
                        mien = parts[0];
                        ngayStr = parts[1];
                        DocFileHelper.NapDuLieuTuThuMuc(thuMuc, manager); // Nạp cho tất cả file trong thư mục (bạn có thể sửa để chỉ nạp 1 file nếu muốn)
                    }
                    else
                    {
                        trangThai = "Lỗi";
                        ghiChu = "Tên file không hợp lệ";
                    }
                }
                catch (Exception ex)
                {
                    trangThai = "Lỗi";
                    ghiChu = ex.Message;
                }
                dgvKetQuaFile.Rows.Add(tenFile, mien, ngayStr, trangThai, ghiChu);
            }
            MessageBox.Show("Đã nạp xong file!");
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtThuMuc.Clear();
            dgvKetQuaFile.Rows.Clear();
        }
    }
}