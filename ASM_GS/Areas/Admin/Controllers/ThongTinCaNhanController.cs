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
            // Validate phone number (assuming 10 digits for Vietnam)
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.SoDienThoai, @"^\d{10}$"))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại phải là 10 chữ số.");
            }

            // Validate date of birth and start date
            if (model.NgaySinh != null && model.NgayBatDau != null)
            {
                int ageDifference = model.NgayBatDau.Value.Year - model.NgaySinh.Value.Year;
                if (ageDifference < 18 || (ageDifference == 18 && model.NgayBatDau.Value < model.NgaySinh.Value.AddYears(18)))
                {
                    ModelState.AddModelError("NgaySinh", "Ngày bắt đầu phải cách ngày sinh ít nhất 18 năm.");
                }
            }

            // Validate CCCD (12 digits for Vietnamese Citizen ID)
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.Cccd, @"^\d{12}$"))
            {
                ModelState.AddModelError("Cccd", "CCCD phải là 12 chữ số.");
            }

            // If any validation errors exist, return the model to the view
            if (ModelState.IsValid)
            {
                return View("Index", model);
            }

            var nhanVien = await _context.NhanViens.FindAsync(model.MaNhanVien);
            if (nhanVien == null)
            {
                return NotFound();
            }

            // Process image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                // Define the path to save the image
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder); // Ensure the folder exists

                // Generate a unique filename
                var uniqueFileName = $"{model.MaNhanVien}_{Path.GetFileName(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Update the image path in the database
                nhanVien.HinhAnh = $"/uploads/{uniqueFileName}";
            }

            // Update other fields
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

            TempData["Message"] = "Thông tin cá nhân đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }

    }
}
