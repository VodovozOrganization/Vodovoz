using Vodovoz.Domain.Orders;

namespace Vodovoz.Services
{
    public interface ISmsPaymentServiceParametersProvider
    {
        /// <summary>
        /// Возвращает Id места, откуда проведена оплата (<see cref="PaymentFrom"/>) равное оплате по Sms
        /// </summary>
        int GetSmsPaymentByCardFromId { get; }
    }
}