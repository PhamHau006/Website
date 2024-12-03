using ASM_GS.Controllers;
using ASM_GS.Models;
using ASM_GS.ViewModels;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;
namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private string? search;
        private string? searchTerm;

        public DonHangController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> ExportToExcel(string searchTerm = "")
        {
            IQueryable<DonHang> ordersQuery = _context.DonHangs
                .Include(dh => dh.KhachHang); 

            if (!string.IsNullOrEmpty(searchTerm))
            {
                ordersQuery = ordersQuery.Where(dh => dh.MaDonHang.Contains(searchTerm) || dh.KhachHang.TenKhachHang.Contains(searchTerm));
            }

            var orders = await ordersQuery.ToListAsync();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Orders");
                worksheet.Cells[1, 1].Value = "Mã Đơn Hàng";
                worksheet.Cells[1, 2].Value = "Mã Khách Hàng";
                worksheet.Cells[1, 3].Value = "Tên Khách Hàng";
                worksheet.Cells[1, 4].Value = "Ngày Đặt Hàng";
                worksheet.Cells[1, 5].Value = "Tổng Tiền";
                worksheet.Cells[1, 6].Value = "Trạng Thái";

                // Đổ dữ liệu vào các ô Excel
                int row = 2;
                foreach (var order in orders)
                {
                    worksheet.Cells[row, 1].Value = order.MaDonHang;
                    worksheet.Cells[row, 2].Value = order.MaKhachHang;
                    worksheet.Cells[row, 3].Value = order.KhachHang.TenKhachHang;
                    worksheet.Cells[row, 4].Value = order.NgayDatHang.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 5].Value = order.TongTien.ToString("N0") + " VNĐ";

                    // Chuyển trạng thái thành chuỗi
                    string trangThai = order.TrangThai switch
                    {
                        0 => "Đang xử lý",
                        1 => "Đang giao",
                        2 => "Đã giao",
                        4 => "Đã huỷ",
                        _ => "Chưa xác định"
                    };
                    worksheet.Cells[row, 6].Value = trangThai;

                    row++;
                }

                // Tải xuống file Excel
                var fileStream = new MemoryStream();
                package.SaveAs(fileStream);
                fileStream.Position = 0;

                // Trả về file Excel cho người dùng tải về
                return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DonHang.xlsx");
            }
        }

        // Action hiển thị danh sách đơn hàng
        public async Task<IActionResult> Index(string trangThai = "all", int page = 1, int pageSize = 5)
        {
            if (HttpContext.Session.GetString("StaffAccount") == null)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
                ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            }

            IQueryable<DonHang> ordersQuery = _context.DonHangs
                .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.MaSanPhamNavigation)
                .Include(dh => dh.KhachHang);
            switch (trangThai.ToLower())
            {
                case "processing":
                    ordersQuery = ordersQuery.Where(dh => dh.TrangThai == 0);
                    break;
                case "shipped":
                    ordersQuery = ordersQuery.Where(dh => dh.TrangThai == 1);
                    break;
                case "completed":
                    ordersQuery = ordersQuery.Where(dh => dh.TrangThai == 2);
                    break;
                case "refunded":
                    ordersQuery = ordersQuery.Where(dh => dh.TrangThai == 4);
                    break;
                case "all":
                default:
                    ordersQuery = ordersQuery;
                    break;
            }
       
            var orders = ordersQuery.Select(dh => new DonHangViewModel
            {
                MaDonHang = dh.MaDonHang,
                MaKhachHang = dh.MaKhachHang,
                NgayDatHang = dh.NgayDatHang,
                TongTien = dh.TongTien,
                TrangThai = dh.TrangThai ?? 0,
                ChiTietDonHangs = dh.ChiTietDonHangs.Select(ct => new ChiTietDonHangViewModel
                {
                    MaSanPham = ct.MaSanPham,
                    SoLuong = ct.SoLuong,
                    Gia = ct.Gia,
                    TenSanPham = ct.MaSanPhamNavigation != null ? ct.MaSanPhamNavigation.TenSanPham : "Sản phẩm không xác định",
                    UrlAnhSanPham = ct.MaSanPhamNavigation != null && ct.MaSanPhamNavigation.AnhSanPhams.FirstOrDefault() != null
                        ? ct.MaSanPhamNavigation.AnhSanPhams.FirstOrDefault().UrlAnh
                        : "/images/default-product.jpg"
                }).ToList()
            }).ToPagedList(page, pageSize); // Dùng phiên bản đồng bộ

            if (!string.IsNullOrEmpty(searchTerm))
            {
                ordersQuery = ordersQuery.Where(dh => dh.MaDonHang.Contains(searchTerm) || dh.KhachHang.TenKhachHang.Contains(searchTerm));
            }


            ViewBag.TrangThai = trangThai;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;

            return View(orders);

        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(string maDonHang, int trangThai)
        {
            var order = await _context.DonHangs
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

            if (order != null)
            {
                order.TrangThai = trangThai;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Trạng thái đơn hàng đã được cập nhật thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
            }

            return RedirectToAction("Index");
        }

        // Hành động hoàn trả đơn hàng
        [HttpPost]
        public async Task<IActionResult> Refund(string maDonHang)
        {
            var order = await _context.DonHangs
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

            if (order != null)
            {
                if (order.TrangThai == 2)  // Kiểm tra trạng thái "Đã giao"
                {
                    order.TrangThai = 4;  // Đặt trạng thái "Hoàn trả"
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Đơn hàng đã được hoàn trả thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Đơn hàng không thể hoàn trả vì chưa được giao!";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
            }

            return RedirectToAction("Index");
        }

        // Hành động lấy chi tiết đơn hàng
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(string maDonHang)
        {
            // Truy vấn đơn hàng từ cơ sở dữ liệu cùng với chi tiết sản phẩm
            var order = await _context.DonHangs
                .Include(dh => dh.ChiTietDonHangs) // Bao gồm các chi tiết đơn hàng
                .ThenInclude(ct => ct.MaSanPhamNavigation) // Bao gồm thông tin sản phẩm
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang); // Tìm theo mã đơn hàng

            // Kiểm tra nếu không tìm thấy đơn hàng
            if (order == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy đơn hàng
            }

            // Trả về Partial View chứa chi tiết đơn hàng
            return PartialView("_OrderDetails", order);
        }

    }
}
