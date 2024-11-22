using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_GS.Models;

public partial class DonHang
{
    [Key]
    public string MaDonHang { get; set; } = null!;

    public string MaKhachHang { get; set; } = null!;

    public DateOnly NgayDatHang { get; set; }

    public decimal TongTien { get; set; }

    public int? TrangThai { get; set; } // 0: Đang xử lý, 1: Hoàn thành, 2: Đã hủy

    [ForeignKey("MaKhachHang")]
    public virtual KhachHang KhachHang { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
}

