using Microsoft.AspNetCore.Mvc;
using ASM_GS.ViewModels; // Namespace cho ViewModel
using ASM_GS.Models; // Namespace cho Model
using System.Linq;

namespace ASM_GS.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchQuery, string filterBy, string categoryId, int page = 1, int pageSize = 10)
        {
            var products = _context.SanPhams.AsQueryable();
            var danhMuc = _context.DanhMucs.ToList(); // Lấy danh sách danh mục

            // Tìm kiếm sản phẩm
            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products.Where(p => p.TenSanPham.Contains(searchQuery));
            }

            // Lọc theo danh mục
            if (!string.IsNullOrEmpty(categoryId))
            {
                products = products.Where(p => p.MaDanhMuc == categoryId);
            }

            // Lọc theo tiêu chí khác
            switch (filterBy)
            {
                case "Price":
                    products = products.OrderBy(p => p.Gia);
                    break;
                case "Best Sellers":
                    products = products.OrderByDescending(p => p.Gia); // Thay đổi theo nhu cầu của bạn
                    break;
                case "New Arrivals":
                    products = products.OrderByDescending(p => p.NgayThem);
                    break;
            }

            // Phân trang
            int totalItems = products.Count();
            products = products.Skip((page - 1) * pageSize).Take(pageSize);

            var viewModel = new ShopViewModel
            {
                Products = products.ToList(),
                DanhMuc = danhMuc,
                CurrentPage = page,
                TotalItems = totalItems,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                FilterBy = filterBy
            };

            return View(viewModel);
        }
    }
}
