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
        public string TenGiamGia { get; set; } = null!;

        public decimal GiaTri { get; set; }
        
        public DateOnly NgayBatDau { get; set; }
        
        public DateOnly NgayKetThuc { get; set; }
        public int SoLuongMaNhapToiDa { get; set; }
        
        public int TrangThai { get; set; }
        public virtual ICollection<MaNhapGiamGia> MaNhapGiamGias { get; set; } = new List<MaNhapGiamGia>();
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
        public bool CanAddMoreCodes()
        {
            return MaNhapGiamGias.Count < SoLuongMaNhapToiDa;
        }
    }
}


