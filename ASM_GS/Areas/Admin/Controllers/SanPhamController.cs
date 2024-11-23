﻿using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using ASM_GS.Controllers;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Http.Extensions;


namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SanPhamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SanPhamController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(string searchName, string categoryId, int? status, string sortOrder, int? page, int? pageSize = 10)
        {
            // Kiểm tra session đăng nhập
            if (HttpContext.Session.GetString("StaffAccount") == null)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
                ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            }

            int defaultPageSize = pageSize ?? 10;
            int pageNumber = page ?? 1;

            // Query sản phẩm
            var sanPhams = _context.SanPhams
                .Include(s => s.AnhSanPhams)
                .Include(s => s.MaDanhMucNavigation)
                .AsQueryable();

            // Lọc theo tên sản phẩm
            if (!string.IsNullOrEmpty(searchName))
            {
                sanPhams = sanPhams.Where(s => s.TenSanPham.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(categoryId))
            {
                sanPhams = sanPhams.Where(s => s.MaDanhMuc == categoryId);
            }


            // Lọc theo trạng thái
            if (status.HasValue)
            {
                sanPhams = sanPhams.Where(s => s.TrangThai == status.Value);
            }

            // Áp dụng sắp xếp
            switch (sortOrder)
            {
                case "price_asc":
                    sanPhams = sanPhams.OrderBy(s => s.Gia);
                    break;
                case "price_desc":
                    sanPhams = sanPhams.OrderByDescending(s => s.Gia);
                    break;
                case "name_desc":
                    sanPhams = sanPhams.OrderByDescending(s => s.TenSanPham);
                    break;
                default:
                    sanPhams = sanPhams.OrderBy(s => s.TenSanPham);
                    break;
            }

            // Truyền danh sách danh mục sang ViewBag dưới dạng List<DanhMuc>
            ViewBag.DanhMucList = await _context.DanhMucs.ToListAsync();

            // Truyền các giá trị đã chọn sang View
            ViewBag.SearchName = searchName;
            ViewBag.CategoryId = categoryId;
            ViewBag.Status = status;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageSize = pageSize;

            // Trả về danh sách sản phẩm phân trang
            var pagedList = sanPhams.ToPagedList(pageNumber, defaultPageSize);
            return View(pagedList);
        }





        // GET: Admin/SanPham/CreatePartial
        public IActionResult CreatePartial()
        {
            var danhMucList = _context.DanhMucs.ToList();
            ViewBag.DanhMucList = new SelectList(danhMucList, "MaDanhMuc", "TenDanhMuc");
            return PartialView("_CreateSanPhamPartial", new SanPham());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SanPham sanPham)
        {
            // Set NgayThem to today's date
            sanPham.NgayThem = DateOnly.FromDateTime(DateTime.Now);

            // Custom validation for required fields
            if (string.IsNullOrWhiteSpace(sanPham.TenSanPham))
                ModelState.AddModelError("TenSanPham", "Tên sản phẩm là bắt buộc.");

            if (sanPham.Gia <= 0)
                ModelState.AddModelError("Gia", "Giá phải lớn hơn 0.");

            if (string.IsNullOrWhiteSpace(sanPham.MaDanhMuc))
                ModelState.AddModelError("MaDanhMuc", "Danh mục là bắt buộc.");

            if (sanPham.SoLuong < 0)
                ModelState.AddModelError("SoLuong", "Số lượng không được nhỏ hơn 0.");

            if (!sanPham.Nsx.HasValue)
                ModelState.AddModelError("Nsx", "Ngày sản xuất (NSX) là bắt buộc.");

            if (!sanPham.Hsd.HasValue)
                ModelState.AddModelError("Hsd", "Hạn sử dụng (HSD) là bắt buộc.");

            if (sanPham.Nsx.HasValue && sanPham.Hsd.HasValue && sanPham.Hsd.Value < sanPham.Nsx.Value)
                ModelState.AddModelError("Hsd", "Hạn sử dụng (HSD) không được sớm hơn ngày sản xuất (NSX).");

            string randomMaSanPham;
            do
            {
                var random = new Random();
                int randomNumber = random.Next(1, 1000);
                randomMaSanPham = "SP" + randomNumber.ToString("D3");
            } while (SanPhamExists(randomMaSanPham));

            // Clear ModelState error for MaSanPham
            ModelState.Remove("MaSanPham");
            sanPham.MaSanPham = randomMaSanPham;

            // Validate ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                );
                return Json(new { success = false, errors });
            }

            try
            {
                
                sanPham.TrangThai = sanPham.SoLuong == 0 ? 0 : 1;

                // Handle image uploads
                if (sanPham.UploadImages != null && sanPham.UploadImages.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img/AnhSanPham");

                    foreach (var image in sanPham.UploadImages)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        sanPham.AnhSanPhams.Add(new AnhSanPham { UrlAnh = $"/img/AnhSanPham/{uniqueFileName}" });
                    }
                }

                _context.Add(sanPham);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Sản phẩm đã được thêm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thêm sản phẩm. Vui lòng thử lại!", error = ex.Message });
            }
        }


        // GET: Admin/SanPham/EditPartial
        public async Task<IActionResult> EditPartial(string maSanPham)
        {
            if (string.IsNullOrEmpty(maSanPham))
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.AnhSanPhams)
                .FirstOrDefaultAsync(s => s.MaSanPham == maSanPham);

            if (sanPham == null)
            {
                return NotFound();
            }

            var danhMucList = _context.DanhMucs.ToList();
            ViewBag.DanhMucList = new SelectList(danhMucList, "MaDanhMuc", "TenDanhMuc");

            return PartialView("_EditSanPhamPartial", sanPham);
        }

        // POST: Admin/SanPham/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(SanPham sanPham)
        {
            // Custom validation for required fields
            if (string.IsNullOrWhiteSpace(sanPham.TenSanPham))
                ModelState.AddModelError("TenSanPham", "Tên sản phẩm là bắt buộc.");

            if (sanPham.Gia <= 0)
                ModelState.AddModelError("Gia", "Giá phải lớn hơn 0.");

            if (string.IsNullOrWhiteSpace(sanPham.MaDanhMuc))
                ModelState.AddModelError("MaDanhMuc", "Danh mục là bắt buộc.");

            if (sanPham.SoLuong < 0)
                ModelState.AddModelError("SoLuong", "Số lượng không được nhỏ hơn 0.");

            if (!sanPham.Nsx.HasValue)
                ModelState.AddModelError("Nsx", "Ngày sản xuất (NSX) là bắt buộc.");

            if (!sanPham.Hsd.HasValue)
                ModelState.AddModelError("Hsd", "Hạn sử dụng (HSD) là bắt buộc.");

            if (sanPham.Nsx.HasValue && sanPham.Hsd.HasValue && sanPham.Hsd.Value < sanPham.Nsx.Value)
                ModelState.AddModelError("Hsd", "Hạn sử dụng (HSD) không được sớm hơn ngày sản xuất (NSX).");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                );
                return Json(new { success = false, errors }); // Return errors as JSON
            }

            try
            {
                var existingSanPham = await _context.SanPhams
                    .Include(s => s.AnhSanPhams)
                    .FirstOrDefaultAsync(s => s.MaSanPham == sanPham.MaSanPham);

                if (existingSanPham == null)
                {
                    return NotFound();
                }

                // Cập nhật các trường thông tin của sản phẩm
                existingSanPham.TenSanPham = sanPham.TenSanPham;
                existingSanPham.Gia = sanPham.Gia;
                existingSanPham.MoTa = sanPham.MoTa;
                existingSanPham.MaDanhMuc = sanPham.MaDanhMuc;
                existingSanPham.SoLuong = sanPham.SoLuong;
                existingSanPham.NgayThem = sanPham.NgayThem;
                existingSanPham.DonVi = sanPham.DonVi;
                existingSanPham.Nsx = sanPham.Nsx;
                existingSanPham.Hsd = sanPham.Hsd;
                existingSanPham.TrangThai = sanPham.SoLuong == 0 ? 0 : 1;

                // Kiểm tra nếu có chọn "ReplaceAllImages" và có tải lên ảnh mới
                // Xóa tất cả các ảnh cũ từ hệ thống tệp và cơ sở dữ liệu
                foreach (var image in existingSanPham.AnhSanPhams)
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.UrlAnh.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath); // Xóa ảnh cũ từ hệ thống tệp
                    }
                }

                _context.AnhSanPhams.RemoveRange(existingSanPham.AnhSanPhams); // Xóa ảnh cũ từ cơ sở dữ liệu
                await _context.SaveChangesAsync(); // Lưu ngay để đảm bảo ảnh cũ bị xóa

                // Bước 3: Thêm các ảnh mới vào cơ sở dữ liệu và hệ thống tệp
                if (sanPham.UploadImages != null && sanPham.UploadImages.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img/AnhSanPham");

                    foreach (var image in sanPham.UploadImages)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        existingSanPham.AnhSanPhams.Add(new AnhSanPham { UrlAnh = $"/img/AnhSanPham/{uniqueFileName}" });
                    }
                }

                // Bước 4: Cập nhật sản phẩm trong cơ sở dữ liệu
                _context.Update(existingSanPham);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Sản phẩm đã được cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errors = new List<string> { ex.Message } });
            }
        }


        // POST: Admin/SanPham/Delete/5
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sanPham = await _context.SanPhams
                .Include(s => s.AnhSanPhams)
                .FirstOrDefaultAsync(s => s.MaSanPham == id);

            if (sanPham != null)
            {
                if (sanPham.AnhSanPhams != null)
                {
                    foreach (var image in sanPham.AnhSanPhams)
                    {
                        string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.UrlAnh.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

                _context.SanPhams.Remove(sanPham);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }


        // GET: Admin/SanPham/DetailsPartial
        public async Task<IActionResult> DetailsPartial(string maSanPham)
        {
            if (string.IsNullOrEmpty(maSanPham))
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.AnhSanPhams)
                .Include(s => s.MaDanhMucNavigation)
                .FirstOrDefaultAsync(s => s.MaSanPham == maSanPham);

            if (sanPham == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsSanPhamPartial", sanPham);
        }



        private bool SanPhamExists(string id)
        {
            return _context.SanPhams.Any(e => e.MaSanPham == id);
        }
    }
}