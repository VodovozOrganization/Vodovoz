using CustomerOrdersApi.Library.Dto.Orders;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Library.Services
{
	/// <summary>
	/// Сервис для отмены заказов из ИПЗ
	/// </summary>
	public interface IOrderCancellationService
	{
		/// <summary>
		/// Выполняет отмену заказа
		/// </summary>
		/// <param name="cancelOrderDto">DTO с данными для отмены</param>
		/// <returns>Результат операции отмены</returns>
		Task<CancelOrderResult> CancelOrderAsync(CancelOrderDto cancelOrderDto);
	}
}
