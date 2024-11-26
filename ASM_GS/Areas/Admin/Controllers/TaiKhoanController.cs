using ASM_GS.Controllers;
using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;
using X.PagedList;
using X.PagedList.Mvc.Core;
using ASM_GS.ViewModels;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Extensions;
namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;
        public static bool authenticatedAdmin =false;
        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("StaffAccount") == null)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
                ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            }
            ViewData["VaiTro"] = HttpContext.Session.GetString("StaffRole");
            string MaNhanVien = HttpContext.Session.GetString("MaNhanVien");
            return View();
        }
        public IActionResult Customer(string searchTerm, int? pageSize, int page = 1, int? status = null)       
        {
            int pageSizeValue = pageSize ?? 5;

            var ListTaiKhoan = _context.TaiKhoans
                .Where(tk1 => tk1.MaKhachHang != null)
                .Select(tk => new TaiKhoanViewModel
                {
                    MaTaiKhoan = tk.MaTaiKhoan,
                    TenTaiKhoan = tk.TenTaiKhoan,
                    MatKhau = tk.MatKhau,
                    HinhAnh = tk.MaKhachHangNavigation.HinhAnh,
                    TenKhachHang = tk.MaKhachHangNavigation.TenKhachHang,
                    MaKhachHang = tk.MaKhachHang,
                    TinhTrang = tk.TinhTrang.ToString(),
                    VaiTro = tk.VaiTro
                }).ToList();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                ListTaiKhoan = ListTaiKhoan
                    .Where(tk => tk.TenKhachHang.Contains(searchTerm) || tk.TenTaiKhoan.Contains(searchTerm))
                    .ToList();
            }

            if (status.HasValue)
            {
                ListTaiKhoan = ListTaiKhoan
                    .Where(tk => tk.TinhTrang.Trim() == status.ToString().Trim())
                    .ToList();
            }
            var pagedKhachHangs = ListTaiKhoan.ToPagedList(page, pageSizeValue);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_KhachHangTable", pagedKhachHangs);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.PageSize = pageSizeValue;
            ViewBag.Page = page;

            return View(pagedKhachHangs);
        }
        string GenerateNextMaKhachHang()
        {
            // Lấy mã lớn nhất hiện tại
            var maxMaKhachHang = _context.KhachHangs
                .OrderByDescending(kh => kh.MaKhachHang)
                .Select(kh => kh.MaKhachHang)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(maxMaKhachHang))
            {
                return "KH001";
            }
            int nextNumber = int.Parse(maxMaKhachHang.Substring(2)) + 1;
            return "KH" + nextNumber.ToString("D3");
        }
        string GenerateNextMaTaiKhoan()
        {
            var maxMaTaiKhoan = _context.TaiKhoans
                .OrderByDescending(tk => tk.MaTaiKhoan)
                .Select(tk => tk.MaTaiKhoan)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(maxMaTaiKhoan))
            {
                return "TK001";
            }
            int nextNumber = int.Parse(maxMaTaiKhoan.Substring(2)) + 1;
            return "TK" + nextNumber.ToString("D3");
        }

        public IActionResult GetCustomerDetails(string maKhachHang, string maTaiKhoan)
        {
            var customerDetails = (from kh in _context.KhachHangs
                                   join tk in _context.TaiKhoans on kh.MaKhachHang equals tk.MaKhachHang into jointData
                                   from tk in jointData.DefaultIfEmpty()
                                   where kh.MaKhachHang == maKhachHang
                                   select new
                                   {
                                       kh.MaKhachHang,
                                       kh.TenKhachHang,
                                       kh.SoDienThoai,
                                       kh.DiaChi,
                                       kh.HinhAnh,
                                       kh.Cccd,
                                       kh.NgaySinh,
                                       kh.GioiTinh,

                                       tk.MaTaiKhoan,
                                       tk.TenTaiKhoan,
                                       tk.MatKhau,
                                       tk.Email,
                                       TrangThai = tk.TinhTrang
                                   }).FirstOrDefault();

            if (customerDetails == null)
            {
                return NotFound();
            }

            return Json(customerDetails);
        }
        [HttpPost]
        public IActionResult UpdateAccountCustomer(string maTaiKhoan, string matKhau, string email)
        {
            // Kiểm tra thông tin và validate
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(matKhau) || matKhau.Length < 6)
                errors["matKhau"] = "Mật khẩu phải có ít nhất 6 ký tự.";
            if(email==null)
                errors["email"] = "Email không hợp lệ.";
            else if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                errors["email"] = "Email không hợp lệ.";
            if (_context.TaiKhoans.Any(tk => tk.Email == email && tk.MaTaiKhoan != maTaiKhoan))
                errors["email"] = "Email đã tồn tại.";
            if (errors.Count > 0)
                return Json(new { success = false, errors });

            // Cập nhật thông tin tài khoản
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(tk => tk.MaTaiKhoan == maTaiKhoan);
            if (taiKhoan == null)
                return Json(new { success = false, message = "Tài khoản không tồn tại." });

            taiKhoan.MatKhau = matKhau;
            taiKhoan.Email = email;

            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult DisableAccountCustomer(string maTaiKhoan)
        {
            var account = _context.TaiKhoans.FirstOrDefault(tk => tk.MaTaiKhoan == maTaiKhoan);
            if (account == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
            }

            account.TinhTrang = 0; 
            _context.SaveChanges();

            return Json(new { success = true, message = "Tài khoản đã được ngưng hoạt động!" });
        }
        [HttpPost]
        public IActionResult EnableAccountCustomer(string maTaiKhoan)
        {
            var account = _context.TaiKhoans.FirstOrDefault(tk => tk.MaTaiKhoan == maTaiKhoan);
            if (account == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
            }
            var customer = _context.KhachHangs.FirstOrDefault(kh => kh.MaKhachHang == account.MaKhachHang);
            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng!" });
            }
            bool isComplete = !string.IsNullOrEmpty(customer.DiaChi)
                              && !string.IsNullOrEmpty(customer.Cccd)
                              && !string.IsNullOrEmpty(customer.SoDienThoai)
                              && customer.NgaySinh.HasValue
                              && customer.TenKhachHang != "ToiXinhDep00000";
            account.TinhTrang = isComplete ? 1 : 2;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = isComplete
                ? "Tài khoản đã được tái kích hoạt!"
                : "Tài khoản thiếu thông tin và chưa thể kích hoạt đầy đủ!"
            });
        }
        [HttpPost]
        public IActionResult ValidateAndAddAccountCustomer(
       string tenTaiKhoan,
       string matKhau,
       string reEnterMatKhau,
       string email,
       string tenKhachHang,
       string soDienThoai,
       string diaChi,
       string cccd,
       DateOnly? ngaySinh,
       string? gioiTinh)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(tenTaiKhoan))
                errors["TenTaiKhoan"] = "Tên tài khoản không được để trống.";
            else if (_context.TaiKhoans.Any(x => x.TenTaiKhoan == tenTaiKhoan))
            {
                errors["TenTaiKhoan"] = "Tên tài khoản đã tồn tại.";
            }
            if (string.IsNullOrWhiteSpace(matKhau))
                errors["MatKhau"] = "Mật khẩu không được để trống.";
            if (string.IsNullOrWhiteSpace(reEnterMatKhau))
                errors["ReEnterMatKhau"] = "Vui lòng nhập lại mật khẩu.";
            if (string.IsNullOrWhiteSpace(email))
                errors["Email"] = "Email không được để trống.";
            if (_context.TaiKhoans.Any(x => x.Email == email))
            {
                errors["Email"] = "Email đã tồn tại.";
            }
            if (string.IsNullOrWhiteSpace(tenKhachHang))
                errors["TenKhachHang"] = "Tên khách hàng không được để trống.";
            if (string.IsNullOrWhiteSpace(diaChi))
                errors["DiaChi"] = "Địa chỉ không được để trống.";

            if (string.IsNullOrWhiteSpace(soDienThoai) || !soDienThoai.StartsWith("0") ||
                (soDienThoai.Length != 10 && soDienThoai.Length != 11))
            {
                errors["SoDienThoai"] = "Số điện thoại phải bắt đầu bằng số 0 và có 10-11 ký tự.";
            }

            if (string.IsNullOrWhiteSpace(cccd) || !cccd.StartsWith("0") || cccd.Length != 12)
            {
                errors["Cccd"] = "CCCD phải bắt đầu bằng số 0 và có đúng 12 ký tự.";
            }
            if (!ngaySinh.HasValue)
            {
                errors["NgaySinh"] = "Ngày sinh không được để trống.";
            }
            else if (ngaySinh > DateOnly.FromDateTime(DateTime.Now.AddYears(-15)))
            {
                errors["NgaySinh"] = "Khách hàng phải trên 15 tuổi.";
            }
            if (!string.IsNullOrWhiteSpace(matKhau) && matKhau.Length < 6)
            {
                errors["MatKhau"] = "Mật khẩu phải lớn hơn hoặc bằng 6 ký tự.";
            }
            if (matKhau != reEnterMatKhau)
            {
                errors["ReEnterMatKhau"] = "Mật khẩu nhập lại không khớp.";
            }

            if (errors.Any())
            {
                return Json(new { success = false, errors });
            }

            var maKhachHang = GenerateNextMaKhachHang();
            var maTaiKhoan = GenerateNextMaTaiKhoan();

            var newCustomer = new KhachHang
            {
                MaKhachHang = maKhachHang,
                TenKhachHang = tenKhachHang,
                SoDienThoai = soDienThoai,
                DiaChi = diaChi,
                Cccd = cccd,
                NgaySinh = ngaySinh,
                GioiTinh = true?gioiTinh=="Nam":false,
                TrangThai = 1, 
                NgayDangKy = DateOnly.FromDateTime(DateTime.Now),
            };
            _context.KhachHangs.Add(newCustomer);

            var newAccount = new TaiKhoan
            {
                MaTaiKhoan = maTaiKhoan,
                TenTaiKhoan = tenTaiKhoan,
                MatKhau = matKhau,
                Email = email,
                VaiTro = "Customer", 
                TinhTrang = 1, 
                MaKhachHang = maKhachHang
            };
            _context.TaiKhoans.Add(newAccount);

            _context.SaveChanges();

            return Json(new { success = true });
        }

        //TaiKhoanStaff
        public IActionResult Staff(string searchTerm, int? pageSize, int page = 1, int? status = null)
        {
            if (HttpContext.Session.GetString("StaffAccount") == null || authenticatedAdmin == false)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
                ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            }
            int pageSizeValue = pageSize ?? 5;
            var ListTaiKhoan = _context.TaiKhoans
        .Where(tk1 => tk1.MaNhanVien != null && tk1.VaiTro == "Staff")
        .Include(tk => tk.MaNhanVienNavigation) // Tải dữ liệu liên kết
        .Select(tk => new TaiKhoanViewModel
        {
            MaTaiKhoan = tk.MaTaiKhoan,
            TenTaiKhoan = tk.TenTaiKhoan,
            MatKhau = tk.MatKhau,
            HinhAnh = tk.MaNhanVienNavigation != null ? tk.MaNhanVienNavigation.HinhAnh : null,
            TenKhachHang = tk.MaNhanVienNavigation != null ? tk.MaNhanVienNavigation.TenNhanVien : "N/A",
            MaKhachHang = tk.MaNhanVien,
            TinhTrang = tk.TinhTrang.ToString(),
            VaiTro = tk.VaiTro
        }).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                ListTaiKhoan = ListTaiKhoan
                    .Where(tk => tk.TenKhachHang.Contains(searchTerm) || tk.TenTaiKhoan.Contains(searchTerm))
                    .ToList();
            }

            if (status.HasValue)
            {
                ListTaiKhoan = ListTaiKhoan
                    .Where(tk => tk.TinhTrang.Trim() == status.ToString().Trim())
                    .ToList();
            }
            var pagedKhachHangs = ListTaiKhoan.ToPagedList(page, pageSizeValue);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_KhachHangTable", pagedKhachHangs);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.PageSize = pageSizeValue;
            ViewBag.Page = page;

            return View(pagedKhachHangs);
        }

        public IActionResult GetStaffDetails(string maKhachHang, string maTaiKhoan)
        {
            var customerDetails = (from kh in _context.NhanViens
                                   join tk in _context.TaiKhoans on kh.MaNhanVien equals tk.MaNhanVien into jointData
                                   from tk in jointData.DefaultIfEmpty()
                                   where kh.MaNhanVien == maKhachHang
                                   select new
                                   {
                                       kh.MaNhanVien,
                                       kh.TenNhanVien,
                                       kh.SoDienThoai,
                                       kh.DiaChi,
                                       kh.HinhAnh,
                                       kh.Cccd,
                                       kh.NgaySinh,
                                       kh.GioiTinh,

                                       tk.MaTaiKhoan,
                                       tk.TenTaiKhoan,
                                       tk.MatKhau,
                                       tk.Email,
                                       TrangThai = tk.TinhTrang
                                   }).FirstOrDefault();

            if (customerDetails == null)
            {
                return NotFound();
            }

            return Json(customerDetails);
        }
        [HttpPost]
        public IActionResult UpdateAccountStaff(string maTaiKhoan, string matKhau, string email)
        {

            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(matKhau) || matKhau.Length < 6)
                errors["matKhau"] = "Mật khẩu phải có ít nhất 6 ký tự.";
            if (email == null)
                errors["email"] = "Email không hợp lệ.";
            else if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                errors["email"] = "Email không hợp lệ.";
            if (_context.TaiKhoans.Any(tk => tk.Email == email && tk.MaTaiKhoan != maTaiKhoan))
                errors["email"] = "Email đã tồn tại.";
            if (errors.Count > 0)
                return Json(new { success = false, errors });

            // Cập nhật thông tin tài khoản
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(tk => tk.MaTaiKhoan == maTaiKhoan);
            if (taiKhoan == null)
                return Json(new { success = false, message = "Tài khoản không tồn tại." });

            taiKhoan.MatKhau = matKhau;
            taiKhoan.Email = email;

            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult DisableAccountStaff(string maTaiKhoan)
        {
            var account = _context.TaiKhoans.FirstOrDefault(tk => tk.MaTaiKhoan == maTaiKhoan);
            if (account == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
            }

            account.TinhTrang = 0;
            _context.SaveChanges();

            return Json(new { success = true, message = "Tài khoản đã được ngưng hoạt động!" });
        }
        [HttpPost]
        public IActionResult EnableAccountStaff(string maTaiKhoan)
        {
            var account = _context.TaiKhoans.FirstOrDefault(tk => tk.MaTaiKhoan == maTaiKhoan);
            if (account == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
            }
            var customer = _context.NhanViens.FirstOrDefault(kh => kh.MaNhanVien == account.MaNhanVien);
            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng!" });
            }
            bool isComplete = !string.IsNullOrEmpty(customer.DiaChi)
                              && !string.IsNullOrEmpty(customer.Cccd)
                              && !string.IsNullOrEmpty(customer.SoDienThoai)
                              && customer.NgaySinh.HasValue;
            account.TinhTrang = isComplete ? 1 : 2;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = isComplete
                ? "Tài khoản đã được tái kích hoạt!"
                : "Tài khoản thiếu thông tin và chưa thể kích hoạt đầy đủ!"
            });
        }
        string GenerateNextMaKhachHang2()
        {
            // Lấy mã lớn nhất hiện tại
            var maxMaKhachHang = _context.NhanViens
                .OrderByDescending(kh => kh.MaNhanVien)
                .Select(kh => kh.MaNhanVien)
                .FirstOrDefault();

            // Nếu chưa có mã nào, khởi tạo từ "KH001"
            if (string.IsNullOrEmpty(maxMaKhachHang))
            {
                return "NV001";
            }

            // Lấy phần số từ mã hiện tại, tăng lên 1 và format lại
            int nextNumber = int.Parse(maxMaKhachHang.Substring(2)) + 1;
            return "NV" + nextNumber.ToString("D3");
        }
        string GenerateNextMaTaiKhoan2()
        {
            var maxMaTaiKhoan = _context.TaiKhoans
                .OrderByDescending(tk => tk.MaTaiKhoan)
                .Select(tk => tk.MaTaiKhoan)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(maxMaTaiKhoan))
            {
                return "TK001";
            }
            int nextNumber = int.Parse(maxMaTaiKhoan.Substring(2)) + 1;
            return "TK" + nextNumber.ToString("D3");
        }
        [HttpPost]
        public IActionResult ValidateAndAddAccountStaff(
               string tenTaiKhoan,
               string matKhau,
               string reEnterMatKhau,
               string email,
               string tenKhachHang,
               string soDienThoai,
               string diaChi,
               string cccd,
               DateOnly? ngaySinh,
               string? gioiTinh)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(tenTaiKhoan))
                errors["TenTaiKhoan"] = "Tên tài khoản không được để trống.";
            else if (_context.TaiKhoans.Any(x => x.TenTaiKhoan == tenTaiKhoan))
            {
                errors["TenTaiKhoan"] = "Tên tài khoản đã tồn tại.";
            }
            if (string.IsNullOrWhiteSpace(matKhau))
                errors["MatKhau"] = "Mật khẩu không được để trống.";
            if (string.IsNullOrWhiteSpace(reEnterMatKhau))
                errors["ReEnterMatKhau"] = "Vui lòng nhập lại mật khẩu.";
            if (string.IsNullOrWhiteSpace(email))
                errors["Email"] = "Email không được để trống.";
            if (_context.TaiKhoans.Any(x => x.Email == email))
            {
                errors["Email"] = "Email đã tồn tại.";
            }
            if (string.IsNullOrWhiteSpace(tenKhachHang))
                errors["TenKhachHang"] = "Tên nhân viên không được để trống.";
            if (string.IsNullOrWhiteSpace(diaChi))
                errors["DiaChi"] = "Địa chỉ không được để trống.";

            if (string.IsNullOrWhiteSpace(soDienThoai) || !soDienThoai.StartsWith("0") ||
                (soDienThoai.Length != 10 && soDienThoai.Length != 11))
            {
                errors["SoDienThoai"] = "Số điện thoại phải bắt đầu bằng số 0 và có 10-11 ký tự.";
            }

            if (string.IsNullOrWhiteSpace(cccd) || !cccd.StartsWith("0") || cccd.Length != 12)
            {
                errors["Cccd"] = "CCCD phải bắt đầu bằng số 0 và có đúng 12 ký tự.";
            }
            if (!ngaySinh.HasValue)
            {
                errors["NgaySinh"] = "Ngày sinh không được để trống.";
            }
            else if (ngaySinh > DateOnly.FromDateTime(DateTime.Now.AddYears(-18)))
            {
                errors["NgaySinh"] = "Khách hàng phải trên 18 tuổi.";
            }
            if (!string.IsNullOrWhiteSpace(matKhau) && matKhau.Length < 6)
            {
                errors["MatKhau"] = "Mật khẩu phải lớn hơn hoặc bằng 6 ký tự.";
            }
            if (matKhau != reEnterMatKhau)
            {
                errors["ReEnterMatKhau"] = "Mật khẩu nhập lại không khớp.";
            }

            if (errors.Any())
            {
                return Json(new { success = false, errors });
            }

            var maKhachHang = GenerateNextMaKhachHang2();
            var maTaiKhoan = GenerateNextMaTaiKhoan2();

            var newCustomer = new NhanVien
            {
                MaNhanVien = maKhachHang,
                TenNhanVien = tenKhachHang,
                SoDienThoai = soDienThoai,
                DiaChi = diaChi,
                Cccd = cccd,
                NgaySinh = ngaySinh,
                VaiTro = "Staff",
                GioiTinh = true ? gioiTinh == "Nam" : false,
                TrangThai = 1,
                NgayBatDau = DateOnly.FromDateTime(DateTime.Now),
            };
            _context.NhanViens.Add(newCustomer);

            var newAccount = new TaiKhoan
            {
                MaTaiKhoan = maTaiKhoan,
                TenTaiKhoan = tenTaiKhoan,
                MatKhau = matKhau,
                Email = email,
                VaiTro = "Staff",
                TinhTrang = 1,
                MaNhanVien = maKhachHang
            };
            _context.TaiKhoans.Add(newAccount);

            _context.SaveChanges();

            return Json(new { success = true });
        }

        //TaiKhoanAdmin
        public IActionResult Admin(string searchTerm, int? pageSize, int page = 1, int? status = null)
        {
            if (HttpContext.Session.GetString("StaffAccount") == null || authenticatedAdmin == false)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
                ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            }
            int pageSizeValue = pageSize ?? 5;
            var ListTaiKhoan = _context.TaiKhoans
        .Where(tk1 => tk1.MaNhanVien != null && tk1.VaiTro == "Admin")
        .Include(tk => tk.MaNhanVienNavigation) // Tải dữ liệu liên kết
        .Select(tk => new TaiKhoanViewModel
        {
            MaTaiKhoan = tk.MaTaiKhoan,
            TenTaiKhoan = tk.TenTaiKhoan,
            MatKhau = tk.MatKhau,
            HinhAnh = tk.MaNhanVienNavigation != null ? tk.MaNhanVienNavigation.HinhAnh : null,
            TenKhachHang = tk.MaNhanVienNavigation != null ? tk.MaNhanVienNavigation.TenNhanVien : "N/A",
            MaKhachHang = tk.MaNhanVien,
            TinhTrang = tk.TinhTrang.ToString(),
            VaiTro = tk.VaiTro
        }).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                ListTaiKhoan = ListTaiKhoan
                    .Where(tk => tk.TenKhachHang.Contains(searchTerm) || tk.TenTaiKhoan.Contains(searchTerm))
                    .ToList();
            }

            if (status.HasValue)
            {
                ListTaiKhoan = ListTaiKhoan
                    .Where(tk => tk.TinhTrang.Trim() == status.ToString().Trim())
                    .ToList();
            }
            var pagedKhachHangs = ListTaiKhoan.ToPagedList(page, pageSizeValue);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_KhachHangTable", pagedKhachHangs);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.PageSize = pageSizeValue;
            ViewBag.Page = page;

            return View(pagedKhachHangs);
        }

        public IActionResult GetAdminDetails(string maKhachHang, string maTaiKhoan)
        {
            var customerDetails = (from kh in _context.NhanViens
                                   join tk in _context.TaiKhoans on kh.MaNhanVien equals tk.MaNhanVien into jointData
                                   from tk in jointData.DefaultIfEmpty()
                                   where kh.MaNhanVien == maKhachHang
                                   select new
                                   {
                                       kh.MaNhanVien,
                                       kh.TenNhanVien,
                                       kh.SoDienThoai,
                                       kh.DiaChi,
                                       kh.HinhAnh,
                                       kh.Cccd,
                                       kh.NgaySinh,
                                       kh.GioiTinh,

                                       tk.MaTaiKhoan,
                                       tk.TenTaiKhoan,
                                       tk.MatKhau,
                                       tk.Email,
                                       TrangThai = tk.TinhTrang
                                   }).FirstOrDefault();

            if (customerDetails == null)
            {
                return NotFound();
            }

            return Json(customerDetails);
        }
        [HttpPost]
        public IActionResult UpdateAccountAdmin(string maTaiKhoan, string matKhau, string email)
        {

            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(matKhau) || matKhau.Length < 6)
                errors["matKhau"] = "Mật khẩu phải có ít nhất 6 ký tự.";
            if (email == null)
                errors["email"] = "Email không hợp lệ.";
            else if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                errors["email"] = "Email không hợp lệ.";
            if (_context.TaiKhoans.Any(tk => tk.Email == email && tk.MaTaiKhoan != maTaiKhoan))
                errors["email"] = "Email đã tồn tại.";
            if (errors.Count > 0)
                return Json(new { success = false, errors });

            // Cập nhật thông tin tài khoản
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(tk => tk.MaTaiKhoan == maTaiKhoan);
            if (taiKhoan == null)
                return Json(new { success = false, message = "Tài khoản không tồn tại." });

            taiKhoan.MatKhau = matKhau;
            taiKhoan.Email = email;

            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult DisableAccountAdmin(string maTaiKhoan)
        {
            var account = _context.TaiKhoans.FirstOrDefault(tk => tk.MaTaiKhoan == maTaiKhoan);
            if (account == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
            }

            account.TinhTrang = 0;
            _context.SaveChanges();

            return Json(new { success = true, message = "Tài khoản đã được ngưng hoạt động!" });
        }
        [HttpPost]
        public IActionResult EnableAccountAdmin(string maTaiKhoan)
        {
            var account = _context.TaiKhoans.FirstOrDefault(tk => tk.MaTaiKhoan == maTaiKhoan);
            if (account == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
            }
            var customer = _context.NhanViens.FirstOrDefault(kh => kh.MaNhanVien == account.MaNhanVien);
            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng!" });
            }
            bool isComplete = !string.IsNullOrEmpty(customer.DiaChi)
                              && !string.IsNullOrEmpty(customer.Cccd)
                              && !string.IsNullOrEmpty(customer.SoDienThoai)
                              && customer.NgaySinh.HasValue;
            account.TinhTrang = isComplete ? 1 : 2;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = isComplete
                ? "Tài khoản đã được tái kích hoạt!"
                : "Tài khoản thiếu thông tin và chưa thể kích hoạt đầy đủ!"
            });
        }
        string GenerateNextMaKhachHang3()
        {
            // Lấy mã lớn nhất hiện tại
            var maxMaKhachHang = _context.NhanViens
                .OrderByDescending(kh => kh.MaNhanVien)
                .Select(kh => kh.MaNhanVien)
                .FirstOrDefault();

            // Nếu chưa có mã nào, khởi tạo từ "KH001"
            if (string.IsNullOrEmpty(maxMaKhachHang))
            {
                return "NV001";
            }

            // Lấy phần số từ mã hiện tại, tăng lên 1 và format lại
            int nextNumber = int.Parse(maxMaKhachHang.Substring(2)) + 1;
            return "NV" + nextNumber.ToString("D3");
        }
        string GenerateNextMaTaiKhoan3()
        {
            var maxMaTaiKhoan = _context.TaiKhoans
                .OrderByDescending(tk => tk.MaTaiKhoan)
                .Select(tk => tk.MaTaiKhoan)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(maxMaTaiKhoan))
            {
                return "TK001";
            }
            int nextNumber = int.Parse(maxMaTaiKhoan.Substring(2)) + 1;
            return "TK" + nextNumber.ToString("D3");
        }
        [HttpPost]
        public IActionResult ValidateAndAddAccountAdmin(
               string tenTaiKhoan,
               string matKhau,
               string reEnterMatKhau,
               string email,
               string tenKhachHang,
               string soDienThoai,
               string diaChi,
               string cccd,
               DateOnly? ngaySinh,
               string? gioiTinh)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(tenTaiKhoan))
                errors["TenTaiKhoan"] = "Tên tài khoản không được để trống.";
            else if (_context.TaiKhoans.Any(x => x.TenTaiKhoan == tenTaiKhoan))
            {
                errors["TenTaiKhoan"] = "Tên tài khoản đã tồn tại.";
            }
            if (string.IsNullOrWhiteSpace(matKhau))
                errors["MatKhau"] = "Mật khẩu không được để trống.";
            if (string.IsNullOrWhiteSpace(reEnterMatKhau))
                errors["ReEnterMatKhau"] = "Vui lòng nhập lại mật khẩu.";
            if (string.IsNullOrWhiteSpace(email))
                errors["Email"] = "Email không được để trống.";
            if (_context.TaiKhoans.Any(x => x.Email == email))
            {
                errors["Email"] = "Email đã tồn tại.";
            }
            if (string.IsNullOrWhiteSpace(tenKhachHang))
                errors["TenKhachHang"] = "Tên nhân viên không được để trống.";
            if (string.IsNullOrWhiteSpace(diaChi))
                errors["DiaChi"] = "Địa chỉ không được để trống.";

            if (string.IsNullOrWhiteSpace(soDienThoai) || !soDienThoai.StartsWith("0") ||
                (soDienThoai.Length != 10 && soDienThoai.Length != 11))
            {
                errors["SoDienThoai"] = "Số điện thoại phải bắt đầu bằng số 0 và có 10-11 ký tự.";
            }

            if (string.IsNullOrWhiteSpace(cccd) || !cccd.StartsWith("0") || cccd.Length != 12)
            {
                errors["Cccd"] = "CCCD phải bắt đầu bằng số 0 và có đúng 12 ký tự.";
            }
            if (!ngaySinh.HasValue)
            {
                errors["NgaySinh"] = "Ngày sinh không được để trống.";
            }
            else if (ngaySinh > DateOnly.FromDateTime(DateTime.Now.AddYears(-18)))
            {
                errors["NgaySinh"] = "Khách hàng phải trên 18 tuổi.";
            }
            if (!string.IsNullOrWhiteSpace(matKhau) && matKhau.Length < 6)
            {
                errors["MatKhau"] = "Mật khẩu phải lớn hơn hoặc bằng 6 ký tự.";
            }
            if (matKhau != reEnterMatKhau)
            {
                errors["ReEnterMatKhau"] = "Mật khẩu nhập lại không khớp.";
            }

            if (errors.Any())
            {
                return Json(new { success = false, errors });
            }

            var maKhachHang = GenerateNextMaKhachHang3();
            var maTaiKhoan = GenerateNextMaTaiKhoan3();

            var newCustomer = new NhanVien
            {
                MaNhanVien = maKhachHang,
                TenNhanVien = tenKhachHang,
                SoDienThoai = soDienThoai,
                DiaChi = diaChi,
                Cccd = cccd,
                NgaySinh = ngaySinh,
                VaiTro = "Admin",
                GioiTinh = true ? gioiTinh == "Nam" : false,
                TrangThai = 1,
                NgayBatDau = DateOnly.FromDateTime(DateTime.Now),
            };
            _context.NhanViens.Add(newCustomer);

            var newAccount = new TaiKhoan
            {
                MaTaiKhoan = maTaiKhoan,
                TenTaiKhoan = tenTaiKhoan,
                MatKhau = matKhau,
                Email = email,
                VaiTro = "Admin",
                TinhTrang = 1,
                MaNhanVien = maKhachHang
            };
            _context.TaiKhoans.Add(newAccount);

            _context.SaveChanges();

            return Json(new { success = true });
        }
        public class PasswordRequest
        {
            public string Password { get; set; }
        }

        [HttpPost]
        public IActionResult ValidateAdminPassword([FromBody] PasswordRequest request)
        {
            string MaNhanVien = HttpContext.Session.GetString("MaNhanVien");
            var admin = _context.TaiKhoans.FirstOrDefault(nv => nv.MaNhanVien == MaNhanVien);

            if (admin != null && admin.VaiTro == "Admin" && admin.MatKhau == request.Password)
            {
                authenticatedAdmin = true;
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
