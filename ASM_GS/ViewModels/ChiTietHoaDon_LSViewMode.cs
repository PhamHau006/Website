namespace ASM_GS.ViewModels
{
    public class ChiTietHoaDon_LSViewMode
    {
        public int MaChiTietDonHang { get; set; }

        public string MaDonHang { get; set; }

        public string? MaSanPham { get; set; }

        public string? MaCombo { get; set; }

        public int SoLuong { get; set; }

        public decimal Gia { get; set; }

        public string ComboName { get; set; }
        public string SanPhamName { get; set; }
       
        public bool IsRefunded { get; set; } = false;
        public string HinhAnhSanPham { get; set; }
        public string HinhAnhCombo { get; set; }
    }
}