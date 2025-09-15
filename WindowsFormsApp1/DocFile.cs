using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace WindowsFormsApp1
{
    public static class DocFileHelper
    {
        public static void NapDuLieuTuThuMuc(string thuMuc, XoSoManager manager)
        {
            if (!Directory.Exists(thuMuc))
            {
                Console.WriteLine("Thu muc khong ton tai.");
                return;
            }

            string[] files = Directory.GetFiles(thuMuc, "*.txt");
            foreach (string file in files)
            {
                try
                {
                    string tenFile = Path.GetFileNameWithoutExtension(file); // VD: Bac_2025-09-01
                    var parts = tenFile.Split('_');
                    if (parts.Length != 2) continue;

                    string mien = parts[0];
                    if (!DateTime.TryParseExact(parts[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ngay))
                        continue;

                    var danhSachGiai = new Dictionary<string, List<string>>();
                    foreach (var line in File.ReadAllLines(file))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var giaiParts = line.Split(':');
                        if (giaiParts.Length == 2)
                        {
                            string tenGiai = giaiParts[0].Trim();
                            var soTrungs = giaiParts[1]
                                .Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .ToList();
                            danhSachGiai[tenGiai] = soTrungs;
                        }
                    }

                    var doiTuong = manager.LayDoiTuongMien(mien);
                    if (doiTuong != null)
                    {
                        bool ok = doiTuong.ThemKetQuaTheoNgay(ngay, danhSachGiai, capNhat: false);
                        if (!ok)
                            Console.WriteLine($"Da co ket qua {mien} - {ngay:yyyy-MM-dd}, bo qua file: {tenFile}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Loi doc file: {file}, Chi tiet: {ex.Message}");
                }
            }
        }
    }
}