using ASM_GS.Utils;
using ASM_GS.ViewModels;

namespace ASM_GS.Services
{
    public interface IVnPayServices
    {
        string CreatePaymentUrl( HttpContext context, VnPaymentRequestModel model );
        VnPaymentResponseModel PaymentExcute(IQueryCollection collections);
    }
}
