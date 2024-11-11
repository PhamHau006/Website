using ASM_GS.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ASM_GS.Views.Components
{
    public class ComboViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public ComboViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var combos = _context.Combos.ToList();
            return View(combos);
        }
       
    }
}
