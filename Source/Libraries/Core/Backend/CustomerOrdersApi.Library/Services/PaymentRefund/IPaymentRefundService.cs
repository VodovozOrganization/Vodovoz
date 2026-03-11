using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	public interface IPaymentRefundService
	{
		/// <summary>
		/// Может ли сервис обработать возврат для данного источника оплаты
		/// </summary>
		/// <param name="paymentSource"></param>
		/// <returns>true, если сервис может обработать возврат для указанного источника оплаты, иначе - false</returns>
		bool CanHandle(OnlinePaymentSource paymentSource);

		/// <summary>
		/// Обработка возврата
		/// </summary>
		/// <param name="request">Запрос на возврат</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат операции возврата средств в виде объекта <see cref="RefundResultDto"/></returns>
		Task<RefundResultDto> ProcessRefundAsync(RefundRequestDto request, CancellationToken cancellationToken);
	}
}
