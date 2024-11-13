using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASM_GS.Models;

public partial class DanhMuc
{
    [Key]
    public string MaDanhMuc { get; set; } = null!;

    //[Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    //[StringLength(100, ErrorMessage = "Tên danh mục không vượt quá 100 ký tự")]
    public string TenDanhMuc { get; set; } = null!;

    public int? TrangThai { get; set; }
     

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
