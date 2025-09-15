using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace WindowsFormsApp1
{
    public class CrawlerXoSoRss
    {
        private static readonly HttpClient http = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            var h = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true
            };

            var c = new HttpClient(h);
            c.Timeout = TimeSpan.FromSeconds(30);
            c.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120 Safari/537.36");
            c.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml, application/xml, text/xml, */*");
            c.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
            c.DefaultRequestHeaders.AcceptLanguage.ParseAdd("vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7");
            return c;
        }

        private static string Truncate(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Length <= maxLen ? s : s.Substring(0, maxLen);
        }

        private static bool LooksLikeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            var t = text.TrimStart();
            if (t.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.StartsWith("<html", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.IndexOf("cloudflare", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static async Task<string> DownloadStringAsync(string url)
        {
            using (var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                resp.EnsureSuccessStatusCode();
                var bytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                var charset = resp.Content.Headers.ContentType != null
                    ? resp.Content.Headers.ContentType.CharSet
                    : null;

                string text = null;
                if (!string.IsNullOrEmpty(charset))
                {
                    try { text = Encoding.GetEncoding(charset.Trim('"').Trim()).GetString(bytes); } catch { }
                }
                if (text == null) { try { text = Encoding.UTF8.GetString(bytes); } catch { } }
                if (text == null) { try { text = Encoding.Unicode.GetString(bytes); } catch { } }
                if (text == null) { text = Encoding.GetEncoding("iso-8859-1").GetString(bytes); }
                return text;
            }
        }

        private static string SanitizeXml(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return xml;
            xml = xml.Trim('\uFEFF', '\u200B', '\u0000');
            // & rời rạc -> &amp; (giữ các entity hợp lệ)
            xml = Regex.Replace(xml,
                @"&(?!amp;|lt;|gt;|apos;|quot;|#\d+;|#x[0-9A-Fa-f]+;)",
                "&amp;");
            // bỏ control char
            xml = Regex.Replace(xml, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");
            return xml;
        }

        private static string InnerCDataOrText(XmlNode node, string childName)
        {
            var n = node[childName];
            if (n == null) return "";
            if (n.FirstChild is XmlCDataSection cd && !string.IsNullOrEmpty(cd.Value))
                return cd.Value;
            return n.InnerText ?? "";
        }

        // regex fallback khi XML bẩn: lấy <item>...</item>
        private static IEnumerable<string> ExtractItemXmlByRegex(string xml)
        {
            foreach (Match m in Regex.Matches(xml, @"<item\b[\s\S]*?</item>", RegexOptions.IgnoreCase))
                yield return m.Value;
        }

        public async Task<List<RssItem>> LayKetQuaTuRssAsync(string rssUrl)
        {
            if (string.IsNullOrWhiteSpace(rssUrl))
                throw new Exception("RSS URL rong");

            var list = new List<RssItem>();
            string raw = null;

            try
            {
                raw = await DownloadStringAsync(rssUrl).ConfigureAwait(false);
                // log thô để debug (Desktop\rss_log.txt)
                try
                {
                    var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "rss_log.txt");
                    File.WriteAllText(logPath, Truncate(raw ?? "", 4000));
                }
                catch { }

                if (string.IsNullOrWhiteSpace(raw))
                    throw new Exception("RSS tra ve rong");

                if (LooksLikeHtml(raw))
                    throw new Exception("Server tra ve HTML (co the bi Cloudflare/anti-bot).");

                string xml = SanitizeXml(raw);

                // 1) Parse bằng XmlDocument bình thường
                try
                {
                    var doc = new XmlDocument { XmlResolver = null };
                    var settings = new XmlReaderSettings
                    {
                        DtdProcessing = DtdProcessing.Ignore,
                        IgnoreComments = true,
                        IgnoreWhitespace = true
                    };
                    using (var xr = XmlReader.Create(new StringReader(xml), settings))
                    {
                        doc.Load(xr);
                    }

                    var items = doc.GetElementsByTagName("item");
                    foreach (XmlNode item in items)
                    {
                        string title = InnerCDataOrText(item, "title");
                        string pub = InnerCDataOrText(item, "pubDate");
                        string desc = InnerCDataOrText(item, "description");

                        DateTime ngay = DateTime.Today;
                        DateTime tmp;
                        if (DateTime.TryParse(pub, out tmp)) ngay = tmp;

                        var dict = RssAdapter.ParseKetQuaFromPlainText(desc);
                        list.Add(new RssItem
                        {
                            TieuDe = title,
                            Ngay = ngay,
                            MoTa = desc,
                            KetQuaTheoGiai = dict
                        });
                    }
                }
                catch
                {
                    // 2) Fallback regex: bóc từng <item> và đọc thô các trường con
                    foreach (var itemXml in ExtractItemXmlByRegex(xml))
                    {
                        string title = "";
                        string pub = "";
                        string desc = "";

                        var mTitle = Regex.Match(itemXml, @"<title\b[^>]*>(?<t>[\s\S]*?)</title>", RegexOptions.IgnoreCase);
                        if (mTitle.Success) title = WebUtility.HtmlDecode(mTitle.Groups["t"].Value);

                        var mPub = Regex.Match(itemXml, @"<pubDate\b[^>]*>(?<t>[\s\S]*?)</pubDate>", RegexOptions.IgnoreCase);
                        if (mPub.Success) pub = WebUtility.HtmlDecode(mPub.Groups["t"].Value);

                        var mDesc = Regex.Match(itemXml, @"<description\b[^>]*>(?<t>[\s\S]*?)</description>", RegexOptions.IgnoreCase);
                        if (mDesc.Success)
                        {
                            var t = mDesc.Groups["t"].Value;
                            // gỡ <![CDATA[ ... ]]>
                            t = Regex.Replace(t, @"^<!\[CDATA\[(.*?)\]\]>$", "$1", RegexOptions.Singleline);
                            desc = WebUtility.HtmlDecode(t);
                        }

                        DateTime ngay = DateTime.Today;
                        DateTime tmp;
                        if (DateTime.TryParse(pub, out tmp)) ngay = tmp;

                        var dict = RssAdapter.ParseKetQuaFromPlainText(desc);
                        list.Add(new RssItem
                        {
                            TieuDe = title,
                            Ngay = ngay,
                            MoTa = desc,
                            KetQuaTheoGiai = dict
                        });
                    }
                }

                if (list.Count == 0)
                    throw new Exception("Doc duoc RSS nhung khong co item hop le.");
            }
            catch (Exception ex)
            {
                throw new Exception("Loi RSS: " + ex.Message);
            }

            return list;
        }

        public class RssItem
        {
            public string TieuDe { get; set; }
            public DateTime Ngay { get; set; }
            public string MoTa { get; set; }
            public Dictionary<string, List<string>> KetQuaTheoGiai { get; set; }
        }
    }
}
