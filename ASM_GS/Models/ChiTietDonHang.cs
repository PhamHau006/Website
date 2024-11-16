using ASM_GS.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public partial class ChiTietDonHang
{
    [Key]
    public int MaChiTietDonHang { get; set; }

    public string MaDonHang { get; set; } = null!;

    public string? MaSanPham { get; set; }

    public string? MaCombo { get; set; }

    public int SoLuong { get; set; }

    public decimal Gia { get; set; }

    [ForeignKey("MaDonHang")]
    public virtual DonHang DonHang { get; set; }

    [ForeignKey("MaSanPham")]
    public virtual SanPham? MaSanPhamNavigation { get; set; }

    [ForeignKey("MaCombo")]
    public virtual Combo? Combo { get; set; }
}
