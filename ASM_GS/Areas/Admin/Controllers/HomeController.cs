using Microsoft.AspNetCore.Mvc;

namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

}
