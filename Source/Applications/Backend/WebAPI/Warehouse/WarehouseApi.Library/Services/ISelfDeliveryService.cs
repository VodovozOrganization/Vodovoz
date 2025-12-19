using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;

namespace WarehouseApi.Library.Services
{
	public interface ISelfDeliveryService
	{
		/// <summary>
		/// Получение информацию о документе отпуска самовывоза по идентификатору документа отпуска самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocumentId"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocumentEntity>> GetSelfDeliveryDocumentById(int selfDeliveryDocumentId, CancellationToken cancellationToken);

		/// <summary>
		/// Получение информацию о документе отпуска самовывоза по идентификатору заказа самовывоза
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocumentEntity>> GetSelfDeliveryDocumentByOrderId(int orderId, CancellationToken cancellationToken);

		/// <summary>
		/// Создание документа самовывоза
		/// </summary>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocumentEntity>> CreateDocument(Employee author, int orderId, int warehouseId, CancellationToken cancellationToken);

		/// <summary>
		/// Добавление кодов маркировки честного знака самовывоза в документ самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocument"></param>
		/// <param name="codesToAdd"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocumentEntity>> AddCodes(SelfDeliveryDocumentEntity selfDeliveryDocument, IEnumerable<string> codesToAdd, CancellationToken cancellationToken);

		/// <summary>
		/// Изменение кодов маркировки честного знака самовывоза в документе самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocument"></param>
		/// <param name="codesToChange"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocumentEntity>> ChangeCodes(SelfDeliveryDocumentEntity selfDeliveryDocument, IDictionary<string, string> codesToChange, CancellationToken cancellationToken);

		/// <summary>
		/// Удаление кодов маркировки честного знака самовывоза в документе самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocument"></param>
		/// <param name="codesToDelete"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocumentEntity>> RemoveCodes(SelfDeliveryDocumentEntity selfDeliveryDocument, IEnumerable<string> codesToDelete, CancellationToken cancellationToken);

		/// <summary>
		/// Завершение отгрузки документа самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocument"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocumentEntity>> EndLoad(SelfDeliveryDocumentEntity selfDeliveryDocument, CancellationToken cancellationToken);
		Task<Result<IEnumerable<OrderEntity>>> GetSelfDeliveryOrders(int warehouseId, CancellationToken cancellationToken);
	}
}
