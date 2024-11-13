using ASM_GS.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASM_GS.Models
{
    public partial class GiamGia
    {
        [Key]

        public string MaGiamGia { get; set; } = null!;
        //[Required(ErrorMessage = "Tên giảm giá là bắt buộc.")]
        //[StringLength(100, ErrorMessage = "Tên giảm giá không được vượt quá 100 ký tự.")]
        public string TenGiamGia { get; set; } = null!;

        //[Required(ErrorMessage = "Giá trị không được để trống.")]
        //[Range(0.01, 1.00, ErrorMessage = "Giá trị giảm giá phải nằm trong khoảng từ 0.01 đến 1.00 (tức là 1% đến 100%).")]
        public decimal GiaTri { get; set; }
        
        //[Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
        public DateOnly NgayBatDau { get; set; }
        
        //[Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
        //[DateGreaterThan(nameof(NgayBatDau), ErrorMessage = "Ngày kết thúc phải sau ngày bắt đầu.")]
        public DateOnly NgayKetThuc { get; set; }
        // Thêm giới hạn số lượng mã nhập giảm giá
        
        //[Required(ErrorMessage = "Số lượng mã nhập không được để trống.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Số lượng mã nhập phải lớn hơn 0.")]
        public int SoLuongMaNhapToiDa { get; set; }
        
        //[Range(0, 1, ErrorMessage = "Trạng thái phải là Không áp dụng hoặc Đang áp dụng.")]
        public int TrangThai { get; set; }
        // Liên kết với các mã nhập chi tiết
        public virtual ICollection<MaNhapGiamGia> MaNhapGiamGias { get; set; } = new List<MaNhapGiamGia>();
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
        // Phương thức kiểm tra nếu có thể thêm mã nhập mới
        public bool CanAddMoreCodes()
        {
            return MaNhapGiamGias.Count < SoLuongMaNhapToiDa;
        }
    }
}


