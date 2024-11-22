using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_GS.Models;

public class RefundRequestImage
{
    [Key]
    public int Id { get; set; }  // Mã ảnh

    [Required]
    public string ImageUrl { get; set; } = null!; // URL của ảnh (hoặc bạn có thể lưu trữ dưới dạng Blob nếu lưu trực tiếp vào DB)

    public int RefundRequestId { get; set; }  // Khóa ngoại tới RefundRequest

    [ForeignKey("RefundRequestId")]
    public virtual RefundRequest RefundRequest { get; set; } = null!;
}
