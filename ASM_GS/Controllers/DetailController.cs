using Microsoft.AspNetCore.Mvc;
using ASM_GS.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ASM_GS.Controllers
{
    public class DetailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DetailController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string id)
        {
            var sanPham = _context.SanPhams
                                  .Include(sp => sp.AnhSanPhams)
                                  .FirstOrDefault(sp => sp.MaSanPham == id);
            if (sanPham == null)
            {
                return NotFound();
            }
            return View(sanPham);
        }
    }
}
