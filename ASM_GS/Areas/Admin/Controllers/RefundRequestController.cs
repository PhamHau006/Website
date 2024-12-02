    using ASM_GS.Controllers;
    using ASM_GS.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using X.PagedList.Extensions;

    namespace ASM_GS.Areas.Admin.Controllers
    {
        [Area("Admin")]
        public class RefundRequestController : Controller
        {
            private readonly ApplicationDbContext _context;

            public RefundRequestController(ApplicationDbContext context)
            {
                _context = context;
            }

            // Hiển thị danh sách các yêu cầu hoàn trả
            public IActionResult Index(string searchTerm, string sortBy = "NgayTao", int? page = 1, int pageSize = 5, string trangThai = null)
            {
                var refundRequests = _context.RefundRequests.Include(r => r.DonHang).AsQueryable();

                // Lọc theo từ khóa tìm kiếm
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    refundRequests = refundRequests.Where(r => r.MaDonHang.Contains(searchTerm) || r.LyDo.Contains(searchTerm));
                }

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(trangThai))
                {
                    refundRequests = refundRequests.Where(r => r.TrangThai == trangThai);
                }

                // Sắp xếp theo tiêu chí
                switch (sortBy)
                {
                    case "MaDonHang":
                        refundRequests = refundRequests.OrderBy(r => r.MaDonHang);
                        break;
                    case "TrangThai":
                        refundRequests = refundRequests.OrderBy(r => r.TrangThai);
                        break;
                    case "NgayTao":
                    default:
                        refundRequests = refundRequests.OrderByDescending(r => r.NgayTao);
                        break;
                }

                // Phân trang
                int pageNumber = page ?? 1;
                var pagedRefundRequests = refundRequests.ToPagedList(pageNumber, pageSize);

                ViewBag.SearchTerm = searchTerm;
                ViewBag.SortBy = sortBy;
                ViewBag.TrangThai = trangThai;
                ViewBag.PageSize = pageSize;

                return View(pagedRefundRequests);
            }

            // Hiển thị chi tiết yêu cầu hoàn trả (cho AJAX modal)
            [HttpGet]
            public IActionResult Details(int id)
            {
                try
                {
                    var refundRequest = _context.RefundRequests
                        .Include(r => r.DonHang)
                            .ThenInclude(d => d.KhachHang)
                        .Include(r => r.DonHang.ChiTietDonHangs)
                            .ThenInclude(c => c.MaSanPhamNavigation) // Lấy thông tin sản phẩm
                        .Include(r => r.RefundRequestImages) // Lấy hình ảnh hoàn trả
                        .FirstOrDefault(r => r.Id == id);

                    if (refundRequest == null)
                    {
                        return NotFound("Không tìm thấy yêu cầu hoàn trả."); // Trả về 404 nếu không tìm thấy
                    }

                    return PartialView("_RefundRequestDetails", refundRequest);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi tải chi tiết yêu cầu hoàn trả: {ex.Message}");
                    return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu.");
                }
            }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateRefundRequest(string MaDonHang, string LyDo, List<IFormFile> Images)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(MaDonHang) || string.IsNullOrEmpty(LyDo))
                {
                    return StatusCode(400, "Thông tin không hợp lệ.");
                }

                // Kiểm tra xem yêu cầu hoàn trả đã được gửi trước đó chưa
                var existingRequest = _context.RefundRequests
                    .FirstOrDefault(r => r.MaDonHang == MaDonHang);

                if (existingRequest != null)
                {
                    return StatusCode(400, "Đã gửi yêu cầu hoàn trả trước đó.");
                }

                // Kiểm tra thời hạn hoàn trả (3 ngày)
                var order = _context.DonHangs.FirstOrDefault(o => o.MaDonHang == MaDonHang);
                if (order == null)
                {
                    return NotFound("Không tìm thấy hóa đơn.");
                }

                if ((DateTime.Now - order.NgayDatHang.ToDateTime(new TimeOnly(0, 0))).TotalDays > 3)
                {
                    return StatusCode(400, "Thời hạn yêu cầu hoàn trả đã hết.");
                }

                // Tạo mới yêu cầu hoàn trả
                var refundRequest = new RefundRequest
                {
                    MaDonHang = MaDonHang,
                    LyDo = LyDo,
                    TrangThai = "Đang chờ",
                    NgayTao = DateTime.Now
                };

                _context.RefundRequests.Add(refundRequest);
                _context.SaveChanges();

                // Lưu hình ảnh và tạo đối tượng RefundRequestImage
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/AnhHoanTra");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var refundImages = new List<RefundRequestImage>();
                foreach (var image in Images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                        var filePath = Path.Combine(uploadFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            image.CopyTo(stream);
                        }

                        refundImages.Add(new RefundRequestImage
                        {
                            ImageUrl = $"/img/AnhHoanTra/{fileName}",
                            RefundRequestId = refundRequest.Id
                        });
                    } 
                }

                if (refundImages.Any())
                {
                    _context.RefundRequestImages.AddRange(refundImages);
                    _context.SaveChanges();
                }

                return Ok("Yêu cầu hoàn trả đã được xử lý thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                return StatusCode(500, "Lỗi không xác định trên server.");
            }
        }



        [HttpPost]
        public IActionResult Approve(int id)
        {
            var refundRequest = _context.RefundRequests
                .Include(r => r.DonHang)
                .FirstOrDefault(r => r.Id == id);

            if (refundRequest == null)
            {
                return NotFound("Không tìm thấy yêu cầu hoàn trả.");
            }

            // Cập nhật trạng thái yêu cầu hoàn trả
            refundRequest.TrangThai = "Đã duyệt";

            // Cập nhật trạng thái đơn hàng thành "5 - Đã hoàn"
            if (refundRequest.DonHang != null)
            {
                refundRequest.DonHang.TrangThai = 5; // 5: "Đã hoàn"
            }

            _context.SaveChanges();

            // Lưu thông báo vào TempData
            TempData["SuccessMessage"] = "Yêu cầu hoàn trả đã được duyệt thành công và trạng thái đơn hàng đã được cập nhật.";

            return RedirectToAction("Index"); // Quay lại danh sách yêu cầu hoàn trả
        }


        [HttpPost]
        public IActionResult Reject(int id)
        {
            var refundRequest = _context.RefundRequests
                .Include(r => r.DonHang)
                .FirstOrDefault(r => r.Id == id);

            if (refundRequest == null)
            {
                return NotFound("Không tìm thấy yêu cầu hoàn trả.");
            }

            // Cập nhật trạng thái yêu cầu hoàn trả
            refundRequest.TrangThai = "Không duyệt";

            // Cập nhật trạng thái đơn hàng thành "6 - Không đồng ý hoàn"
            if (refundRequest.DonHang != null)
            {
                refundRequest.DonHang.TrangThai = 6; // 6: "Không đồng ý hoàn"
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Yêu cầu hoàn trả KHÔNG ĐƯỢC DUYỆT và trạng thái đơn hàng đã được cập nhật.";

            return RedirectToAction("Index"); // Quay lại danh sách yêu cầu hoàn trả

        }


    }
}
