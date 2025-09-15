using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public static class DichVuBridge
    {
        private static string MapGiai(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var t = raw.Trim().ToLowerInvariant();
            t = Regex.Replace(t, @"^\s*g(i|í)ai\s+", ""); // bỏ chữ "giải" nếu có

            if (t.StartsWith("dac")) return "Dac Biet";
            if (t.StartsWith("nhat") || t == "1") return "Nhat";
            if (t.StartsWith("nhi") || t == "2") return "Nhi";
            if (t == "ba" || t == "3") return "Ba";
            if (t == "tu" || t == "4") return "Tu";
            if (t == "nam" || t == "5") return "Nam";
            if (t == "sau" || t == "6") return "Sau";
            if (t == "bay" || t == "7") return "Bay";
            if (t == "tam" || t == "8") return "Tam";
            if (t == "chin") return "Chin";
            return null;
        }

        private static IEnumerable<string> SplitNumbers(string s)
        {
            foreach (Match m in Regex.Matches(s ?? "", @"\b\d{2,6}\b"))
                yield return m.Value;
        }

        private static Dictionary<string, List<string>> FoldByGiai(IEnumerable<KetQua> items)
        {
            var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var it in items)
            {
                var key = MapGiai(it.Giai);
                if (key == null) continue;

                var nums = SplitNumbers(it.SoTrung).ToList();
                if (nums.Count == 0) continue;

                if (!dict.ContainsKey(key)) dict[key] = new List<string>();
                dict[key].AddRange(nums);
            }
            return dict;
        }

        // mienUI: "Bac" | "Trung" | "Nam" (giống combobox form bạn)
        public static async Task<Dictionary<string, List<string>>> LayTheoNgayAsync(
            string mienUI, DateTime ngay, string tinh = null)
        {
            var svc = new DichVuSoXo();
            string mien = (mienUI ?? "Bac").Trim().ToLowerInvariant(); // "bac|trung|nam"

            var list = await svc.LayKetQuaNgayAsync(mien, ngay).ConfigureAwait(false);

            // lọc theo tỉnh nếu có
            if (!string.IsNullOrWhiteSpace(tinh) && (mien == "trung" || mien == "nam"))
                list = list.Where(x => string.Equals(x.Tinh, tinh, StringComparison.OrdinalIgnoreCase)).ToList();

            var dict = FoldByGiai(list);

            // sắp xếp theo thứ tự giải
            var order = new[] { "Dac Biet", "Nhat", "Nhi", "Ba", "Tu", "Nam", "Sau", "Bay", "Tam", "Chin" };
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var g in order)
                if (dict.ContainsKey(g))
                    result[g] = dict[g];
            foreach (var kv in dict)
                if (!result.ContainsKey(kv.Key))
                    result[kv.Key] = kv.Value;

            return result;
        }
    }
}
