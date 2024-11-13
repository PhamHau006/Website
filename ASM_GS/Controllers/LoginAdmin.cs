using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;

namespace ASM_GS.Controllers
{
    public class LoginAdmin : Controller
    {
		private readonly ApplicationDbContext _context;

		public LoginAdmin(ApplicationDbContext context)
		{
			_context = context;
		}

		public IActionResult Index()
        {
			if (HttpContext.Session.GetString("StaffAccount") != null)
				return RedirectToAction("Index", "Home", new {area = "Admin"});
			if (HttpContext.Session.GetString("RedirectUrl") != null)
			{
				ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
			}
			return View();
        }
		[HttpPost]
		public IActionResult ValidateLogin([FromBody] LoginModelView model)
		{
			if (!ModelState.IsValid)
			{
				var errors = new Dictionary<string, string>();
				if (ModelState.ContainsKey(nameof(model.EmailOrUsername)))
				{
					var emailErrors = ModelState[nameof(model.EmailOrUsername)].Errors;
					if (emailErrors.Any())
						errors["EmailOrUsername"] = emailErrors.First().ErrorMessage;
				}

				if (ModelState.ContainsKey(nameof(model.Password)))
				{
					var passwordErrors = ModelState[nameof(model.Password)].Errors;
					if (passwordErrors.Any())
						errors["Password"] = passwordErrors.First().ErrorMessage;
				}

				return Json(new { success = false, errors });
			}

			var user = _context.TaiKhoans
				.FirstOrDefault(u => ((u.Email.Trim() == model.EmailOrUsername.Trim() || u.TenTaiKhoan.Trim() == model.EmailOrUsername.Trim())
									 && u.MatKhau.Trim() == model.Password.Trim() && (u.TinhTrang == 1 || u.TinhTrang == 2) && (u.VaiTro=="Admin" || u.VaiTro=="Staff")));

			if (user == null)
			{
				return Json(new { success = false, message = "Sai email, tên tài khoản hoặc mật khẩu" });
			}
			HttpContext.Session.SetString("LoginStaffRoute", "true");
			HttpContext.Session.SetString("StaffAccount", user.MaTaiKhoan);
			var Staff = _context.NhanViens.FirstOrDefault(c => (c.MaNhanVien == user.MaNhanVien));
			HttpContext.Session.SetString("Staff", Staff.TenNhanVien);
			return Json(new { success = true, message = "Đăng nhập thành công" });
		}
    }
}
