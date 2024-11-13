//Mã Tài Khoản đăng nhập được lưu trong Session là UserAccount
//Mã Khách hàng đăng nhập được lưu trong Session là User

using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Security.Principal;
using static System.Net.WebRequestMethods;
namespace ASM_GS.Controllers
{
    public class LoginAndSignUp : Controller
    {
        private readonly ApplicationDbContext _context;
        private string MaTaiKhoanDuocTao;
        public LoginAndSignUp(ApplicationDbContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }
        public class GoogleUserModel
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Picture { get; set; }
        }
        private readonly IConfiguration _configuration;
        private string GenerateNewAccountId()
        {
            var lastAccount = _context.TaiKhoans
                                      .OrderByDescending(t => t.MaTaiKhoan)
                                      .FirstOrDefault();

            int newIdNumber = 0;

            if (lastAccount != null && lastAccount.MaTaiKhoan.StartsWith("TK"))
            {

                string lastIdNumberPart = lastAccount.MaTaiKhoan.Substring(2);

                if (int.TryParse(lastIdNumberPart, out int lastIdNumber))
                {
                    newIdNumber = lastIdNumber + 1;
                }
            }


            return "TK" + newIdNumber.ToString("D3");
            
        }
        public IActionResult Index()
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            ViewData["GoogleClientId"] = clientId;
            HttpContext.Session.GetString("RoutedPage");
            if (HttpContext.Session.GetString("UserAccount") != null )
            {
                if (HttpContext.Session.GetString("RoutedPage") == null)
                    return RedirectToAction("Index", "Home");
                else
                    return Redirect(HttpContext.Session.GetString("RoutedPage"));
			};
            return View();
        }
        [HttpPost]
        public IActionResult Index(LoginModelView model)
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
                .FirstOrDefault(u => (u.Email.Trim() == model.EmailOrUsername.Trim() || u.TenTaiKhoan.Trim() == model.EmailOrUsername.Trim())
                                     && u.MatKhau.Trim() == model.Password.Trim() && (u.TinhTrang == 1 || u.TinhTrang == 2));
                                   

            if (user == null)
            {
                return Json(new { success = false, message = "Sai email, tên tài khoản hoặc mật khẩu" });
            }
            var Kh = _context.KhachHangs.FirstOrDefault(u => (u.MaKhachHang == user.MaKhachHang));
            HttpContext.Session.SetString("LoginRoute", "true");
            HttpContext.Session.SetString("UserAccount", user.MaTaiKhoan);
            if (Kh != null)
            {
                HttpContext.Session.SetString("User", Kh.MaKhachHang);
                HttpContext.Session.SetString("UserName", Kh.TenKhachHang);
            }
            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }
        [HttpPost]
        public IActionResult SignUp(RegisterModelView model)
        {
            if (!ModelState.IsValid)
            {
                var errors = new Dictionary<string, string>();

                if (ModelState.ContainsKey(nameof(model.TenTaiKhoan)))
                {
                    var usernameErrors = ModelState[nameof(model.TenTaiKhoan)].Errors;
                    if (usernameErrors.Any())
                        errors["TenTaiKhoan"] = usernameErrors.First().ErrorMessage;
                }

                if (ModelState.ContainsKey(nameof(model.Email)))
                {
                    var emailErrors = ModelState[nameof(model.Email)].Errors;
                    if (emailErrors.Any())
                        errors["Email"] = emailErrors.First().ErrorMessage;
                }

                if (ModelState.ContainsKey(nameof(model.Password)))
                {
                    var passwordErrors = ModelState[nameof(model.Password)].Errors;
                    if (passwordErrors.Any())
                        errors["Password"] = passwordErrors.First().ErrorMessage;
                }

                if (ModelState.ContainsKey(nameof(model.ConfirmPassword)))
                {
                    var confirmPasswordErrors = ModelState[nameof(model.ConfirmPassword)].Errors;
                    if (confirmPasswordErrors.Any())
                        errors["ConfirmPassword"] = confirmPasswordErrors.First().ErrorMessage;
                }

                return Json(new { success = false, errors });
            }


            if (_context.TaiKhoans.Any(u => u.TenTaiKhoan == model.TenTaiKhoan || u.Email == model.Email))
            {
                return Json(new { success = false, message = "Tên tài khoản hoặc email đã tồn tại" });
            }
            MaTaiKhoanDuocTao = GenerateNewAccountId();
            var newUser = new TaiKhoan
            {
                MaTaiKhoan = MaTaiKhoanDuocTao,
                TenTaiKhoan = model.TenTaiKhoan,
                Email = model.Email,
                MatKhau = model.Password,
                VaiTro = "Customer",
                TinhTrang = 1,
            };
            _context.TaiKhoans.Add(newUser);
            _context.SaveChanges();
            HttpContext.Session.SetString("SignUpAccount", newUser.MaTaiKhoan);
            return Json(new { success = true, message = "Đăng ký thành công!" });
        }
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(KhachHang customer, IFormFile Anh)
        {
            var errors = new Dictionary<string, string>();
            var KHList = _context.KhachHangs.ToList();
            try
            {
                if (string.IsNullOrEmpty(customer.TenKhachHang))
                {
                    errors.Add("TenKhachHang", "Tên khách hàng không được để trống.");
                }

                if (string.IsNullOrEmpty(customer.SoDienThoai))
                {
                    errors.Add("SoDienThoai", "Số điện thoại không được để trống.");
                }
                else if (!Regex.IsMatch(customer.SoDienThoai, @"^\d{10,11}$"))
                {
                    errors.Add("SoDienThoai", "Số điện thoại không hợp lệ.");
                }

                if (string.IsNullOrEmpty(customer.DiaChi))
                {
                    errors.Add("DiaChi", "Địa chỉ không được để trống.");
                }

                if (string.IsNullOrEmpty(customer.Cccd))
                {
                    errors.Add("Cccd", "Căn cước công dân không được để trống.");
                }
                else if (!Regex.IsMatch(customer.Cccd, @"^0\d{11}$"))
                {
                    errors.Add("Cccd", "Căn cước công dân phải đủ 12 ký tự và bắt đầu bằng số 0.");
                }
                if (string.IsNullOrEmpty(customer.NgaySinh?.ToString()))
                {
                    errors.Add("NgaySinh", "Ngày sinh không được để trống.");
                }
                else if (customer.NgaySinh > DateOnly.FromDateTime(DateTime.Now.AddYears(-15)))
                {
                    errors.Add("NgaySinh", "Khách hàng phải đủ 15 tuổi.");
                }
                if (errors.Any())
                {
                    return Json(new { success = false, errors = errors });
                }

                if (Anh != null && Anh.Length > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(Anh.FileName);
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Avatar", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await Anh.CopyToAsync(stream);
                    }

                    customer.HinhAnh = "/Avatar/" + fileName;
                }
                else
                {
                    errors.Add("Anh", "Vui lòng tải lên hình ảnh hợp lệ.");
                    return Json(new { success = false, errors = errors });
                }

                // Generate a unique, sequential MaKhachHang
                var lastCustomer = await _context.KhachHangs
                                            .OrderByDescending(kh => kh.MaKhachHang)
                                           .FirstOrDefaultAsync();


                int nextId = 1;
                if (lastCustomer != null)
                {
                    string lastIdStr = lastCustomer.MaKhachHang.Substring(2); 
                    if (int.TryParse(lastIdStr, out int lastId))
                    {
                        nextId = lastId + 1;
                    }
                }
                customer.MaKhachHang = "KH" + nextId.ToString("D3"); 

                customer.TrangThai = 1;

                customer.NgayDangKy = DateOnly.FromDateTime(DateTime.Now);


                _context.KhachHangs.Add(customer);
                await _context.SaveChangesAsync();
                string maTaiKhoan = HttpContext.Session.GetString("SignUpAccount");

                if (!string.IsNullOrEmpty(maTaiKhoan))
                {

                    var taiKhoan = await _context.TaiKhoans.FindAsync(maTaiKhoan);
                    if (taiKhoan != null)
                    {
                        taiKhoan.MaKhachHang = customer.MaKhachHang;
                        _context.TaiKhoans.Update(taiKhoan);
                        await _context.SaveChangesAsync();
                    }
                }
                HttpContext.Session.SetString("LoginRoute", "true");
                HttpContext.Session.SetString("UserAccount", maTaiKhoan);
                HttpContext.Session.SetString("User", customer.MaKhachHang);
                return Json(new { success = true, message = "Tạo tài khoản và bổ sung thông tin thành công" });
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                        return Json(new { success = true, message = "Tạo tài khoản và bổ sung thông tin thành công" });
            }

        }
        [HttpPost]
        public async Task<IActionResult> CreateDefaultCustomer()
        {
            try
            {

                var lastCustomer = _context.KhachHangs.OrderByDescending(c => c.MaKhachHang).FirstOrDefault();
                string newMaKhachHang = lastCustomer == null ? "KH000" :
                    "KH" + (int.Parse(lastCustomer.MaKhachHang.Substring(2)) + 1).ToString("D3");

                // Tự động tạo TenKhachHang
                var lastUserName = _context.KhachHangs.OrderByDescending(c => c.TenKhachHang).FirstOrDefault()?.TenKhachHang;
                int newUserNum = lastUserName != null && lastUserName.StartsWith("User") ? int.Parse(lastUserName.Substring(4)) + 1 : 0;
                string newTenKhachHang = $"ToiXinhDep{newUserNum:D5}";

                // Tạo đối tượng KhachHang với các giá trị mặc định
                var newCustomer = new KhachHang
                {
                    MaKhachHang = newMaKhachHang,
                    TenKhachHang = newTenKhachHang,
                    NgayDangKy = DateOnly.FromDateTime(DateTime.Now),
                    TrangThai = 2
                };

                // Thêm vào database
                _context.KhachHangs.Add(newCustomer);
                string maTaiKhoan = HttpContext.Session.GetString("SignUpAccount");
                var Account =_context.TaiKhoans.Where(a => (a.MaTaiKhoan == maTaiKhoan)).FirstOrDefault();
                Account.MaKhachHang = newCustomer.MaKhachHang;
                _context.TaiKhoans.Update(Account);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("LoginRoute", "true");
                HttpContext.Session.SetString("UserAccount", maTaiKhoan);
                HttpContext.Session.SetString("User", newCustomer.MaKhachHang);
                return Json(new { success = true, message = "Tài Khoản của bạn đã được đăng ký thành công" });

            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                return Json(new { success = true, message = "Tài Khoản của bạn đã được đăng ký thành công" });
            };

            // Tự động tạo MaKhachHang
           
        }
        [HttpPost]
        public async Task<IActionResult> CreateCustomerFromGoogleLogin([FromBody] GoogleUserModel user)
        {
            // Check if an account with the given email already exists
            var existingAccount = _context.TaiKhoans.FirstOrDefault(u =>( u.Email == user.Email));
            if (existingAccount == null)
            {
                // Generate new IDs for MaKhachHang and MaTaiKhoan
                var lastCustomer = _context.KhachHangs.OrderByDescending(k => k.MaKhachHang).FirstOrDefault();
                int newCustomerId = lastCustomer != null ? int.Parse(lastCustomer.MaKhachHang.Substring(2)) + 1 : 1;
                string maKhachHang = $"KH{newCustomerId:D3}";
                var lastAccount = _context.TaiKhoans.OrderByDescending(t => t.MaTaiKhoan).FirstOrDefault();
                int newAccountId = lastAccount != null ? int.Parse(lastAccount.MaTaiKhoan.Substring(2)) + 1 : 1;
                string maTaiKhoan = $"TK{newAccountId:D3}";
                // Create and save the new customer
                var newCustomer = new KhachHang
                {
                    MaKhachHang = maKhachHang,
                    TenKhachHang = user.Name,
                    HinhAnh = user.Picture,
                    NgayDangKy = DateOnly.FromDateTime(DateTime.Today),
                    TrangThai = 2
                };
                _context.KhachHangs.Add(newCustomer);
                await _context.SaveChangesAsync();
                // Create and save the new account linked to the new customer
                var newAccount = new TaiKhoan
                {
                    MaTaiKhoan = maTaiKhoan,
                    TenTaiKhoan = user.Email,
                    MatKhau = "123456",
                    VaiTro = "Customer",
                    Email = user.Email,
                    MaKhachHang = maKhachHang,
                    TinhTrang = 1
                };
                _context.TaiKhoans.Add(newAccount);
                await _context.SaveChangesAsync();
				HttpContext.Session.SetString("LoginRoute", "true");
				HttpContext.Session.SetString("UserAccount", newAccount.MaTaiKhoan);
                HttpContext.Session.SetString("User", newCustomer.MaKhachHang);
                return Ok("Đã tạo tài khoản và khách hàng mới thành công.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(existingAccount.MaKhachHang))
                {
                    // Generate a new MaKhachHang for the existing account
                    var lastCustomer = _context.KhachHangs.OrderByDescending(k => k.MaKhachHang).FirstOrDefault();
                    int newCustomerId = lastCustomer != null ? int.Parse(lastCustomer.MaKhachHang.Substring(2)) + 1 : 1;
                    string maKhachHang = $"KH{newCustomerId:D3}";
                    // Create a new customer and link it to the existing account
                    var newCustomer = new KhachHang
                    {
                        MaKhachHang = maKhachHang,
                        TenKhachHang = user.Name,
                        HinhAnh = user.Picture,
                        NgayDangKy = DateOnly.FromDateTime(DateTime.Today),
                        TrangThai = 2
                    };
                    _context.KhachHangs.Add(newCustomer);
                    existingAccount.MaKhachHang = maKhachHang;
                    _context.TaiKhoans.Update(existingAccount);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("LoginRoute", "true");
                    HttpContext.Session.SetString("UserAccount", existingAccount.MaTaiKhoan);
                    HttpContext.Session.SetString("User", newCustomer.MaKhachHang);
                    return Ok("Đã tạo khách hàng mới và liên kết với tài khoản hiện có.");
                }
                else
                {
                    // Update the existing customer's picture
                    var existingCustomer = _context.KhachHangs.FirstOrDefault(k => k.MaKhachHang == existingAccount.MaKhachHang);
                    if (string.IsNullOrWhiteSpace(existingCustomer.HinhAnh))
                    {
                        existingCustomer.TenKhachHang = user.Name;
                        _context.TaiKhoans.Update(existingAccount);
                        await _context.SaveChangesAsync();
                        HttpContext.Session.SetString("LoginRoute", "true");
                        HttpContext.Session.SetString("UserAccount", existingAccount.MaTaiKhoan);
                        HttpContext.Session.SetString("User", existingCustomer.MaKhachHang);
                    }
                    return Json(new { Message = "Cập nhật hình ảnh cho khách hàng hiện có." });
                }
            }
        }

    }
}