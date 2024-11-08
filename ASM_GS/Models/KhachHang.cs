using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // Thêm thư viện này cho IFormFile

namespace ASM_GS.Models
{
    public partial class KhachHang
    {
        [Key]
        public string MaKhachHang { get; set; } = null!;

        [Required(ErrorMessage = "Tên khách hàng không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự.")]
        public string TenKhachHang { get; set; } = null!;
        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
        public string? SoDienThoai { get; set; }
        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự.")]
        public string? DiaChi { get; set; }

        [Required]
        public DateOnly NgayDangKy { get; set; }
        //[Required(ErrorMessage = "Vui lòng chọn ảnh.")]
        public string? HinhAnh { get; set; }

        [NotMapped]
        public IFormFile Anh { get; set; }
        [Required(ErrorMessage = "Căn cước công dân không được để trống.")]
        [StringLength(12, ErrorMessage = "CCCD không được vượt quá 12 ký tự.")]
        public string? Cccd { get; set; }

        public DateOnly? NgaySinh { get; set; }

        public bool? GioiTinh { get; set; }

        public int? TrangThai { get; set; }

        public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

        public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

        public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

        public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
    }
}
