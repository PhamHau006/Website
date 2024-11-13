using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASM_GS.ViewModels
{
    public class CheckoutViewModel
    {
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public string Email { get; set; }

        // Tổng số tiền
        public decimal Total { get; set; }

        // Phương thức thanh toán (COD hoặc VNPay)
        public string PaymentMethod { get; set; }

        // Danh sách sản phẩm trong giỏ hàng
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
    }
}
