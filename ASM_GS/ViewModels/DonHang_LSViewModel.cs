using ASM_GS.Models;
using System;
using System.Collections.Generic;

namespace ASM_GS.ViewModels
{
    public class DonHang_LSViewModel
    {
        public string MaHoaDon { get; set; }
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public DateOnly NgayXuatHoaDon { get; set; }
        public decimal TongTien { get; set; }
        public int? TrangThai { get; set; }
        public bool IsRefunded { get; set; } // Kiểm tra hóa đơn đã hoàn trả
        public List<ChiTietHoaDon_LSViewMode> ChiTietHoaDons { get; set; }
        public List<RefundRequestImage> RefundRequestImages { get; set; } // Thêm trường này
    }
}
