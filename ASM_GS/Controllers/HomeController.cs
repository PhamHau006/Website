using ASM_GS.Models;
using ASM_GS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace ASM_GS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly GeminiSettings _authSettings;
        public HomeController(ILogger<HomeController> logger, IOptions<GeminiSettings> authSettings, ApplicationDbContext context)
        {
            _context = context;
            _authSettings = authSettings.Value;
            _logger = logger;
            _context = context; // Assign _context here
        }

        public IActionResult Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // Lấy danh sách sản phẩm
            List<SanPham> sanPhamList = _context.SanPhams
                .Where(h => h.SoLuong > 0 && h.TrangThai == 1)
                .AsNoTracking() // thêm AsNoTracking
                .ToList();

            // Lấy giảm giá nhiều nhất
            var giamNhieuNhat = _context.GiamGia
                .Where(g => g.NgayBatDau <= today && g.NgayKetThuc >= today && g.TrangThai == 1)
                .OrderByDescending(g => g.GiaTri)
                .AsNoTracking() // thêm AsNoTracking
                .FirstOrDefault();

            // Lấy danh sách tên danh mục
            var danhMucDict = _context.DanhMucs
                .AsNoTracking() // thêm AsNoTracking
                .ToDictionary(dm => dm.MaDanhMuc, dm => dm.TenDanhMuc);

            // Xử lý kết quả
            var sanPhamList2 = sanPhamList.Select(item => new SearchingProduct
            {
                MaSanPham = item.MaSanPham,
                TenSanPham = item.TenSanPham,
                GiaGoc = (float)item.Gia,
                MoTa = item.MoTa,
                MaDanhMuc = danhMucDict.ContainsKey(item.MaDanhMuc) ? danhMucDict[item.MaDanhMuc] : null,
                Ava = _context.AnhSanPhams.Where(a => (a.MaSanPham == item.MaSanPham)).Select(a => a.UrlAnh).FirstOrDefault(),
                DonVi = item.DonVi,
                TrangThai = item.TrangThai ?? 1,
                GiaDaGiam = giamNhieuNhat != null ? (float)(item.Gia * (1 - giamNhieuNhat.GiaTri)) : (float)item.Gia,
                TietKiem = giamNhieuNhat != null ? (float)(giamNhieuNhat.GiaTri * item.Gia) : 0
            }).ToList();
            ViewData["SanPhamList2"] = sanPhamList2;
            ViewData["Account"] = _context.TaiKhoans
                .Where(a => a.MaTaiKhoan == HttpContext.Session.GetString("UserAccount"))
                .Select(a => new
                {
                    MaTaiKhoan = a.MaTaiKhoan,
                    TenTaiKhoan = a.TenTaiKhoan,

                })
                .FirstOrDefault();

            ViewData["Customer"] = _context.KhachHangs
                .Where(a => a.MaKhachHang == HttpContext.Session.GetString("User"))
                .Select(c => new
                {
                    MaKhachHang = c.MaKhachHang,
                    TenKhachHang = c.TenKhachHang,
                })
                .FirstOrDefault();
            ViewData["RoutedFromLogin"] = HttpContext.Session.GetString("LoginRoute");


            string maKhachHang = HttpContext.Session.GetString("User");
            string tenKhachHang = string.Empty;

            if (!string.IsNullOrEmpty(maKhachHang))
            {
                var khachHang = _context.KhachHangs.FirstOrDefault(kh => kh.MaKhachHang == maKhachHang);
                if (khachHang != null)
                {
                    tenKhachHang = khachHang.TenKhachHang;
                }
            }

            ViewBag.TenKhachHang = tenKhachHang;
            return View();
        }
        public IActionResult Privacy()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public class GeminiAPI
        {
            public string GoogleAPIUrl { get; set; }
            public string GoogleAPIKey { get; set; }
        }

        public class GeminiSettings
        {
            public GeminiAPI Google { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> TraLoi([FromBody] string userInput)
        {
            string Openning = "Dưới đây là danh sách sản phẩm và combo chứa các sản phẩm của cửa hàng của tôi: \n Danh Sách sản phẩm: ";
            var ListSanPham = _context.SanPhams.ToList();
            var ListCombo = _context.ChiTietCombos.ToList();
            for (int i = 0; i < ListSanPham.Count(); i++)
            {
                Openning += (i + 1).ToString() + ". Mã:" + ListSanPham[i].MaSanPham + ", Tên:" + ListSanPham[i].TenSanPham + ", Mô tả:" + ListSanPham[i].MoTa + ", Giá:" + ListSanPham[i].Gia + "\n" + "Số lượng còn lại:" + ListSanPham[i].SoLuong.ToString();
            }
            var Test = userInput;
            var GoogleAPIKey = _authSettings.Google.GoogleAPIKey;
            var GoogleAPIUrl = _authSettings.Google.GoogleAPIUrl;
            var requestBody = new
            {
                contents = new[]
                {
         new
         {
             parts = new[]
             {
                new{  text = Openning +
             "\nYêu cầu đầu ra: Tôi sẽ đưa câu trả lời của bạn vào innerHTML , do đó, nếu cần xuống dòng hãy thêm <br>, nếu cần gắn link sản phẩm hãy gắn <button class='btn btn-primary'><a href=\"https://localhost:7265/Detail/Index/@maSanPham\">Xem Chi Tiết</a></button>." +
             "\n Hãy linh hoạt trong câu trả lời, nếu khách hàng hỏi câu hỏi bên ngoài thì vẫn trả lời và không cần đề cập đến các sản phẩm trong cửa hàng mà hãy trả lời như bình thường." +
             " Còn nếu câu hỏi về cần tư vấn sản phẩm thì hãy dựa vào dữ liệu tôi đưa ( khi cần ) để trả lời câu hỏi sau : " + userInput
             }
         },
     } }
            }
            ;

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");


            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={GoogleAPIKey}", content);
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);


                string answer = responseObject?.candidates[0].content?.parts[0]?.text ?? "Xin lỗi, câu hỏi của bạn đã vi phạm chính sách của Google hoặc câu trở lời quá dài nên Rem không hiển thị cho bạn được";
                return Json(new { response = answer });
            }
        }
        [HttpPost]
        public IActionResult KiemTraDangNhap()
        {
            var userAccount = HttpContext.Session.GetString("UserAccount");
            var user = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(userAccount) || string.IsNullOrEmpty(user))
            {
                return Json(new { isLoggedIn = false });
            }

            return Json(new { isLoggedIn = true });
        }


        public IActionResult RemoveRoutedFromLoginSession()
        {
            HttpContext.Session.Remove("LoginRoute");
            ViewData["RoutedFromLogin"] = HttpContext.Session.GetString("LoginRoute");
            return Ok();
        }
        public IActionResult Logout()
        {
            // Xóa các session
            HttpContext.Session.Remove("LoginRoute");
            HttpContext.Session.Remove("UserAccount");
            HttpContext.Session.Remove("User");
            ViewData["Account"] = null;
            ViewData["RoutedFromLogin"] = null;
            ViewData["Customer"] =null;
            // Chuyển hướng đến trang đăng nhập
            return RedirectToAction("Index", "LoginAndSignUp");
        }
    }
}