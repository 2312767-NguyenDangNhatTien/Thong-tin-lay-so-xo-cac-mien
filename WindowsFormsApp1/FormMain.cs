using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class FormMain : Form   // <-- partial (khớp Designer)
    {
        private readonly XoSoManager manager = new XoSoManager();

        public FormMain()
        {
            InitializeComponent(); // do Designer sinh
        }

        // Mở form Tra cứu KẾT QUẢ (danh sách giải)
        private void btnTraCuu_Click(object sender, EventArgs e)
        {
            new FormTraCuu().Show();
        }

        // Mở form Tra cứu VÉ TRÚNG THƯỞNG
        private void btnTraCuuVeTrungThuong_Click(object sender, EventArgs e)
        {
            new FormTraCuuVeTrungThuong().Show();
        }

        // Mở form Nhập Kết Quả (offline)
        private void btnNhapKetQua_Click(object sender, EventArgs e)
        {
            using (var f = new FormNhapKetQua(manager))
                f.ShowDialog(this);
        }

        // Mở form Nạp File (offline)
        private void btnNapFile_Click(object sender, EventArgs e)
        {
            using (var f = new FormNapFile(manager))
                f.ShowDialog(this);
        }
    }
}