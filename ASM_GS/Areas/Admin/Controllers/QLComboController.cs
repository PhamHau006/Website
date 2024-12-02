using ASM_GS.Controllers;
using ASM_GS.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QLComboController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QLComboController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Index - Hiển thị danh sách Combo
        public IActionResult Index(string searchTerm, int? page, int pageSize = 5, int? trangThai = null, string sortBy = "TenCombo")
        {
            if (HttpContext.Session.GetString("StaffAccount") == null)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
                ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            }
            var combos = _context.Combos
                                 .Include(c => c.ChiTietCombos)
                                 .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                combos = combos.Where(c => c.TenCombo.Contains(searchTerm) || c.MaCombo.Contains(searchTerm));
            }

            if (trangThai.HasValue)
            {
                combos = combos.Where(c => c.TrangThai == trangThai.Value);
            }

            switch (sortBy)
            {
                case "Gia_Desc":
                    combos = combos.OrderByDescending(c => c.Gia); // Sắp xếp theo giá từ cao đến thấp
                    break;
                case "Gia":
                    combos = combos.OrderBy(c => c.Gia); // Sắp xếp theo giá từ thấp đến cao
                    break;
                case "TenCombo_Desc":
                    combos = combos.OrderByDescending(c => c.TenCombo); // Sắp xếp theo tên từ Z-A
                    break;
                case "TenCombo":
                default:
                    combos = combos.OrderBy(c => c.TenCombo); // Sắp xếp theo tên từ A-Z
                    break;
            }


            var pageNumber = page ?? 1;
            var pagedCombos = combos.ToPagedList(pageNumber, pageSize);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.PageSize = pageSize;
            ViewBag.TrangThai = trangThai;
            ViewBag.SortBy = sortBy;

            return View(pagedCombos);
        }

        // Hiển thị form tạo mới Combo
        public IActionResult Create()
        {
            ViewBag.SanPhams = _context.SanPhams.ToList();
            return PartialView("_ComboCreatePartial", new Combo());
        }
        [HttpPost]
        public async Task<IActionResult> Create(Combo combo, List<string>? selectedSanPhams)
        {
            Random random = new Random();
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string maCombo = "CB" + string.Concat(Enumerable.Range(0, 6).Select(_ => characters[random.Next(characters.Length)]));
            combo.MaCombo = maCombo;

            // Xử lý lưu ảnh
            if (combo.anhcombo != null && combo.anhcombo.Length > 0)
            {
                var folderPath = Path.Combine("wwwroot/img/AnhCombo");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, combo.anhcombo.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await combo.anhcombo.CopyToAsync(stream);
                }

                combo.Anh = combo.anhcombo.FileName;
            }

            // Thêm Combo vào cơ sở dữ liệu
            _context.Combos.Add(combo);
            await _context.SaveChangesAsync();

            // Thêm ChiTietCombos nếu có
            if (selectedSanPhams != null)
            {
                foreach (var item in selectedSanPhams)
                {
                    var parts = item.Split(':');
                    var maSanPham = parts[0];
                    var soLuong = int.Parse(parts[1]);

                    var chiTietCombo = new ChiTietCombo
                    {
                        MaCombo = combo.MaCombo,
                        MaSanPham = maSanPham,
                        SoLuong = soLuong
                    };
                    _context.ChiTietCombos.Add(chiTietCombo);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm combo thành công!";
            return RedirectToAction("Index");
        }


        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = _context.Combos
                                .Include(c => c.ChiTietCombos)
                                .FirstOrDefault(c => c.MaCombo == id);
            if (combo == null)
            {
                return NotFound();
            }

            foreach (var chiTiet in combo.ChiTietCombos)
            {
                _context.ChiTietCombos.Remove(chiTiet);
            }

            _context.Combos.Remove(combo);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Xóa combo thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = _context.Combos
                                .Include(c => c.ChiTietCombos)
                                    .ThenInclude(ct => ct.MaSanPhamNavigation)
                                        .ThenInclude(sp => sp.AnhSanPhams)
                                .FirstOrDefault(c => c.MaCombo == id);

            if (combo == null)
            {
                return NotFound();
            }

            return PartialView("_ComboDetailsPartial", combo);
        }

        public IActionResult Edit(string id)
        {
            if (id == null) return NotFound();

            var combo = _context.Combos
                                .Include(c => c.ChiTietCombos)
                                .FirstOrDefault(c => c.MaCombo == id);
            if (combo == null) return NotFound();

            ViewBag.SanPhams = _context.SanPhams.ToList();
            return PartialView("_ComboEditPartial", combo); // Trả về PartialView để tải vào modal
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Combo combo, List<string> selectedSanPhams)
        {

            var existingCombo = _context.Combos.Include(c => c.ChiTietCombos).FirstOrDefault(c => c.MaCombo == combo.MaCombo);
            if (existingCombo == null) return NotFound();

            existingCombo.TenCombo = combo.TenCombo;
            existingCombo.MoTa = combo.MoTa;
            existingCombo.Gia = combo.Gia;
            existingCombo.TrangThai = combo.TrangThai;
            // Xử lý ảnh nếu có tệp ảnh mới được tải lên
            if (combo.anhcombo != null && combo.anhcombo.Length > 0)
            {
                var folderPath = Path.Combine("wwwroot/img/AnhCombo");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = Path.GetFileName(combo.anhcombo.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                // Lưu tệp ảnh mới
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await combo.anhcombo.CopyToAsync(stream);
                }

                // Cập nhật đường dẫn ảnh
                existingCombo.Anh = fileName;
            }

            _context.ChiTietCombos.RemoveRange(existingCombo.ChiTietCombos);

            foreach (var item in selectedSanPhams)
            {
                var parts = item.Split(':');
                var maSanPham = parts[0];
                var soLuong = int.Parse(parts[1]);

                var chiTietCombo = new ChiTietCombo
                {
                    MaCombo = combo.MaCombo,
                    MaSanPham = maSanPham,
                    SoLuong = soLuong
                };
                _context.ChiTietCombos.Add(chiTietCombo);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật combo thành công!";
            return RedirectToAction("Index");
        }
    }
}
