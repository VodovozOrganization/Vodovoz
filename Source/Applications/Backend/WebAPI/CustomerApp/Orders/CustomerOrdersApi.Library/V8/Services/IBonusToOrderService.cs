using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V8.Dto.Sale;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <summary>
	/// Расчёт баланса, доступного списания и начисления бонусов при оформлении заказа
	/// </summary>
	public interface IBonusToOrderService
	{
		/// <summary>
		/// Рассчитывает бонусы по заказу. Ничего не сохраняет
		/// </summary>
		/// <param name="request">Запрос с корзиной и суммой списания</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат расчёта бонусов и позиций заказа</returns>
		/// <exception cref="BonusToOrderValidationException">Запрос не прошёл валидацию</exception>
		Task<ApplyBonusToOrderResponse> CalculateAsync(
			ApplyBonusToOrderRequest request,
			CancellationToken cancellationToken);
	}
}
