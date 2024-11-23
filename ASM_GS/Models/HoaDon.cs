using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_GS.Models
{
    public partial class HoaDon
    {
        [Key]
        public string MaHoaDon { get; set; } = null!;

        public string? MaKhachHang { get; set; }

        public DateOnly NgayXuatHoaDon { get; set; }

        public decimal TongTien { get; set; }

        public int? TrangThai { get; set; }

        public string? MaGiamGia { get; set; }

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

        [ForeignKey("MaGiamGia")]
        public virtual GiamGia? MaGiamGiaNavigation { get; set; }

        // Thiết lập mối quan hệ với KhachHang
        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }
        
    }
}