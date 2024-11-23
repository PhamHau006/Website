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

        // Optionally, you can add navigation properties if needed
        public string SanPhamName { get; set; }
        public string ComboName { get; set; }
        public string HinhAnhSanPham { get; set; }
        public string HinhAnhCombo { get; set; }
    }
}