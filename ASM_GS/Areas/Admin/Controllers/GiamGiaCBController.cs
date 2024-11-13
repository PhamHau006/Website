using ASM_GS.Controllers;
using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using X.PagedList;
using X.PagedList.Extensions;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Extensions;
namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GiamGiaCBController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GiamGiaCBController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchTerm, int? trangThai, string sortOrder, int? page, int pageSize = 10)
        {
            if (HttpContext.Session.GetString("StaffAccount") == null)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
				ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
			}
            // Truy vấn tất cả các giảm giá, bao gồm các mã nhập chi tiết
            var discounts = _context.GiamGia
                .Include(d => d.MaNhapGiamGias) // Bao gồm mã nhập chi tiết nếu cần
                .AsQueryable();

            // Lọc theo từ khóa tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchTerm))
            {
                discounts = discounts.Where(d => d.TenGiamGia.Contains(searchTerm));
            }

            // Lọc theo trạng thái nếu được chỉ định
            if (trangThai.HasValue)
            {
                discounts = discounts.Where(d => d.TrangThai == trangThai.Value);
            }

            // Sắp xếp theo yêu cầu
            switch (sortOrder)
            {
                case "newest":
                    discounts = discounts.OrderByDescending(d => d.NgayBatDau); // Mới nhất trước
                    break;
                case "oldest":
                    discounts = discounts.OrderBy(d => d.NgayBatDau); // Cũ nhất trước
                    break;
                default:
                    discounts = discounts.OrderBy(d => d.TenGiamGia); // Mặc định sắp xếp theo tên từ A-Z
                    break;
            }

            // Cấu hình phân trang
            int pageNumber = page ?? 1;
            var pagedList = discounts.ToPagedList(pageNumber, pageSize);

            // Thiết lập ViewBag cho phân trang và bộ lọc
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TrangThai = trangThai;
            ViewBag.PageSize = pageSize;
            ViewBag.SortOrder = sortOrder;

            return View(pagedList);
        }

        public IActionResult Create()
        {
            return PartialView("_DiscountCreatePartial", new GiamGia()); // Trả về partial view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GiamGia giamGia)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_DiscountCreatePartial", giamGia); // Trả về partial view nếu có lỗi
            }

            Random random = new Random();
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string maGiamGia = "CP" + string.Concat(Enumerable.Range(0, 6).Select(_ => characters[random.Next(characters.Length)]));
            giamGia.MaGiamGia = maGiamGia;


            // Thêm giảm giá vào cơ sở dữ liệu
            _context.Add(giamGia);
            await _context.SaveChangesAsync();

            // Tạo mã nhập giảm giá theo số lượng tối đa
            for (int i = 0; i < giamGia.SoLuongMaNhapToiDa; i++)
            {
                // Tạo mã nhập ngẫu nhiên (8 ký tự viết hoa)
                string randomCode = CodeGenerator.GenerateRandomCode();
                var maNhapGiamGia = new MaNhapGiamGia
                {
                    MaNhap = randomCode,
                    MaGiamGia = giamGia.MaGiamGia
                };
                _context.MaNhapGiamGia.Add(maNhapGiamGia);
            }

            // Lưu các mã nhập giảm giá mới vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            TempData["successMessage"] = "Tạo giảm giá thành công!";
            return RedirectToAction("Index");
        }


        public IActionResult Details(string id)
        {
            // Truy vấn giảm giá và bao gồm danh sách mã nhập
            var discount = _context.GiamGia
                .Include(g => g.MaNhapGiamGias)
                .FirstOrDefault(d => d.MaGiamGia == id);

            if (discount == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy giảm giá
            }

            return PartialView("_DiscountDetailsPartial", discount);
        }

        public IActionResult Edit(string id)
        {
            var discount = _context.GiamGia.FirstOrDefault(d => d.MaGiamGia == id);
            if (discount == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy
            }
            return PartialView("_DiscountEditPartial", discount); // Trả về partial view để chỉnh sửa
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GiamGia giamGia)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_DiscountEditPartial", giamGia); // Trả về partial view với lỗi xác thực
            }

            // Lấy thông tin giảm giá hiện tại từ cơ sở dữ liệu, bao gồm các mã nhập chi tiết
            var existingDiscount = _context.GiamGia
                .Include(g => g.MaNhapGiamGias)
                .FirstOrDefault(g => g.MaGiamGia == giamGia.MaGiamGia);

            if (existingDiscount == null)
            {
                return NotFound("Mã giảm giá không tồn tại.");
            }

            // Cập nhật thông tin giảm giá
            existingDiscount.TenGiamGia = giamGia.TenGiamGia;
            existingDiscount.GiaTri = giamGia.GiaTri;
            existingDiscount.NgayBatDau = giamGia.NgayBatDau;
            existingDiscount.NgayKetThuc = giamGia.NgayKetThuc;
            existingDiscount.TrangThai = giamGia.TrangThai;

            // Điều chỉnh số lượng mã nhập dựa vào SoLuongMaNhapToiDa mới
            int currentCount = existingDiscount.MaNhapGiamGias.Count;
            if (currentCount < giamGia.SoLuongMaNhapToiDa)
            {
                // Thêm mã nhập mới để phù hợp với số lượng tăng
                int codesToGenerate = giamGia.SoLuongMaNhapToiDa - currentCount;
                for (int i = 0; i < codesToGenerate; i++)
                {
                    string randomCode = CodeGenerator.GenerateRandomCode(); // Tạo mã ngẫu nhiên mới
                    var maNhapGiamGia = new MaNhapGiamGia
                    {
                        MaNhap = randomCode,
                        MaGiamGia = giamGia.MaGiamGia
                    };
                    _context.MaNhapGiamGia.Add(maNhapGiamGia);
                }
            }
            else if (currentCount > giamGia.SoLuongMaNhapToiDa)
            {
                // Xóa các mã dư thừa để phù hợp với số lượng giảm
                int codesToRemove = currentCount - giamGia.SoLuongMaNhapToiDa;
                var codesToBeRemoved = existingDiscount.MaNhapGiamGias
                    .OrderBy(c => c.Id) // Giả sử bạn muốn xóa các mã cũ nhất
                    .Take(codesToRemove)
                    .ToList();

                _context.MaNhapGiamGias.RemoveRange(codesToBeRemoved);
            }

            existingDiscount.SoLuongMaNhapToiDa = giamGia.SoLuongMaNhapToiDa; // Cập nhật số lượng mã tối đa

            // Lưu các thay đổi vào cơ sở dữ liệu
            _context.Update(existingDiscount);
            await _context.SaveChangesAsync();
            TempData["successMessage"] = "Cập nhật giảm giá thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var giamgia = _context.GiamGia.FirstOrDefault(g => g.MaGiamGia == id);
      
            _context.GiamGia.Remove(giamgia);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Xóa giảm giá thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult UseCode(string maNhap)
        {
            // Tìm mã nhập trong cơ sở dữ liệu
            var code = _context.MaNhapGiamGias.FirstOrDefault(c => c.MaNhap == maNhap);

            // Kiểm tra xem mã nhập có tồn tại và chưa được sử dụng
            if (code != null && !code.IsUsed)
            {
                code.IsUsed = true; // Đánh dấu mã nhập là đã sử dụng
                _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                TempData["successMessage"] = "Mã nhập đã được sử dụng!";
            }
            else
            {
                TempData["errorMessage"] = "Mã nhập không hợp lệ hoặc đã được sử dụng.";
            }

            // Chuyển hướng trở lại trang chi tiết giảm giá
            return RedirectToAction("Details", new { id = code?.MaGiamGia });
        }

    }
}

