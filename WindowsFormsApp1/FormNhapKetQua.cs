using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class FormNhapKetQua : Form
    {
        private XoSoManager manager;

        public FormNhapKetQua(XoSoManager manager)
        {
            InitializeComponent();
            this.manager = manager;
            InitForm();
        }

        private void InitForm()
        {
            cbMien.Items.Clear();
            cbMien.Items.AddRange(new string[] { "Bắc", "Trung", "Nam" });
            cbMien.SelectedIndex = 0;
            dtpNgay.Value = DateTime.Today;
            // Bạn sẽ add DataGridView vào vùng panel phải (panelGrid)
            LoadDanhSach();
        }

        // Map tên giải với textbox, đúng layout trên ảnh
        private Dictionary<string, List<TextBox>> MapTextBox()
        {
            return new Dictionary<string, List<TextBox>>
            {
                { "Đặc Biệt", new List<TextBox> { txtDB1, txtDB2, txtDB3 } },
                { "Nhất",     new List<TextBox> { txtNhat1, txtNhat2, txtNhat3 } },
                { "Nhì",      new List<TextBox> { txtNhi1, txtNhi2, txtNhi3 } },
                { "Ba",       new List<TextBox> { txtBa1, txtBa2, txtBa3 } },
                { "Tư",       new List<TextBox> { txtTu1, txtTu2, txtTu3 } },
                { "Năm",      new List<TextBox> { txtNam1, txtNam2, txtNam3 } },
                { "Sáu",      new List<TextBox> { txtSau1, txtSau2, txtSau3 } },
                { "Bảy",      new List<TextBox> { txtBay1, txtBay2, txtBay3 } },
                { "Tám",      new List<TextBox> { txtTam1, txtTam2, txtTam3 } },
                { "Chín",     new List<TextBox> { txtChin1, txtChin2, txtChin3 } },
            };
        }

        // Lấy dữ liệu từ các textbox nhập form
        private Dictionary<string, List<string>> LayDuLieuNhap()
        {
            var dict = new Dictionary<string, List<string>>();
            var map = MapTextBox();
            foreach (var giai in map.Keys)
            {
                var list = map[giai].Select(tb => tb.Text.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                dict[giai] = list;
            }
            return dict;
        }

        // Đổ dữ liệu từ Dictionary lên form
        private void DoLenForm(Dictionary<string, List<string>> kq)
        {
            var map = MapTextBox();
            foreach (var giai in map.Keys)
            {
                var arr = kq.ContainsKey(giai) ? kq[giai] : new List<string>();
                for (int i = 0; i < 3; ++i)
                {
                    map[giai][i].Text = i < arr.Count ? arr[i] : "";
                }
            }
        }

        // Làm mới form
        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            foreach (var tb in MapTextBox().SelectMany(x => x.Value))
                tb.Text = "";
        }

        // Lưu mới
        private void btnLuu_Click(object sender, EventArgs e)
        {
            string mien = cbMien.SelectedItem.ToString();
            DateTime ngay = dtpNgay.Value.Date;
            var duLieu = LayDuLieuNhap();

            var doiTuong = manager.LayDoiTuongMien(mien);
            if (doiTuong != null)
            {
                bool ok = doiTuong.ThemKetQuaTheoNgay(ngay, duLieu, capNhat: false);
                if (ok)
                {
                    MessageBox.Show("Lưu thành công!");
                    LoadDanhSach();
                }
                else
                {
                    MessageBox.Show("Đã có kết quả ngày này, hãy chọn Sửa nếu muốn cập nhật.");
                }
            }
        }

        // Sửa
        private void btnSua_Click(object sender, EventArgs e)
        {
            string mien = cbMien.SelectedItem.ToString();
            DateTime ngay = dtpNgay.Value.Date;
            var duLieu = LayDuLieuNhap();

            var doiTuong = manager.LayDoiTuongMien(mien);
            if (doiTuong != null)
            {
                bool ok = doiTuong.ThemKetQuaTheoNgay(ngay, duLieu, capNhat: true);
                if (ok)
                {
                    MessageBox.Show("Cập nhật thành công!");
                    LoadDanhSach();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy kết quả để sửa.");
                }
            }
        }

        // Xóa
        private void btnXoa_Click(object sender, EventArgs e)
        {
            string mien = cbMien.SelectedItem.ToString();
            DateTime ngay = dtpNgay.Value.Date;
            var doiTuong = manager.LayDoiTuongMien(mien);
            if (doiTuong != null && doiTuong.LichSuKetQua.ContainsKey(ngay))
            {
                doiTuong.LichSuKetQua.Remove(ngay);
                MessageBox.Show("Đã xóa!");
                LoadDanhSach();
            }
        }

        // Khi chọn dòng kết quả trên lưới
        private void dgvKetQua_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvKetQua.SelectedRows.Count > 0)
            {
                var row = dgvKetQua.SelectedRows[0];
                DateTime ngay = DateTime.Parse(row.Cells["Ngay"].Value.ToString());
                string mien = cbMien.SelectedItem.ToString();

                var doiTuong = manager.LayDoiTuongMien(mien);
                var dict = doiTuong.LayKetQuaTheoNgay(ngay);
                if (dict != null)
                    DoLenForm(dict);
                dtpNgay.Value = ngay;
            }
        }

        // Hiển thị danh sách kết quả ở DataGridView
        private void LoadDanhSach()
        {
            dgvKetQua.Rows.Clear();
            dgvKetQua.Columns.Clear();
            dgvKetQua.Columns.Add("Ngay", "Ngày");
            dgvKetQua.Columns.Add("Giai", "Giải");
            dgvKetQua.Columns.Add("SoTrung", "Số trúng thưởng");

            string mien = cbMien.SelectedItem.ToString();
            var doiTuong = manager.LayDoiTuongMien(mien);
            if (doiTuong != null)
            {
                foreach (var ngay in doiTuong.LichSuKetQua.Keys.OrderByDescending(x => x))
                {
                    var kq = doiTuong.LichSuKetQua[ngay];
                    foreach (var giai in kq.Keys)
                    {
                        dgvKetQua.Rows.Add(ngay.ToShortDateString(), giai, string.Join(" | ", kq[giai]));
                    }
                }
            }
        }

        
    }
}