using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace ASM_GS.Controllers
{
    public class InformationPersonalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InformationPersonalController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Check if the user is logged in by retrieving the account session
            string accountId = HttpContext.Session.GetString("UserAccount");

            if (string.IsNullOrEmpty(accountId))
            {
                // If account session is empty, request login
                ViewBag.Message = "Vui lòng đăng nhập để xem thông tin cá nhân.";
                return View(new KhachHang()); // Return an empty model to avoid null reference
            }

            // Retrieve customer information based on the account ID
            var account = _context.TaiKhoans
                                  .Include(tk => tk.MaKhachHangNavigation)
                                  .FirstOrDefault(tk => tk.MaTaiKhoan == accountId);

            if (account == null || account.MaKhachHangNavigation == null)
            {
                ViewBag.Message = "Không tìm thấy thông tin khách hàng.";
                return View(new KhachHang()); // Return an empty model to avoid null reference
            }

            // Pass customer information to the View for display
            return View(account.MaKhachHangNavigation);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] KhachHang updatedKhachHang, IFormFile Anh)
        {
            string accountId = HttpContext.Session.GetString("UserAccount");

            if (string.IsNullOrEmpty(accountId))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để chỉnh sửa thông tin cá nhân." });
            }

            var account = await _context.TaiKhoans
                                        .Include(tk => tk.MaKhachHangNavigation)
                                        .FirstOrDefaultAsync(tk => tk.MaTaiKhoan == accountId);

            if (account?.MaKhachHangNavigation == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
            }

            var khachHang = account.MaKhachHangNavigation;

            // Validate số điện thoại
            if (!System.Text.RegularExpressions.Regex.IsMatch(updatedKhachHang.SoDienThoai ?? "", @"^0\d{9,11}$"))
            {
                return Json(new { success = false, message = "Số điện thoại không hợp lệ." });
            }

            // Validate căn cước công dân (CCCD)
            if (!System.Text.RegularExpressions.Regex.IsMatch(updatedKhachHang.Cccd ?? "", @"^\d{12}$"))
            {
                return Json(new { success = false, message = "CCCD không hợp lệ." });
            }

            // Chỉ cập nhật ảnh nếu có tệp mới được tải lên
            if (Anh != null && Anh.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/AnhKhachHang", Anh.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Anh.CopyToAsync(stream);
                }
                khachHang.HinhAnh = $"/AnhKhachHang/{Anh.FileName}";
            }

            // Cập nhật các thông tin khác
            khachHang.TenKhachHang = updatedKhachHang.TenKhachHang;
            khachHang.SoDienThoai = updatedKhachHang.SoDienThoai;
            khachHang.DiaChi = updatedKhachHang.DiaChi;
            khachHang.Cccd = updatedKhachHang.Cccd;
            khachHang.NgaySinh = updatedKhachHang.NgaySinh;
            khachHang.GioiTinh = updatedKhachHang.GioiTinh;

            _context.KhachHangs.Update(khachHang);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thông tin cá nhân đã được cập nhật thành công!" });
        }



        [HttpGet]
        public IActionResult CheckLoginStatus()
        {
            // Kiểm tra nếu mã khách hàng tồn tại trong session thì nghĩa là đã đăng nhập
            bool isLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("UserAccount"));
            return Json(new { isLoggedIn });
        }
    }
}
