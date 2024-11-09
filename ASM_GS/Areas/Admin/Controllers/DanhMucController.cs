﻿using ASM_GS.Controllers;
using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
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
        public async Task<IActionResult> Index(string searchName, int? status, string sortOrder, int? page, int? pageSize)
        {
            int defaultPageSize = pageSize ?? 5;
            int pageNumber = page ?? 1;

            var danhMucs = _context.DanhMucs.AsQueryable();

            // Apply filters and sorting
            if (!string.IsNullOrEmpty(searchName))
            {
                danhMucs = danhMucs.Where(d => d.TenDanhMuc.Contains(searchName));
            }
            if (status.HasValue)
            {
                danhMucs = danhMucs.Where(d => d.TrangThai == status.Value);
            }
            danhMucs = sortOrder switch
            {
                "name_desc" => danhMucs.OrderByDescending(d => d.TenDanhMuc),
                _ => danhMucs.OrderBy(d => d.TenDanhMuc)
            };

            

            

            // Normal request (full page load)
            ViewBag.CurrentPageSize = defaultPageSize;
            ViewBag.PageSize = defaultPageSize;
            var pagedList = danhMucs.ToPagedList(pageNumber, defaultPageSize);
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
            if (ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .ToDictionary(
                        ms => ms.Key,
                        ms => ms.Value.Errors.First().ErrorMessage
                    );

                return Json(new { success = false, errors });
            }

            string randomMaDanhMuc;
            do
            {
                Random random = new Random();
                int randomNumber = random.Next(1, 1000);
                randomMaDanhMuc = "SP" + randomNumber.ToString("D3");
            } while (DanhMucExists(randomMaDanhMuc));

            danhMuc.MaDanhMuc = randomMaDanhMuc;

            danhMuc.TrangThai = danhMuc.TrangThai == 0 ? 0 : 1;

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


        private bool DanhMucExists(string id)
        {
            return _context.DanhMucs.Any(e => e.MaDanhMuc == id);
        }
    }
}
 