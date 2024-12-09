namespace ASM_GS.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal Total => Items.Sum(i => i.Subtotal);
        public int TotalItems { get; set; }
    }
}