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
    public class KhachHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KhachHangController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/KhachHang
        public IActionResult Index(string searchTerm, int? pageSize, int page = 1, int? status = null)
        {
            if (HttpContext.Session.GetString("StaffAccount") == null)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
                ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            }

            int pageSizeValue = pageSize ?? 5;
            var khachHangs = _context.KhachHangs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                khachHangs = khachHangs.Where(kh => kh.TenKhachHang.Contains(searchTerm) ||
                                                     kh.SoDienThoai.Contains(searchTerm) ||
                                                     kh.Cccd.Contains(searchTerm));
            }
            if (status.HasValue)
            {
                khachHangs = khachHangs.Where(kh => kh.TrangThai == status);
            }
            // Phân trang sử dụng ToPagedList
            var pagedKhachHangs = khachHangs.ToPagedList(page, pageSizeValue);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_KhachHangTable", pagedKhachHangs); 
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.PageSize = pageSizeValue;
            ViewBag.Page = page;
            return View(pagedKhachHangs); 
        }




        // GET: Admin/KhachHang/Create
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_UserCreatePartial");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachHang khachHang)
        {
            // Tạo mã khách hàng ngẫu nhiên
            Random random = new Random();
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string maKhachHang = "KH" + string.Concat(Enumerable.Range(0, 6).Select(_ => characters[random.Next(characters.Length)]));
            khachHang.MaKhachHang = maKhachHang;


            // Xử lý lưu ảnh nếu có
            if (khachHang.Anh != null)
            {
                var fileName = $"{Guid.NewGuid()}_{khachHang.Anh.FileName}";
                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/AnhKhachHang");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await khachHang.Anh.CopyToAsync(stream);
                }
                khachHang.HinhAnh = $"img/AnhKhachHang/{fileName}";
            }

            // Nếu ModelState không hợp lệ, trả về PartialView với các lỗi
            if (ModelState.IsValid)
            {
                return PartialView("_UserCreatePartial", khachHang); // Trả về modal `_UserCreatePartial`
            }

            // Nếu ModelState hợp lệ, tiếp tục xử lý
            khachHang.NgayDangKy = DateOnly.FromDateTime(DateTime.Now);
            _context.Add(khachHang);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }
            return PartialView("_UserEditPartial", khachHang);
        }

        // POST: Admin/KhachHang/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, KhachHang khachHang)
        {
            if (id != khachHang.MaKhachHang)
            {
                return NotFound();
            }
                // Tìm khách hàng hiện tại trong cơ sở dữ liệu
                var existingKhachHang = await _context.KhachHangs.FindAsync(id);
                if (existingKhachHang == null)
                {
                    return NotFound();
                }

                existingKhachHang.TenKhachHang = khachHang.TenKhachHang;
                existingKhachHang.DiaChi = khachHang.DiaChi;
                existingKhachHang.SoDienThoai = khachHang.SoDienThoai;
                existingKhachHang.Cccd = khachHang.Cccd;
                existingKhachHang.NgaySinh = khachHang.NgaySinh;
                existingKhachHang.GioiTinh = khachHang.GioiTinh;
                existingKhachHang.TrangThai = khachHang.TrangThai;

                // Xử lý ảnh mới nếu có
                if (khachHang.Anh != null)
                {
                    var fileName = $"{Guid.NewGuid()}_{khachHang.Anh.FileName}";
                    var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "img/AnhKhachHang");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    var filePath = Path.Combine(folderPath, fileName);

                    // Xóa ảnh cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(existingKhachHang.HinhAnh))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingKhachHang.HinhAnh);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await khachHang.Anh.CopyToAsync(stream);
                    }
                    existingKhachHang.HinhAnh = $"img/AnhKhachHang/{fileName}";
                }
                _context.Update(existingKhachHang);
                var result = await _context.SaveChangesAsync();

                if (result > 0) 
                {
                    TempData["SuccessMessage"] = "Cập nhật khách hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không có thay đổi nào được thực hiện.");
                }     
            TempData["ErrorMessage"] = "Cập nhật không thành công!";
            return PartialView("_UserEditPartial", khachHang);
        }

        public IActionResult Delete(string id)
        {
            var khachHang = _context.KhachHangs.Find(id); 
            if (khachHang == null)
            {
                return NotFound();
            }
            var taiKhoan = _context.TaiKhoans.Where(a => (a.MaKhachHang == khachHang.MaKhachHang)).FirstOrDefault();
            if(taiKhoan!=null)
            _context.TaiKhoans.Remove(taiKhoan);
            _context.KhachHangs.Remove(khachHang);

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Xoá khách hàng thành công!";
            return RedirectToAction("Index");
        }

        private bool KhachHangExists(string id)
        {
            return _context.KhachHangs.Any(e => e.MaKhachHang == id);
        }
        //Phần Quý Làm: 
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(KhachHang customer, IFormFile Anh)
        {
            var errors = new Dictionary<string, string>();

            // Validate customer fields
            if (string.IsNullOrEmpty(customer.TenKhachHang))
            {
                errors.Add("TenKhachHang", "Tên khách hàng không được để trống.");
            }

            if (string.IsNullOrEmpty(customer.SoDienThoai))
            {
                errors.Add("SoDienThoai", "Số điện thoại không được để trống.");
            }
            else if (!Regex.IsMatch(customer.SoDienThoai, @"^\d{10,11}$"))
            {
                errors.Add("SoDienThoai", "Số điện thoại không hợp lệ.");
            }

            if (string.IsNullOrEmpty(customer.DiaChi))
            {
                errors.Add("DiaChi", "Địa chỉ không được để trống.");
            }

            if (string.IsNullOrEmpty(customer.Cccd))
            {
                errors.Add("Cccd", "Căn cước công dân không được để trống.");
            }
            else if (!Regex.IsMatch(customer.Cccd, @"^0\d{11}$"))
            {
                errors.Add("Cccd", "Căn cước công dân phải đủ 12 ký tự và bắt đầu bằng số 0.");
            }

            if (string.IsNullOrEmpty(customer.NgaySinh?.ToString()))
            {
                errors.Add("NgaySinh", "Ngày sinh không được để trống.");
            }
            else if (customer.NgaySinh > DateOnly.FromDateTime(DateTime.Now.AddYears(-15)))
            {
                errors.Add("NgaySinh", "Khách hàng phải đủ 15 tuổi.");
            }
            if(customer.GioiTinh!=true && customer.GioiTinh!=false)
            {
                errors.Add("GioiTinh", "Vui lòng chọn giới tính");
            }    
            
            // Handle file upload for the image (Anh)
            if (Anh != null && Anh.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(Anh.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/AnhKhachHang", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await Anh.CopyToAsync(stream);
                }
                customer.HinhAnh = "/img/AnhKhachHang/" + fileName;
            }
            else
            {
                errors.Add("Anh", "Vui lòng tải lên hình ảnh hợp lệ.");
            }
            if (errors.Any())
            {
                return Json(new { success = false, errors = errors });
            }
            // Generate a unique, sequential MaKhachHang
            var lastCustomer = await _context.KhachHangs
                                             .OrderByDescending(kh => kh.MaKhachHang)
                                             .FirstOrDefaultAsync();

            int nextId = 1;
            if (lastCustomer != null)
            {
                string lastIdStr = lastCustomer.MaKhachHang.Substring(2); 
                if (int.TryParse(lastIdStr, out int lastId))
                {
                    nextId = lastId + 1;
                }
            }
            customer.MaKhachHang = "KH" + nextId.ToString("D3"); 


            customer.TrangThai = 1;
            customer.NgayDangKy = DateOnly.FromDateTime(DateTime.Now);


            _context.KhachHangs.Add(customer);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Khách hàng đã được thêm thành công!" });
        }
        [HttpGet]
        public async Task<IActionResult> EditCustomer(string id)
        {
            var customer = await _context.KhachHangs.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            return Json(customer);
        }

        // Action to handle updating customer information
        [HttpPost]
        public async Task<IActionResult> EditCustomer2(KhachHang updatedCustomer, IFormFile Anh)
        {
            var errors = new Dictionary<string, string>();

            // Validate customer fields
            if (string.IsNullOrEmpty(updatedCustomer.TenKhachHang))
            {
                errors.Add("TenKhachHang", "Tên khách hàng không được để trống.");
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
                errors.Add("NgaySinh", "Khách hàng phải đủ 15 tuổi.");
            }

            if (updatedCustomer.GioiTinh != true && updatedCustomer.GioiTinh != false)
            {
                errors.Add("GioiTinh", "Vui lòng chọn giới tính.");
            }

            // Handle file upload for the image (Anh)
            if (Anh != null && Anh.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(Anh.FileName);

                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/AnhKhachHang", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await Anh.CopyToAsync(stream);
                }

                updatedCustomer.HinhAnh = "/img/AnhKhachHang/" + fileName;
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
            var existingCustomer = await _context.KhachHangs.FindAsync(updatedCustomer.MaKhachHang);
            if (existingCustomer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khách hàng." });
            }

            // Update fields
            existingCustomer.TenKhachHang = updatedCustomer.TenKhachHang;
            existingCustomer.SoDienThoai = updatedCustomer.SoDienThoai;
            existingCustomer.DiaChi = updatedCustomer.DiaChi;
            existingCustomer.Cccd = updatedCustomer.Cccd;
            existingCustomer.NgaySinh = updatedCustomer.NgaySinh;
            existingCustomer.GioiTinh = updatedCustomer.GioiTinh;
            if (updatedCustomer.HinhAnh != null)
            {
                existingCustomer.HinhAnh = updatedCustomer.HinhAnh;
            }

            _context.KhachHangs.Update(existingCustomer);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Khách hàng đã được cập nhật thành công!" });
        }

    }
}
