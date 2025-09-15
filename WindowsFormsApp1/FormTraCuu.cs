using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class FormTraCuu : Form
    {
        private ComboBox cbMien;
        private DateTimePicker dtpNgay;
        private TextBox txtRssUrl;
        private CheckBox chkDungRss;
        private Button btnTraCuu, btnLamMoi;
        private DataGridView dgvKetQua;
        private Button btnTestRss;
        public FormTraCuu()
        {
            this.Text = "Tra Cuu Ket Qua Xo So";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(900, 520);

            cbMien = new ComboBox { Left = 10, Top = 10, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cbMien.Items.AddRange(new[] { "Bac", "Trung", "Nam" });
            cbMien.SelectedIndex = 0;

            dtpNgay = new DateTimePicker { Left = 140, Top = 10, Width = 150, Format = DateTimePickerFormat.Short };

            chkDungRss = new CheckBox { Left = 310, Top = 13, Width = 140, Text = "Tra cuu tu RSS" };
            txtRssUrl = new TextBox { Left = 10, Top = 45, Width = 760, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            btnTraCuu = new Button { Text = "Tra cuu", Left = 780, Top = 42, Width = 100, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnLamMoi = new Button { Text = "Lam moi", Left = 780, Top = 10, Width = 100, Anchor = AnchorStyles.Top | AnchorStyles.Right };

            dgvKetQua = new DataGridView
            {
                Left = 10,
                Top = 80,
                Width = 870,
                Height = 420,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvKetQua.Columns.Add("Giai", "Giai");
            dgvKetQua.Columns.Add("SoTrung", "So trung thuong");

            this.Controls.AddRange(new Control[] { cbMien, dtpNgay, chkDungRss, txtRssUrl, btnTraCuu, btnLamMoi, dgvKetQua });

            // events
            cbMien.SelectedIndexChanged += (s, e) =>
            {
                var mien = cbMien.SelectedItem?.ToString() ?? "Bac";
                txtRssUrl.Text = RssSources.GetUrl(mien);
            };
            btnLamMoi.Click += (s, e) => { dgvKetQua.Rows.Clear(); };
            btnTraCuu.Click += BtnTraCuu_Click;

            // init url
            txtRssUrl.Text = RssSources.GetUrl("Bac");
        }

        private async void BtnTraCuu_Click(object sender, EventArgs e)
        {
            dgvKetQua.Rows.Clear();

            string mien = cbMien.SelectedItem?.ToString() ?? "Bac";
            DateTime ngay = dtpNgay.Value.Date;

            try
            {
                var kq = await DichVuBridge.LayTheoNgayAsync(mien, ngay);

                if (kq == null || kq.Count == 0)
                {
                    MessageBox.Show("Khong co du lieu tu dich vu");
                    return;
                }

                string[] order = { "Dac Biet", "Nhat", "Nhi", "Ba", "Tu", "Nam", "Sau", "Bay", "Tam", "Chin" };

                foreach (var g in order)
                {
                    if (kq.TryGetValue(g, out var list) && list.Count > 0)
                        dgvKetQua.Rows.Add(g, string.Join(" | ", list));
                }

                // hien cac giai khac (neu co)
                foreach (var kv in kq.Where(x => !order.Contains(x.Key)))
                {
                    if (kv.Value?.Count > 0)
                        dgvKetQua.Rows.Add(kv.Key, string.Join(" | ", kv.Value));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi: " + ex.Message);
            }
        }
    }
}