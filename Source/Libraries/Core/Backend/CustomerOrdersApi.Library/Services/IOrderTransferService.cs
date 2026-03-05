using CustomerOrdersApi.Library.Dto.Orders;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Library.Services
{
	/// <summary>
	/// Сервис для переноса заказов из ИПЗ
	/// </summary>
	public interface IOrderTransferService
	{
		/// <summary>
		/// Выполняет перенос заказа на новую дату и интервал
		/// </summary>
		/// <param name="transferOrderDto">DTO с данными для переноса</param>
		/// <returns>Результат операции переноса</returns>
		Task<TransferOrderResult> TransferOrderAsync(TransferOrderDto transferOrderDto);
	}
}
