using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace WarehouseApi.Library.Services
{
	public interface ISelfDeliveryService
	{
		/// <summary>
		/// Получение информацию о документе отпуска самовывоза по идентификатору документа отпуска самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocumentId"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> GetSelfDeliveryDocumentById(int selfDeliveryDocumentId, CancellationToken cancellationToken);

		/// <summary>
		/// Получение информацию о документе отпуска самовывоза по идентификатору заказа самовывоза
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> GetSelfDeliveryDocumentByOrderId(int orderId, CancellationToken cancellationToken);

		/// <summary>
		/// Создание документа самовывоза
		/// </summary>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> CreateDocument(Employee author, int orderId, int warehouseId, CancellationToken cancellationToken);

		/// <summary>
		/// Добавление кодов маркировки честного знака самовывоза в документ самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocument"></param>
		/// <param name="codesToAdd"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> AddCodes(SelfDeliveryDocument selfDeliveryDocument, IEnumerable<string> codesToAdd, CancellationToken cancellationToken);

		/// <summary>
		/// Завершение отгрузки документа самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocument"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> EndLoad(SelfDeliveryDocument selfDeliveryDocument, CancellationToken cancellationToken);
		Task<Result<IEnumerable<Order>>> GetSelfDeliveryOrders(int warehouseId, CancellationToken cancellationToken);
	}
}
