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
        public string MaCombo { get; set; } = null!;
        public string TenCombo { get; set; } = null!;

        public string? MoTa { get; set; } // Không bắt buộc, có thể để trống

        public decimal Gia { get; set; }

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
