﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_GS.Models;

public class RefundRequest
{
    [Key]
    public int Id { get; set; } // Mã yêu cầu hoàn trả

    [Required]
    public string MaDonHang { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string LyDo { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string TrangThai { get; set; } = "Đang chờ"; 

    [DataType(DataType.Date)]
    public DateTime NgayTao { get; set; } = DateTime.Now; 

    [ForeignKey("MaDonHang")]
    public virtual DonHang DonHang { get; set; } = null!;
    public virtual ICollection<RefundRequestImage> RefundRequestImages { get; set; } = new List<RefundRequestImage>();

}