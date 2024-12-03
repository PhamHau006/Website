using Microsoft.AspNetCore.Mvc;
using ASM_GS.ViewModels; // Namespace cho ViewModel
using ASM_GS.Models; // Namespace cho Model
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ASM_GS.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchQuery, string filterBy, string categoryId, string priceRange, int page = 1, int pageSize = 10)
        {
            var products = _context.SanPhams.Include(p => p.AnhSanPhams).AsQueryable();
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

            // Lọc theo khoảng giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "under100k":
                        products = products.Where(p => p.Gia < 100000);
                        break;
                    case "100k-500k":
                        products = products.Where(p => p.Gia >= 100000 && p.Gia <= 500000);
                        break;
                    case "500k-1m":
                        products = products.Where(p => p.Gia > 500000 && p.Gia <= 1000000);
                        break;
                    case "above1m":
                        products = products.Where(p => p.Gia > 1000000);
                        break;
                }
            }

            // Lọc theo tiêu chí khác
            switch (filterBy)
            {
                case "Price":
                    products = products.OrderBy(p => p.Gia);
                    break;
                case "Best Sellers":
                    // Thực hiện lọc theo sản phẩm bán chạy. Ví dụ: dựa trên số lượng bán
                    products = products.OrderByDescending(p => p.ChiTietHoaDons.Count); // Giả sử bạn có bảng OrderDetails chứa các đơn hàng
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
