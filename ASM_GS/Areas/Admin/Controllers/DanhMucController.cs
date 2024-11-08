using ASM_GS.Controllers;
using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList.Extensions;

namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhMucController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DanhMucController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/DanhMuc
        public async Task<IActionResult> Index(string searchName, string sortOrder, int? page, int? pageSize)
        {
            int defaultPageSize = pageSize ?? 5;
            int pageNumber = page ?? 1;

            var danhMucs = _context.DanhMucs.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                danhMucs = danhMucs.Where(d => d.TenDanhMuc.Contains(searchName));
            }

            danhMucs = sortOrder switch
            {
                "name_desc" => danhMucs.OrderByDescending(d => d.TenDanhMuc),
                "status" => danhMucs.OrderBy(d => d.TrangThai),
                _ => danhMucs.OrderBy(d => d.TenDanhMuc),
            };

            var pagedList = danhMucs.ToPagedList(pageNumber, defaultPageSize);

            // Lưu giá trị tìm kiếm và sắp xếp vào ViewBag để sử dụng trong view
            ViewBag.SearchName = searchName;
            ViewBag.SortOrder = sortOrder;

            return View(pagedList);
        }



        // GET: Admin/DanhMuc/CreatePartial
        public IActionResult CreatePartial()
        {
            return PartialView("_CreateDanhMucPartial", new DanhMuc());
        }

        [HttpPost]
        public async Task<IActionResult> Create(DanhMuc danhMuc)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors });
            }

            _context.Add(danhMuc);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Danh mục đã được thêm thành công!" });
        }

        // GET: Admin/DanhMuc/EditPartial
        public async Task<IActionResult> EditPartial(string maDanhMuc)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(maDanhMuc);
            if (danhMuc == null) return NotFound();

            return PartialView("_EditDanhMucPartial", danhMuc);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DanhMuc danhMuc)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors });
            }

            _context.Update(danhMuc);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Danh mục đã được cập nhật thành công!" });
        }

        // POST: Admin/DanhMuc/Delete/5
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null) return Json(new { success = false });

            _context.DanhMucs.Remove(danhMuc);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Danh mục đã được xóa thành công!" });
        }
    }
}
