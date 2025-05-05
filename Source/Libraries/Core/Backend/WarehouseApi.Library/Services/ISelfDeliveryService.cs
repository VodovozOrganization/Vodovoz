using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Errors;
using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Library.Services
{
	public interface ISelfDeliveryService
	{
		/// <summary>
		/// Получение информацию о заказе самовывоза
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		Result<OrderDto> GetSelfDeliveryOrder(int orderId);

		/// <summary>
		/// Добавление кода ЧЗ в заказ самовывоза
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="scannedCode">Сканированный код</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> AddTrueMarkCode(int orderId, string scannedCode, CancellationToken cancellationToken);
	}
}
