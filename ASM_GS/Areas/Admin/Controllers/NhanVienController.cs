using ASM_GS.Controllers;
using ASM_GS.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList.Extensions;
using X.PagedList;
using X.PagedList.Mvc.Core;
//using ASM_GS.Migrations;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text.RegularExpressions;


namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhanVienController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public NhanVienController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index(string searchTerm, int? pageSize, int page = 1)
        {
            if (HttpContext.Session.GetString("StaffAccount") == null)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
                ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            }

            int pageSizeValue = pageSize ?? 5;
            var nhanViens = _context.NhanViens.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                nhanViens = nhanViens.Where(kh => kh.TenNhanVien.Contains(searchTerm) ||
                                                     kh.SoDienThoai.Contains(searchTerm) ||
                                                     kh.Cccd.Contains(searchTerm));
            }

            var pagednhanViens = nhanViens.ToPagedList(page, pageSizeValue);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_nhanVienTable", pagednhanViens); 
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.PageSize = pageSizeValue;
            ViewBag.Page = page;

            return View(pagednhanViens); 
        }
        public IActionResult Delete(string id)
        {
            var nhanVien = _context.NhanViens.Find(id);
            if (nhanVien == null)
            {
                return NotFound();
            }
            var taiKhoan = _context.TaiKhoans.Where(a => (a.MaNhanVien == nhanVien.MaNhanVien)).FirstOrDefault();
            if(taiKhoan!=null)
            {
                _context.TaiKhoans.Remove(taiKhoan);
            }    
            _context.NhanViens.Remove(nhanVien);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Xoá nhân viên thành công!";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> CreateStaff(string TenNhanVien, string SoDienThoai, string DiaChi, string VaiTro, IFormFile Anh, string Cccd, DateTime NgaySinh, bool GioiTinh)
        {
            var errors = new Dictionary<string, string>();

            // Validate customer fields
            if (string.IsNullOrEmpty(TenNhanVien))
            {
                errors.Add("TenNhanVien", "Tên nhân viên không được để trống.");
            }

            if (string.IsNullOrEmpty(SoDienThoai))
            {
                errors.Add("SoDienThoai", "Số điện thoại không được để trống.");
            }
            else if (!Regex.IsMatch(SoDienThoai, @"^\d{10,11}$"))
            {
                errors.Add("SoDienThoai", "Số điện thoại không hợp lệ.");
            }

            if (string.IsNullOrEmpty(DiaChi))
            {
                errors.Add("DiaChi", "Địa chỉ không được để trống.");
            }

            if (string.IsNullOrEmpty(Cccd))
            {
                errors.Add("Cccd", "Căn cước công dân không được để trống.");
            }
            else if (!Regex.IsMatch(Cccd, @"^0\d{11}$"))
            {
                errors.Add("Cccd", "Căn cước công dân phải đủ 12 ký tự và bắt đầu bằng số 0.");
            }

            if (NgaySinh == null)
            {
                errors.Add("NgaySinh", "Ngày sinh không được để trống.");
            }
            else if (NgaySinh > DateTime.Now.AddYears(-15))
            {
                errors.Add("NgaySinh", "Nhân viên phải đủ 15 tuổi.");
            }

            if (GioiTinh == null)
            {
                errors.Add("GioiTinh", "Vui lòng chọn giới tính.");
            }

            if (VaiTro != "Staff" && VaiTro != "Admin")
            {
                errors.Add("VaiTro", "Vui lòng chọn vai trò.");
            }

            // Kiểm tra trùng số điện thoại
            var isPhoneExist = await _context.NhanViens
                .AnyAsync(nv => nv.SoDienThoai == SoDienThoai);
            if (isPhoneExist)
            {
                errors.Add("SoDienThoai", "Số điện thoại đã tồn tại.");
            }

            // Kiểm tra trùng căn cước công dân
            var isCccdExist = await _context.NhanViens
                .AnyAsync(nv => nv.Cccd == Cccd);
            if (isCccdExist)
            {
                errors.Add("Cccd", "Căn cước công dân đã tồn tại.");
            }

            string fileName = null;

            // Handle file upload for the image (Anh)
            if (Anh != null && Anh.Length > 0)
            {
                fileName = Guid.NewGuid() + Path.GetExtension(Anh.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/AnhNhanVien", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await Anh.CopyToAsync(stream);
                }
            }
            else
            {
                errors.Add("Anh", "Vui lòng tải lên hình ảnh hợp lệ.");
            }

            if (errors.Any())
            {
                return Json(new { success = false, errors = errors });
            }

            var lastStaff = await _context.NhanViens
                                            .OrderByDescending(kh => kh.MaNhanVien)
                                            .FirstOrDefaultAsync();

            int nextId = 1;
            if (lastStaff != null)
            {
                string lastIdStr = lastStaff.MaNhanVien.Substring(2);
                if (int.TryParse(lastIdStr, out int lastId))
                {
                    nextId = lastId + 1;
                }
            }

            // Create new NhanVien object and save to DB
            var staff = new NhanVien
            {
                MaNhanVien = "NV" + nextId.ToString("D3"),
                TenNhanVien = TenNhanVien,
                SoDienThoai = SoDienThoai,
                DiaChi = DiaChi,
                VaiTro = VaiTro,
                Cccd = Cccd,
                NgaySinh = DateOnly.FromDateTime(NgaySinh),
                GioiTinh = GioiTinh,
                HinhAnh = "/img/AnhNhanVien/" + fileName,
                NgayBatDau = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.NhanViens.Add(staff);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Nhân viên đã được thêm thành công!" });
        }

        [HttpGet]
        public async Task<IActionResult> EditStaff(string id)
        {
            var staff = await _context.NhanViens.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            return Json(staff);
        }
        [HttpPost]
        public async Task<IActionResult> EditStaff2(NhanVien updatedCustomer, IFormFile Anh)
        {
            var errors = new Dictionary<string, string>();

            // Validate customer fields
            if (string.IsNullOrEmpty(updatedCustomer.TenNhanVien))
            {
                errors.Add("TenKhachHang", "Tên Nhân Viên không được để trống.");
            }

            if (string.IsNullOrEmpty(updatedCustomer.SoDienThoai))
            {
                errors.Add("SoDienThoai", "Số điện thoại không được để trống.");
            }
            else if (!Regex.IsMatch(updatedCustomer.SoDienThoai, @"^\d{10,11}$"))
            {
                errors.Add("SoDienThoai", "Số điện thoại không hợp lệ.");
            }

            if (string.IsNullOrEmpty(updatedCustomer.DiaChi))
            {
                errors.Add("DiaChi", "Địa chỉ không được để trống.");
            }

            if (string.IsNullOrEmpty(updatedCustomer.Cccd))
            {
                errors.Add("Cccd", "Căn cước công dân không được để trống.");
            }
            else if (!Regex.IsMatch(updatedCustomer.Cccd, @"^0\d{11}$"))
            {
                errors.Add("Cccd", "Căn cước công dân phải đủ 12 ký tự và bắt đầu bằng số 0.");
            }

            if (string.IsNullOrEmpty(updatedCustomer.NgaySinh?.ToString()))
            {
                errors.Add("NgaySinh", "Ngày sinh không được để trống.");
            }
            else if (updatedCustomer.NgaySinh > DateOnly.FromDateTime(DateTime.Now.AddYears(-15)))
            {
                errors.Add("NgaySinh", "Nhân Viên phải đủ 15 tuổi.");
            }
            if (updatedCustomer.VaiTro != "Staff" && updatedCustomer.VaiTro != "Admin")
            {
                errors.Add("VaiTro", "Vui lòng chọn vai trò.");
            }
            if (updatedCustomer.GioiTinh != true && updatedCustomer.GioiTinh != false)
            {
                errors.Add("GioiTinh", "Vui lòng chọn giới tính.");
            }

            // Handle file upload for the image (Anh)
            if (Anh != null && Anh.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(Anh.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/AnhNhanVien", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await Anh.CopyToAsync(stream);
                }

                updatedCustomer.HinhAnh = "/img/AnhNhanVien/" + fileName;
            }
            else
            {
                errors.Add("Anh", "Vui lòng tải lên hình ảnh hợp lệ.");
            }

            if (errors.Any())
            {
                return Json(new { success = false, errors = errors });
            }

            // Retrieve the existing customer from the database
            var existingCustomer = await _context.NhanViens.FindAsync(updatedCustomer.MaNhanVien);
            if (existingCustomer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy Nhân Viên." });
            }

            // Update fields
            existingCustomer.TenNhanVien = updatedCustomer.TenNhanVien;
            existingCustomer.SoDienThoai = updatedCustomer.SoDienThoai;
            existingCustomer.DiaChi = updatedCustomer.DiaChi;
            existingCustomer.Cccd = updatedCustomer.Cccd;
            existingCustomer.NgaySinh = updatedCustomer.NgaySinh;
            existingCustomer.GioiTinh = updatedCustomer.GioiTinh;
            existingCustomer.VaiTro = updatedCustomer.VaiTro;
            if (updatedCustomer.HinhAnh != null)
            {
                existingCustomer.HinhAnh = updatedCustomer.HinhAnh;
            }

            _context.NhanViens.Update(existingCustomer);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Nhân viên đã được cập nhật thành công!" });
        }
    }
}
