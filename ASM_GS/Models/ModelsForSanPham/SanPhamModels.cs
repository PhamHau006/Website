using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ASM_GS.Models.ModelsForSanPham
{
    public class SanPhamModels
    {
        [Key]
        public string MaSanPham { get; set; } = null!;

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string TenSanPham { get; set; } = null!;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0, 100000000, ErrorMessage = "Giá phải lớn hơn 0 và bé hơn 100 triệu")]
        public decimal? Gia { get; set; }


        public string? MoTa { get; set; }


        public string? MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, 1000, ErrorMessage = "Số lượng phải lớn hơn 0 và bé hơn 1000")]
        public int? SoLuong { get; set; }


        public DateOnly? NgayThem { get; set; }



        public string? DonVi { get; set; }


        public DateOnly? Nsx { get; set; }


        public DateOnly? Hsd { get; set; }


        public int? TrangThai { get; set; }


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
