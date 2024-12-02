using ASM_GS.Controllers;
using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;  
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text.RegularExpressions;
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
        public async Task<IActionResult> Index(string searchName, int? status, string sortOrder, int? page, int? pageSize)
        {
            if (HttpContext.Session.GetString("StaffAccount") == null)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
                ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            }

            int defaultPageSize = pageSize ?? 5;
            int pageNumber = page ?? 1;

            var danhMucs = _context.DanhMucs.AsQueryable();

            // Áp dụng bộ lọc theo tên
            if (!string.IsNullOrEmpty(searchName))
            {
                danhMucs = danhMucs.Where(d => d.TenDanhMuc.Contains(searchName));
            }

            // Áp dụng bộ lọc theo trạng thái (modified)
            if (status.HasValue)
            {
                danhMucs = danhMucs.Where(d => d.TrangThai == status.Value);
            }
            else if (Request.Query.ContainsKey("status") && int.TryParse(Request.Query["status"], out int parsedStatus))
            {
                // Get status from query string if not provided directly
                status = parsedStatus;
                danhMucs = danhMucs.Where(d => d.TrangThai == status.Value);
            }

            // Áp dụng sắp xếp (modified)
            if (!string.IsNullOrEmpty(sortOrder))
            {
                danhMucs = sortOrder switch
                {
                    "name_desc" => danhMucs.OrderByDescending(d => d.TenDanhMuc),
                    _ => danhMucs.OrderBy(d => d.TenDanhMuc)
                };
            }
            else if (Request.Query.ContainsKey("sortOrder"))
            {
                // Get sortOrder from query string if not provided directly
                sortOrder = Request.Query["sortOrder"];
                danhMucs = sortOrder switch
                {
                    "name_desc" => danhMucs.OrderByDescending(d => d.TenDanhMuc),
                    _ => danhMucs.OrderBy(d => d.TenDanhMuc)
                };
            }

            // Lưu trữ thông tin cho ViewBag
            ViewBag.CurrentPageSize = defaultPageSize;
            ViewBag.PageSize = defaultPageSize;
            ViewBag.CurrentSearchName = searchName;  // Lưu tên tìm kiếm hiện tại
            ViewBag.CurrentStatus = status;  // Lưu trạng thái hiện tại
            ViewBag.CurrentSortOrder = sortOrder;  // Lưu thứ tự sắp xếp hiện tại

            // Sử dụng ToListAsync và sau đó gọi ToPagedList
            var list = await danhMucs.ToListAsync();
            var pagedList = list.ToPagedList(pageNumber, defaultPageSize);

            return View(pagedList);
        }




        // GET: Admin/DanhMuc/CreatePartial
        public IActionResult CreatePartial()
        {
            return PartialView("_CreateDanhMucPartial", new DanhMuc());
        }

        // POST: Admin/DanhMuc/Create
        [HttpPost]
        public async Task<IActionResult> Create(DanhMuc danhMuc)
        {
            

            // Check if category name exceeds 50 characters
            if (!string.IsNullOrEmpty(danhMuc.TenDanhMuc) && danhMuc.TenDanhMuc.Length > 50)
            {
                ModelState.AddModelError("TenDanhMuc", "Tên danh mục không được quá 50 ký tự.");
            }

            // Check if the category name already exists
            if (_context.DanhMucs.Any(dm => dm.TenDanhMuc == danhMuc.TenDanhMuc))
            {
                ModelState.AddModelError("TenDanhMuc", "Tên danh mục đã tồn tại.");
            }
            // Check if TenDanhMuc contains invalid characters
            if (!string.IsNullOrEmpty(danhMuc.TenDanhMuc) && !IsValidTenDanhMuc(danhMuc.TenDanhMuc))
            {
                ModelState.AddModelError("TenDanhMuc", "Tên danh mục không được chứa ký tự đặc biệt.");
            }

            // Generate a random code for the category if no errors
            string randomMaDanhMuc;
            do
            {
                Random random = new Random();
                int randomNumber = random.Next(1, 1000);
                randomMaDanhMuc = "DM" + randomNumber.ToString("D3");
            } while (DanhMucExists(randomMaDanhMuc));
            ModelState.Remove("MaDanhMuc");
            // Assign the generated code to the category
            danhMuc.MaDanhMuc = randomMaDanhMuc;

            // If there are validation errors, return them
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                );
                return Json(new { success = false, errors });
            }

            

            // Set the status of the category (defaulting to 1 if not 0)
            danhMuc.TrangThai = danhMuc.TrangThai == 0 ? 0 : 1;

            // Add the category to the context and save changes
            _context.Add(danhMuc);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Danh mục đã được thêm thành công!" });



            
        }




        // GET: Admin/DanhMuc/EditPartial
        public async Task<IActionResult> EditPartial(string maDanhMuc)
        {
            if (string.IsNullOrEmpty(maDanhMuc))
            {
                return NotFound();
            }

            var danhMuc = await _context.DanhMucs.FindAsync(maDanhMuc);

            if (danhMuc == null)
            {
                return NotFound();
            }

            return PartialView("_EditDanhMucPartial", danhMuc);
        }

        // POST: Admin/DanhMuc/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(DanhMuc danhMuc)
        {
            

            // Check if category name exceeds 50 characters
            if (!string.IsNullOrEmpty(danhMuc.TenDanhMuc) && danhMuc.TenDanhMuc.Length > 50)
            {
                ModelState.AddModelError("TenDanhMuc", "Tên danh mục không được quá 50 ký tự.");
            }

            // Check if the category name already exists (except for the current record)
            if (_context.DanhMucs.Any(dm => dm.TenDanhMuc == danhMuc.TenDanhMuc && dm.MaDanhMuc != danhMuc.MaDanhMuc))
            {
                ModelState.AddModelError("TenDanhMuc", "Tên danh mục đã tồn tại.");
            }

            // Check if TenDanhMuc contains invalid characters
            if (!string.IsNullOrEmpty(danhMuc.TenDanhMuc) && !IsValidTenDanhMuc(danhMuc.TenDanhMuc))
            {
                ModelState.AddModelError("TenDanhMuc", "Tên danh mục không được chứa ký tự đặc biệt.");
            }

            // If there are validation errors, return them
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .ToDictionary(
                        ms => ms.Key,
                        ms => ms.Value.Errors.First().ErrorMessage
                    );

                return Json(new { success = false, errors });
            }

            try
            {
                // Update the category
                _context.Update(danhMuc);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Danh mục đã được cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errors = new List<string> { ex.Message } });
            }
        }



        // POST: Admin/DanhMuc/Delete/5
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);

            if (danhMuc != null)
            {
                _context.DanhMucs.Remove(danhMuc);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }


        // Helper method to check if MaDanhMuc already exists in the database
        private bool DanhMucExists(string maDanhMuc)
        {
            return _context.DanhMucs.Any(dm => dm.MaDanhMuc == maDanhMuc);
        }

        private bool IsValidTenDanhMuc(string tenDanhMuc)
        {
            // Regex pattern to allow letters, numbers, spaces, and punctuation (but no special characters)
            string pattern = @"^[A-Za-z0-9\s.,;!?()-]*$";
            return Regex.IsMatch(tenDanhMuc, pattern);
        }


        

    }
}
