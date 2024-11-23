using ASM_GS.Controllers;
using ASM_GS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongTinCaNhanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ThongTinCaNhanController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var maTaiKhoan = HttpContext.Session.GetString("StaffAccount");
            if (string.IsNullOrEmpty(maTaiKhoan))
            {
                return RedirectToAction("Index", "LoginAdmin");
            }

            var user = _context.TaiKhoans.FirstOrDefault(u => u.MaTaiKhoan == maTaiKhoan);
            if (user == null || user.MaNhanVien == null)
            {
                return NotFound("Employee data not found.");
            }

            var nhanVien = _context.NhanViens.FirstOrDefault(nv => nv.MaNhanVien == user.MaNhanVien);
            if (nhanVien == null)
            {
                return NotFound("Employee data not found.");
            }

            return View(nhanVien);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(NhanVien model, IFormFile imageFile)
        {
            var errors = new Dictionary<string, string>();

            // Validate số điện thoại
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.SoDienThoai, @"^0\d{9,11}$"))
            {
                errors.Add("SoDienThoai", "Số điện thoại không hợp lệ.");
            }

            // Validate ngày sinh và ngày bắt đầu
            if (model.NgaySinh.HasValue && model.NgayBatDau.HasValue)
            {
                int age = model.NgayBatDau.Value.Year - model.NgaySinh.Value.Year;
                if (age < 18 || (age == 18 && model.NgayBatDau.Value < model.NgaySinh.Value.AddYears(18)))
                {
                    errors.Add("NgaySinh", "Bạn chưa đủ 18 tuổi.");
                }
            }

            // Validate CCCD
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.Cccd, @"^\d{12}$"))
            {
                errors.Add("Cccd", "CCCD không hợp lệ.");
            }

            // Nếu có lỗi, trả về lỗi
            if (errors.Count > 0)
            {
                return Json(new { success = false, errors });
            }

            var nhanVien = await _context.NhanViens.FindAsync(model.MaNhanVien);
            if (nhanVien == null)
            {
                return Json(new { success = false, message = "Nhân viên không tồn tại." });
            }

            // Xử lý upload ảnh mới
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img", "AnhNhanVien");
                Directory.CreateDirectory(uploadsFolder);

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(nhanVien.HinhAnh))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, nhanVien.HinhAnh.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Tạo tên tệp mới và lưu
                var uniqueFileName = $"{model.MaNhanVien}_{Path.GetFileName(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                nhanVien.HinhAnh = $"/img/AnhNhanVien/{uniqueFileName}";
            }

            // Cập nhật thông tin nhân viên
            nhanVien.TenNhanVien = model.TenNhanVien;
            nhanVien.VaiTro = model.VaiTro;
            nhanVien.SoDienThoai = model.SoDienThoai;
            nhanVien.NgayBatDau = model.NgayBatDau;
            nhanVien.NgaySinh = model.NgaySinh;
            nhanVien.DiaChi = model.DiaChi;
            nhanVien.TrangThai = model.TrangThai;
            nhanVien.Cccd = model.Cccd;
            nhanVien.GioiTinh = model.GioiTinh;

            _context.Update(nhanVien);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thông tin cá nhân đã được cập nhật thành công!" });
        }





    }
}
