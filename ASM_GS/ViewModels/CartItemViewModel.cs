namespace ASM_GS.ViewModels
{
    public class CartItemViewModel
    {
        public int ItemId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public decimal Subtotal => Price * Quantity;
        public int MaxQuantity { get; set; }
    }
    public static class PaymentType
    {
        public static string COD = "COD";
        public static string VNPAY = "VnPay";
        public static string MOMO = "MoMo";
    }
}