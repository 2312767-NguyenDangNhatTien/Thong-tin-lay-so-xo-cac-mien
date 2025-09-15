using System;

namespace WindowsFormsApp1   // QUAN TRỌNG: để trùng namespace project
{
    public class KetQua
    {
        public string Dai { get; set; }     // "Mien Bac/Trung/Nam"
        public string Giai { get; set; }    // "Dac biet", "Giai nhat", ...
        public string SoTrung { get; set; } // "12345 - 67890 ..." (se tach thanh list)
        public DateTime Ngay { get; set; }
        public string Tinh { get; set; }    // Tinh/Thanh pho (Mien Trung/Nam)
    }
}