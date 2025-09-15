using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class FormTraCuuVeTrungThuong : Form
    {
        private readonly XoSoManager manager = new XoSoManager();
        private readonly CrawlerXoSoRss rss = new CrawlerXoSoRss();

        private ComboBox cbMien;
        private DateTimePicker dtpNgay;
        private TextBox txtVe;
        private Button btnTraCuu, btnLamMoi;
        private CheckBox chkDungRss;
        private TextBox txtRssUrl;
        private DataGridView dgv;
        private Button btnTestRss;
        public FormTraCuuVeTrungThuong()
        {
            this.Text = "Tra Cuu Ve Trung Thuong";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(920, 560);

            cbMien = new ComboBox { Left = 20, Top = 12, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cbMien.Items.AddRange(new[] { "Bac", "Trung", "Nam" });
            cbMien.SelectedIndex = 0;

            dtpNgay = new DateTimePicker { Left = 150, Top = 12, Width = 150, Format = DateTimePickerFormat.Short };

            txtVe = new TextBox { Left = 310, Top = 12, Width = 220, ForeColor = Color.Gray, Text = "Nhap so ve can tra cuu" };
            txtVe.GotFocus += (s, e) =>
            {
                if (txtVe.ForeColor == Color.Gray)
                {
                    txtVe.Text = "";
                    txtVe.ForeColor = Color.Black;
                }
            };
            txtVe.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtVe.Text))
                {
                    txtVe.Text = "Nhap so ve can tra cuu";
                    txtVe.ForeColor = Color.Gray;
                }
            };

            btnTraCuu = new Button { Left = 540, Top = 10, Width = 110, Height = 28, Text = "Tra cuu" };
            btnLamMoi = new Button { Left = 660, Top = 10, Width = 110, Height = 28, Text = "Lam moi" };

            chkDungRss = new CheckBox { Left = 20, Top = 50, Width = 140, Text = "Tra cuu tu RSS" };
            txtRssUrl = new TextBox { Left = 160, Top = 48, Width = 740, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            dgv = new DataGridView
            {
                Left = 20,
                Top = 90,
                Width = 880,
                Height = 440,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgv.Columns.Add("Giai", "Giai");
            dgv.Columns.Add("SoTrung", "So trung");
            dgv.Columns.Add("KetQua", "Ket qua");

            this.Controls.AddRange(new Control[] { cbMien, dtpNgay, txtVe, btnTraCuu, btnLamMoi, chkDungRss, txtRssUrl, dgv });

            // events
            cbMien.SelectedIndexChanged += (s, e) =>
            {
                var mien = cbMien.SelectedItem?.ToString() ?? "Bac";
                txtRssUrl.Text = RssSources.GetUrl(mien);
            };
            btnLamMoi.Click += (s, e) =>
            {
                txtVe.Text = "Nhap so ve can tra cuu";
                txtVe.ForeColor = Color.Gray;
                dgv.Rows.Clear();
            };
            btnTraCuu.Click += BtnTraCuu_Click;

            // init url
            txtRssUrl.Text = RssSources.GetUrl("Bac");
        }

        private async void BtnTraCuu_Click(object sender, EventArgs e)
        {
            string mien = cbMien.SelectedItem?.ToString() ?? "Bac";
            DateTime ngay = dtpNgay.Value.Date;
            string ve = (txtVe.Text ?? "").Trim();

            if (string.IsNullOrEmpty(ve) || txtVe.ForeColor == System.Drawing.Color.Gray)
            {
                MessageBox.Show("Vui long nhap so ve can tra cuu!");
                return;
            }

            dgv.Rows.Clear();

            Dictionary<string, List<string>> kq;
            try
            {
                // Neu co ComboBox tinh thi truyen them tham so thu 3
                // var tinh = cbTinh?.SelectedItem?.ToString();
                // kq = await DichVuBridge.LayTheoNgayAsync(mien, ngay, tinh);
                kq = await DichVuBridge.LayTheoNgayAsync(mien, ngay);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi dich vu: " + ex.Message);
                return;
            }

            if (kq == null || kq.Count == 0)
            {
                MessageBox.Show("Khong co du lieu cho ngay nay!");
                return;
            }

            string[] order = { "Dac Biet", "Nhat", "Nhi", "Ba", "Tu", "Nam", "Sau", "Bay", "Tam", "Chin" };

            foreach (var g in order)
            {
                if (kq.TryGetValue(g, out var list))
                {
                    string soTrung = string.Join(" | ", list);
                    string ketQua = list.Any(s => s == ve) ? "Trung" : "Khong trung";
                    dgv.Rows.Add(g, soTrung, ketQua);
                }
            }

            // cac giai con lai (neu co)
            foreach (var kv in kq.Where(kv => !order.Contains(kv.Key)))
            {
                string soTrung = string.Join(" | ", kv.Value);
                string ketQua = kv.Value.Any(s => s == ve) ? "Trung" : "Khong trung";
                dgv.Rows.Add(kv.Key, soTrung, ketQua);
            }
        }
        private static string GetMinhNgocRss(string mienUI)
        {
            var m = (mienUI ?? "Bac").Trim().ToLowerInvariant();
            if (m == "trung") return "https://www.minhngoc.net.vn/rss/xo-so-mien-trung.rss";
            if (m == "nam") return "https://www.minhngoc.net.vn/rss/xo-so-mien-nam.rss";
            return "https://www.minhngoc.net.vn/rss/xo-so-mien-bac.rss";
        }

        private async void BtnTestRss_Click(object sender, EventArgs e)
        {
            string mien = cbMien.SelectedItem?.ToString() ?? "Bac";

            // 1) XSKT theo miền đang chọn
            string xsktUrl = RssSources.GetUrl(mien);
            await RssDiag.RunAsync(xsktUrl);

            // 2) MinhNgoc tương ứng
            string mnUrl = GetMinhNgocRss(mien);
            await RssDiag.RunAsync(mnUrl);
        }
    }
}
