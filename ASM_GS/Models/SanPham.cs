using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace ASM_GS.Models
{
    public partial class SanPham
    {
        [Key]
        public string MaSanPham { get; set; } = null!;

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự.")]
        public string TenSanPham { get; set; } = null!;

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
        public decimal Gia { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc.")]
        public string? MaDanhMuc { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là một số không âm.")]
        public int SoLuong { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày Thêm")]
        public DateOnly? NgayThem { get; set; }

        
        [StringLength(50, ErrorMessage = "Đơn vị không được vượt quá 50 ký tự.")]
        public string? DonVi { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sản xuất")]
        public DateOnly? Nsx { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hạn sử dụng")]
        public DateOnly? Hsd { get; set; }

        [Range(0, 1, ErrorMessage = "Trạng thái phải là 0 hoặc 1.")]
        public int? TrangThai { get; set; }

        // Attribute to upload images from the system
        [NotMapped]
        public List<IFormFile>? UploadImages { get; set; }

        public virtual ICollection<AnhSanPham> AnhSanPhams { get; set; } = new List<AnhSanPham>();

        public virtual ICollection<ChiTietCombo> ChiTietCombos { get; set; } = new List<ChiTietCombo>();

        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

        public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

        public virtual DanhMuc? MaDanhMucNavigation { get; set; }
    }
}
