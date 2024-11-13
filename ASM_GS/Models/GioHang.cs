using ASM_GS.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public partial class GioHang
{
    [Key]
    public string MaGioHang { get; set; } = null!;

    public string? MaKhachHang { get; set; }

    public DateOnly NgayTao { get; set; }

    [ForeignKey("MaKhachHang")]
    public virtual KhachHang? KhachHang { get; set; }

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
}
