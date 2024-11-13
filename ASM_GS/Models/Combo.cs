using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // Cần cho IFormFile

namespace ASM_GS.Models
{
    public partial class Combo
    {
        [Key]
        //[Required(ErrorMessage = "Mã Combo không được để trống")]
        public string MaCombo { get; set; } = null!;

        //[Required(ErrorMessage = "Tên Combo không được để trống.")]
        //[StringLength(100, ErrorMessage = "Tên Combo không được vượt quá 100 ký tự.")]
        public string TenCombo { get; set; } = null!;

        public string? MoTa { get; set; } // Không bắt buộc, có thể để trống

        //[Required(ErrorMessage = "Giá không được để trống.")]
        //[Range(0.01, double.MaxValue, ErrorMessage = "Giá phải là một số dương lớn hơn 0.")]
        public decimal Gia { get; set; }

        //[Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        //[Range(0, 1, ErrorMessage = "Trạng thái phải là Không áp dụng hoặc Đang áp dụng.")]
        public int TrangThai { get; set; }

        //[Required(ErrorMessage = "Vui lòng chọn ảnh!")]
        public string? Anh { get; set; }

        [NotMapped] // Không lưu thuộc tính này vào CSDL
        public IFormFile? anhcombo { get; set; } // Thuộc tính để upload ảnh

        public virtual ICollection<ChiTietCombo> ChiTietCombos { get; set; } = new List<ChiTietCombo>();
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
    }
}
