using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASM_GS.Controllers
{
    public class DetailComboController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DetailComboController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /DetailCombo/Index/id
        public IActionResult Index(string id)
        {
            var combo = _context.Combos
                                .Include(c => c.ChiTietCombos)
                                    .ThenInclude(ct => ct.MaSanPhamNavigation)
                                        .ThenInclude(sp => sp.AnhSanPhams)
                                .FirstOrDefault(c => c.MaCombo == id);


            if (combo == null)
            {
                return NotFound();
            }
            return View(combo);
        }
    }
}
