using ASM_GS.Models;

namespace ASM_GS.ViewModels
{
    public class ShopViewModel
    {
        public List<SanPham> Products { get; set; }
        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public string SearchQuery { get; set; }
        public string FilterBy { get; set; }
        public List<DanhMuc> DanhMuc { get; set; }
    }
}

