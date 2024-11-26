using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASM_GS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartAPIController> _logger;

        public CartAPIController(ApplicationDbContext context, ILogger<CartAPIController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("AddComboToCart")]
        public async Task<IActionResult> AddComboToCart([FromBody] AddComboRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.ComboId) || request.Quantity <= 0)
                {
                    return BadRequest(new { success = false, message = "Thông tin combo hoặc số lượng không hợp lệ." });
                }

                var maKhachHang = HttpContext.Session.GetString("User");
                if (string.IsNullOrEmpty(maKhachHang))
                {
                    return BadRequest(new { success = false, message = "Vui lòng đăng nhập để thêm combo vào giỏ hàng." });
                }

                var gioHang = await GetOrCreateCartAsync(maKhachHang);

                var combo = await _context.Combos
                    .Include(c => c.ChiTietCombos)
                    .ThenInclude(ct => ct.MaSanPhamNavigation)
                    .FirstOrDefaultAsync(c => c.MaCombo == request.ComboId);

                if (combo == null)
                {
                    return NotFound(new { success = false, message = "Combo không tồn tại." });
                }

                foreach (var chiTiet in combo.ChiTietCombos)
                {
                    var sanPham = chiTiet.MaSanPhamNavigation;
                    if (sanPham == null || sanPham.SoLuong < chiTiet.SoLuong * request.Quantity)
                    {
                        return BadRequest(new { success = false, message = $"Sản phẩm '{sanPham?.TenSanPham}' trong combo không đủ số lượng." });
                    }
                }

                var cartItem = gioHang.ChiTietGioHangs.FirstOrDefault(ct => ct.MaCombo == request.ComboId);

                if (cartItem != null)
                {
                    cartItem.SoLuong += request.Quantity;
                }
                else
                {
                    cartItem = new ChiTietGioHang
                    {
                        MaGioHang = gioHang.MaGioHang,
                        MaCombo = request.ComboId, // Gán MaCombo cho combo
                        SoLuong = request.Quantity
                    };
                    _context.ChiTietGioHangs.Add(cartItem);
                }
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Combo đã được thêm vào giỏ hàng." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xử lý yêu cầu của bạn." });
            }
        }



        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ProductId) || request.Quantity <= 0)
            {
                return BadRequest(new { success = false, message = "Thông tin sản phẩm hoặc số lượng không hợp lệ." });
            }

            var maKhachHang = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(maKhachHang))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng." });
            }

            try
            {
                var gioHang = await GetOrCreateCartAsync(maKhachHang);

                // Lấy sản phẩm từ cơ sở dữ liệu
                var product = await _context.SanPhams.FirstOrDefaultAsync(p => p.MaSanPham == request.ProductId);
                if (product == null)
                {
                    return NotFound(new { success = false, message = "Sản phẩm không tồn tại." });
                }

                // Kiểm tra số lượng yêu cầu có vượt quá tồn kho
                var cartItem = gioHang.ChiTietGioHangs.FirstOrDefault(ct => ct.MaSanPham == request.ProductId);
                var totalQuantity = (cartItem?.SoLuong ?? 0) + request.Quantity;
                if (totalQuantity > product.SoLuong)
                {
                    return BadRequest(new { success = false, message = $"Số lượng yêu cầu vượt quá tồn kho ({product.SoLuong})." });
                }

                if (cartItem != null)
                {
                    cartItem.SoLuong += request.Quantity;
                    _context.ChiTietGioHangs.Update(cartItem);
                }
                else
                {
                    cartItem = new ChiTietGioHang
                    {
                        MaGioHang = gioHang.MaGioHang,
                        MaSanPham = request.ProductId,
                        SoLuong = request.Quantity
                    };
                    _context.ChiTietGioHangs.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Sản phẩm đã được thêm vào giỏ hàng." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm vào giỏ hàng.");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xử lý yêu cầu của bạn." });
            }
        }

        [HttpPost("UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequest request)
        {
            if (request.Quantity <= 0)
            {
                return BadRequest(new { success = false, message = "Số lượng phải lớn hơn 0." });
            }

            try
            {
                var cartItem = await _context.ChiTietGioHangs.FindAsync(request.Id);
                if (cartItem == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
                }

                // Kiểm tra tồn kho
                var product = await _context.SanPhams.FirstOrDefaultAsync(p => p.MaSanPham == cartItem.MaSanPham);
                if (product == null || request.Quantity > product.SoLuong)
                {
                    return BadRequest(new { success = false, message = $"Số lượng không được vượt quá tồn kho ({product?.SoLuong ?? 0})." });
                }

                // Cập nhật số lượng
                cartItem.SoLuong = request.Quantity;
                _context.ChiTietGioHangs.Update(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Số lượng đã được cập nhật." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật số lượng sản phẩm.");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xử lý yêu cầu của bạn." });
            }
        }

        [HttpDelete("RemoveItem/{id}")]
        public async Task<IActionResult> RemoveItem(int id)
        {
            try
            {
                var cartItem = await _context.ChiTietGioHangs.FindAsync(id);
                if (cartItem == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
                }

                _context.ChiTietGioHangs.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Sản phẩm đã được xóa khỏi giỏ hàng." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm khỏi giỏ hàng.");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xử lý yêu cầu của bạn." });
            }
        }

        private async Task<GioHang> GetOrCreateCartAsync(string maKhachHang)
        {
            var gioHang = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    MaGioHang = "GH" + Guid.NewGuid().ToString("N"),
                    MaKhachHang = maKhachHang,
                    NgayTao = DateOnly.FromDateTime(DateTime.Today),
                };
                _context.GioHangs.Add(gioHang);
                await _context.SaveChangesAsync();
            }

            return gioHang;
        }
    }

    public class CartItemRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateQuantityRequest
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }

    public class AddComboRequest
    {
        public string ComboId { get; set; }
        public int Quantity { get; set; }
    }
}
