using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace WindowsFormsApp1
{
    internal class DichVuSoXo
    {
        private const bool ENABLE_DEBUG_DUMP = false;

        private readonly Dictionary<string, List<string>> TinhTheoMien = new Dictionary<string, List<string>>
        {
            { "bac", new List<string> { "Miền Bắc" } },
            { "trung", new List<string> {
                "Đà Nẵng","Quảng Nam","Thừa Thiên Huế","Quảng Bình","Quảng Trị","Quảng Ngãi",
                "Khánh Hòa","Phú Yên","Bình Định","Gia Lai","Đắk Lắk","Đắk Nông","Kon Tum","Ninh Thuận"
            }},
            { "nam", new List<string> {
                "TP.HCM","TP HCM","TPHCM","Hồ Chí Minh","Cần Thơ","Đồng Nai","Bình Dương",
                "Bà Rịa - Vũng Tàu","Vũng Tàu","Tiền Giang","Long An","Bến Tre","Vĩnh Long","An Giang",
                "Đồng Tháp","Tây Ninh","Sóc Trăng","Bạc Liêu","Cà Mau","Hậu Giang","Kiên Giang","Trà Vinh","Bình Phước"
            }}
        };

        private static readonly Dictionary<string, string> AliasTinh = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "TP HCM", "TP.HCM" }, { "TPHCM", "TP.HCM" }, { "Ho Chi Minh", "TP.HCM" }, { "Hồ Chí Minh", "TP.HCM" },
            { "Vung Tau", "Vũng Tàu" }, { "Ba Ria - Vung Tau", "Vũng Tàu" }, { "Ba Ria Vung Tau", "Vũng Tàu" }
        };

        // ======== Lấy theo RSS (các ngày gần đây) ========
        public async Task<List<KetQua>> LayKetQuaAsync(string mien, DateTime _ignored)
        {
            string rssUrl = mien == "bac"
                ? "https://xskt.com.vn/rss-feed/mien-bac-xsmb.rss"
                : (mien == "trung" ? "https://xskt.com.vn/rss-feed/mien-trung-xsmt.rss"
                                    : "https://xskt.com.vn/rss-feed/mien-nam-xsmn.rss");

            var list = new List<KetQua>();
            var doc = new XmlDocument();
            doc.LoadXml(await GetStringAsync(rssUrl));

            var items = doc.SelectNodes("//item");
            if (items == null) return list;

            foreach (XmlNode item in items)
            {
                DateTime ngayItem = LayNgay(item);
                if (ngayItem == DateTime.MinValue) continue;

                string descHtml = item.SelectSingleNode("description")?.InnerText ?? "";
                if (string.IsNullOrWhiteSpace(descHtml)) continue;

                string normalized = Regex.Replace(descHtml, @"(?i)<br\s*/?>", "|BR|");
                normalized = Regex.Replace(normalized, "<.*?>", "");
                normalized = WebUtility.HtmlDecode(normalized).Trim();

                if (mien == "bac")
                {
                    foreach (var p in TachGiaiSo(normalized).DefaultIfEmpty(Tuple.Create("Tổng hợp", GopTatCaSo(normalized))))
                        list.Add(new KetQua { Dai = TenMien(mien), Giai = p.Item1, SoTrung = p.Item2, Ngay = ngayItem, Tinh = "Miền Bắc" });
                }
                else
                {
                    var segs = CatTheoTinh_Bracket(normalized);
                    if (segs.Count == 0) segs = CatTheoTinh_Fallback(normalized);
                    if (segs.Count == 0) segs.Add(Tuple.Create("Tất cả", normalized));

                    foreach (var seg in segs)
                    {
                        string tinh = ChuanHoaTenTinh(seg.Item1);
                        if (tinh != "Tất cả" && !NamTrongMien(mien, tinh)) continue;

                        var pairs = TachGiaiSo(seg.Item2);
                        if (pairs.Count == 0)
                            list.Add(new KetQua { Dai = TenMien(mien), Giai = "Tổng hợp", SoTrung = GopTatCaSo(seg.Item2), Ngay = ngayItem, Tinh = tinh });
                        else
                            foreach (var p in pairs)
                                list.Add(new KetQua { Dai = TenMien(mien), Giai = p.Item1, SoTrung = p.Item2, Ngay = ngayItem, Tinh = tinh });
                    }
                }
            }
            return list;
        }

        public async Task<List<KetQua>> LayKetQuaNgayAsync(string mien, DateTime ngay)
        {
            string url = BuildDateUrl(mien, ngay);
            string html = await GetStringAsync(url);

            string desc = ExtractMetaDescription(html);
            string normalized;
            if (!string.IsNullOrEmpty(desc) && Regex.IsMatch(desc, @"(ĐB|Đặc\s*biệt|Giải|[1-8])\s*:?", RegexOptions.IgnoreCase))
                normalized = Normalize(desc);
            else
            {
                string raw = StripTags(html);
                int idx = raw.IndexOf("ĐB", StringComparison.OrdinalIgnoreCase);
                if (idx < 0) idx = raw.IndexOf("Đặc biệt", StringComparison.OrdinalIgnoreCase);
                if (idx > 0) raw = raw.Substring(Math.Max(0, idx - 50));
                normalized = Normalize(raw);
            }

            var list = new List<KetQua>();

            if (mien == "bac")
            {
                foreach (var p in TachGiaiSo(normalized).DefaultIfEmpty(Tuple.Create("Tổng hợp", GopTatCaSo(normalized))))
                    list.Add(new KetQua { Dai = TenMien(mien), Giai = p.Item1, SoTrung = p.Item2, Ngay = ngay.Date, Tinh = "Miền Bắc" });
                return list;
            }

            var segs2 = CatTheoTinh_Bracket(normalized);
            if (segs2.Count == 0) segs2 = CatTheoTinh_Fallback(normalized);
            if (segs2.Count == 0) segs2.Add(Tuple.Create("Tất cả", normalized));

            foreach (var seg in segs2)
            {
                string tinh = ChuanHoaTenTinh(seg.Item1);
                if (tinh != "Tất cả" && !NamTrongMien(mien, tinh)) continue;

                var pairs = TachGiaiSo(seg.Item2);
                if (pairs.Count == 0)
                    list.Add(new KetQua { Dai = TenMien(mien), Giai = "Tổng hợp", SoTrung = GopTatCaSo(seg.Item2), Ngay = ngay.Date, Tinh = tinh });
                else
                    foreach (var p in pairs)
                        list.Add(new KetQua { Dai = TenMien(mien), Giai = p.Item1, SoTrung = p.Item2, Ngay = ngay.Date, Tinh = tinh });
            }
            return list;
        }

        private async Task<string> GetStringAsync(string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            using (var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) })
            {
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                http.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                string s = await http.GetStringAsync(url);
                if (ENABLE_DEBUG_DUMP) SafeDebugDump("last_fetch.txt", s);
                return s;
            }
        }

        private string BuildDateUrl(string mien, DateTime d)
        {
            string path = mien == "bac" ? "xsmb" : (mien == "trung" ? "xsmt" : "xsmn");
            return $"https://xskt.com.vn/{path}/ngay-{d.Day}-{d.Month}-{d.Year}";
        }

        private string ExtractMetaDescription(string html)
        {
            var m1 = Regex.Match(html, @"<meta[^>]+property\s*=\s*[""']og:description[""'][^>]+content\s*=\s*[""'](.*?)[""']",
                                 RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (m1.Success) return WebUtility.HtmlDecode(m1.Groups[1].Value);

            var m2 = Regex.Match(html, @"<meta[^>]+name\s*=\s*[""']description[""'][^>]+content\s*=\s*[""'](.*?)[""']",
                                 RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (m2.Success) return WebUtility.HtmlDecode(m2.Groups[1].Value);

            return "";
        }

        private string Normalize(string s)
        {
            string t = Regex.Replace(s ?? "", @"(?i)<br\s*/?>", "|BR|");
            t = Regex.Replace(t, "<.*?>", "");
            t = WebUtility.HtmlDecode(t);
            t = Regex.Replace(t, @"\s+", " ").Trim();
            return t;
        }

        private string StripTags(string html)
        {
            string t = Regex.Replace(html ?? "", @"(?i)<br\s*/?>", "|BR|");
            t = Regex.Replace(t, "<.*?>", " ");
            t = WebUtility.HtmlDecode(t);
            t = Regex.Replace(t, @"\s+", " ").Trim();
            return t;
        }

        private void SafeDebugDump(string filename, string content)
        {
            try { File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename), content, Encoding.UTF8); }
            catch { }
        }

        private bool NamTrongMien(string mien, string tinh)
        {
            if (!TinhTheoMien.ContainsKey(mien)) return false;
            string Key(string s) => Regex.Replace(s ?? "", @"[\.\s\-]", "").ToLowerInvariant();

            var set = new HashSet<string>(TinhTheoMien[mien].Select(Key));
            return set.Contains(Key(tinh)) || set.Contains(Key(ChuanHoaTenTinh(tinh)));
        }

        private string ChuanHoaTenTinh(string raw)
        {
            string t = (raw ?? "").Trim();
            return AliasTinh.TryGetValue(t, out var mapped) ? mapped : t;
        }

        // --- Tách block theo [Tỉnh] ---
        private List<Tuple<string, string>> CatTheoTinh_Bracket(string normalized)
        {
            var result = new List<Tuple<string, string>>();
            var rx = new Regex(@"\[(?<tinh>[^\]]+)\]\s*(?<body>.*?)(?=(\[[^\]]+\])|$)", RegexOptions.Singleline);
            foreach (Match m in rx.Matches(normalized))
            {
                string tinh = m.Groups["tinh"].Value.Trim();
                string body = m.Groups["body"].Value.Trim();
                if (!string.IsNullOrEmpty(tinh) && !string.IsNullOrEmpty(body))
                    result.Add(Tuple.Create(tinh, body));
            }
            return result;
        }

        // --- Fallback theo "<Tỉnh>: ..." ---
        private List<Tuple<string, string>> CatTheoTinh_Fallback(string normalized)
        {
            var blocks = new List<Tuple<string, string>>();
            var lines = normalized.Split(new[] { "|BR|" }, StringSplitOptions.RemoveEmptyEntries);

            string currentTinh = null;
            var sb = new StringBuilder();

            foreach (var raw in lines)
            {
                var line = raw.Trim();
                var m = Regex.Match(line, @"^([A-Za-zÀ-Ỵà-ỵ\.\s\-]+):\s*(.*)$");
                if (m.Success)
                {
                    if (!string.IsNullOrEmpty(currentTinh))
                    {
                        blocks.Add(Tuple.Create(currentTinh, sb.ToString().Trim()));
                        sb.Clear();
                    }
                    currentTinh = m.Groups[1].Value.Trim();
                    sb.Append(m.Groups[2].Value.Trim());
                }
                else if (!string.IsNullOrEmpty(currentTinh))
                {
                    if (sb.Length > 0) sb.Append(" ");
                    sb.Append(line);
                }
            }
            if (!string.IsNullOrEmpty(currentTinh))
                blocks.Add(Tuple.Create(currentTinh, sb.ToString().Trim()));

            return blocks;
        }

        // --- cắt rác: bắt đầu từ "ĐB"/"Đặc biệt" ---
        private string TrimAtDb(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var m = Regex.Match(s, @"(ĐB|Đặc\s*biệt)\s*:?", RegexOptions.IgnoreCase);
            return m.Success ? s.Substring(m.Index) : s;
        }

        // --- Lấy các số hợp lệ (2–6 chữ số), ghép " - " ---
        private string ExtractNumbers(string chunk)
        {
            var nums = Regex.Matches(chunk ?? "", @"\b\d{2,6}\b")
                            .Cast<Match>()
                            .Select(m => m.Value)
                            .ToList();
            return nums.Count > 0 ? string.Join(" - ", nums) : chunk;
        }

        private List<Tuple<string, string>> TachGiaiSo(string text)
        {
            var res = new List<Tuple<string, string>>();
            if (string.IsNullOrWhiteSpace(text)) return res;

            var norm = Regex.Replace(text, @"\s+", " ").Trim();
            norm = TrimAtDb(norm);

            var labelRx = new Regex(
                @"(ĐB|Đặc\s*biệt|Giải\s*nhất|Giải\s*nhì|Giải\s*ba|Giải\s*tư|Giải\s*năm|Giải\s*sáu|Giải\s*bảy|Giải\s*tám)\s*:|([1-8])\s*:",
                RegexOptions.IgnoreCase);

            var labels = labelRx.Matches(norm);

            for (int i = 0; i < labels.Count; i++)
            {
                string rawLabel = labels[i].Groups[1].Success ? labels[i].Groups[1].Value : labels[i].Groups[2].Value;
                int start = labels[i].Index + labels[i].Length;
                int end = (i + 1 < labels.Count) ? labels[i + 1].Index : norm.Length;

                string chunk = norm.Substring(start, end - start).Trim();
                chunk = Regex.Replace(chunk, @"^[\-\s,;:]+|[\-\s,;:]+$", "");

                string numbers = ExtractNumbers(chunk);
                if (Regex.IsMatch(numbers, @"\d"))
                    res.Add(Tuple.Create(ChuanHoaGiai(rawLabel), numbers));
            }

            bool has8 = res.Any(t => t.Item1.IndexOf("tám", StringComparison.OrdinalIgnoreCase) >= 0);
            if (!has8)
            {
                var m = Regex.Match(norm, @"7\s*:\s*[^[]*?:\s*(\d{2})(\s|\[|$)");
                if (m.Success) res.Add(Tuple.Create("Giải tám", m.Groups[1].Value));
            }

            return res;
        }

        private string GopTatCaSo(string s)
        {
            var nums = Regex.Matches(s ?? "", @"\b\d{2,}\b");
            if (nums.Count == 0) return "(không thấy số)";
            var sb = new StringBuilder();
            foreach (Match m in nums) { if (sb.Length > 0) sb.Append(" "); sb.Append(m.Value); }
            return sb.ToString();
        }

        // --- Parse ngày từ item RSS ---
        private DateTime LayNgay(XmlNode item)
        {
            string title = item.SelectSingleNode("title")?.InnerText ?? "";

            var mFull = Regex.Match(title, @"ngày\s+(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{2,4})", RegexOptions.IgnoreCase);
            if (mFull.Success) { int d = int.Parse(mFull.Groups[1].Value); int m = int.Parse(mFull.Groups[2].Value); int y = int.Parse(mFull.Groups[3].Value); if (y < 100) y += 2000; return new DateTime(y, m, d); }

            var mShort = Regex.Match(title, @"ngày\s+(\d{1,2})[\/\-](\d{1,2})", RegexOptions.IgnoreCase);
            if (mShort.Success) { int d = int.Parse(mShort.Groups[1].Value); int m = int.Parse(mShort.Groups[2].Value); return new DateTime(DateTime.Now.Year, m, d); }

            var mFull2 = Regex.Match(title, @"ngày\s+(\d{1,2})\s*[\/\-]\s*(\d{1,2})\s*[\/\-]\s*(\d{2,4})", RegexOptions.IgnoreCase);
            if (mFull2.Success) { int d = int.Parse(mFull2.Groups[1].Value); int m = int.Parse(mFull2.Groups[2].Value); int y = int.Parse(mFull2.Groups[3].Value); if (y < 100) y += 2000; return new DateTime(y, m, d); }

            string pub = item.SelectSingleNode("pubDate")?.InnerText ?? "";
            if (DateTime.TryParse(pub, out var dPub)) return dPub.Date;

            string desc = item.SelectSingleNode("description")?.InnerText ?? "";
            desc = Regex.Replace(desc, "<.*?>", "");
            var md = Regex.Match(desc, @"(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{2,4})");
            if (md.Success) { int d = int.Parse(md.Groups[1].Value); int m = int.Parse(md.Groups[2].Value); int y = int.Parse(md.Groups[3].Value); if (y < 100) y += 2000; return new DateTime(y, m, d); }

            return DateTime.MinValue;
        }

        private string ChuanHoaGiai(string text)
        {
            string t = (text ?? "").Trim().ToUpperInvariant();
            if (t == "ĐB" || t.Contains("ĐẶC")) return "Đặc biệt";
            if (t == "1" || Regex.IsMatch(t, @"GIẢI\s*NHẤT")) return "Giải nhất";
            if (t == "2" || Regex.IsMatch(t, @"GIẢI\s*NHÌ")) return "Giải nhì";
            if (t == "3" || Regex.IsMatch(t, @"GIẢI\s*BA")) return "Giải ba";
            if (t == "4" || Regex.IsMatch(t, @"GIẢI\s*TƯ")) return "Giải tư";
            if (t == "5" || Regex.IsMatch(t, @"GIẢI\s*NĂM")) return "Giải năm";
            if (t == "6" || Regex.IsMatch(t, @"GIẢI\s*SÁU")) return "Giải sáu";
            if (t == "7" || Regex.IsMatch(t, @"GIẢI\s*BẢY")) return "Giải bảy";
            if (t == "8" || Regex.IsMatch(t, @"GIẢI\s*TÁM")) return "Giải tám";
            return "Giải " + text;
        }

        private string TenMien(string mien) => mien == "bac" ? "Miền Bắc" : (mien == "trung" ? "Miền Trung" : "Miền Nam");

        public List<string> LayDanhSachTinh(string mien)
        {
            if (!TinhTheoMien.ContainsKey(mien)) return new List<string>();
            var ds = new List<string>();
            foreach (var s in TinhTheoMien[mien])
            {
                var ch = ChuanHoaTenTinh(s);
                if (!ds.Exists(x => string.Equals(x, ch, StringComparison.OrdinalIgnoreCase))) ds.Add(ch);
            }
            ds.Sort(StringComparer.OrdinalIgnoreCase);
            if (mien != "bac") ds.Insert(0, "Tất cả");
            return ds;
        }
    }
}
