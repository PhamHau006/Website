namespace ASM_GS.ViewModels
{
    public class CheckoutViewModel
    {
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public string Email { get; set; }

        public decimal Total => CartItems.Sum(i => i.Price * i.Quantity);
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal { get; set; }  // Make it settable

        public string DiscountCode { get; set; }
        public string PaymentMethod { get; set; }

        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
    }
}