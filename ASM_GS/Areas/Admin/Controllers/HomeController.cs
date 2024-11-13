using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("StaffAccount") == null)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
				ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
			}
            ViewData["TenNhanVien"] = HttpContext.Session.GetString("Staff");
            ViewData["LoginStaffRoute"] = HttpContext.Session.GetString("LoginStaffRoute");
            return View();
		}
        [HttpPost]
        public IActionResult RemoveRoutedFromLoginSession()
        {
            HttpContext.Session.Remove("RedirectUrl");
            ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            return Ok();
        }
    }
}
