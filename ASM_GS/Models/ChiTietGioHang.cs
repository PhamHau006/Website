using ASM_GS.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public partial class ChiTietGioHang
{
    [Key]
    public int Id { get; set; }

    public string MaGioHang { get; set; } = null!;

    [ForeignKey("MaGioHang")]
    public virtual GioHang GioHang { get; set; }

    public string? MaSanPham { get; set; } // Cho phép null

    [ForeignKey("MaSanPham")]
    public virtual SanPham? SanPham { get; set; } // Cho phép null

    public string? MaCombo { get; set; }

    [ForeignKey("MaCombo")]
    public virtual Combo? Combo { get; set; }

    public int SoLuong { get; set; }
}
