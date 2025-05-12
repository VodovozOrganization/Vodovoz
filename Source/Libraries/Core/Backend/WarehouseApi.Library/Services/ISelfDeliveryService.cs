using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
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
		/// Получение информацию о документе отпуска самовывоза по идентификатору заказа самовывоза
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> GetSelfDeliveryDocumentByOrderId(int? orderId);

		/// <summary>
		/// Создание документа самовывоза
		/// </summary>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> CreateDocument(int orderId, int warehouseId);

		Task<Result<bool>> AddCodes(IEnumerable<string> codesToAdd);
		Task<Result<bool>> ChangeCodes(IEnumerable<string> codesToChange);
		Task<Result<bool>> RemoveCodes(IEnumerable<string> codesToDelete);
		Task<Result<bool>> EndLoad();
	}
}
