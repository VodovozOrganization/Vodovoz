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
		/// <summary>
		/// Получение списка заказов самовывоза для склада
		/// </summary>
		/// <param name="warehouseId">Id склада</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result<IEnumerable<Order>>> GetSelfDeliveryOrders(int warehouseId, CancellationToken cancellationToken);
		/// <summary>
		/// Отправка запроса в ЭДО по заказу с самовывозом
		/// </summary>
		/// <param name="selfDeliveryDocument">Документ самовывоза</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> SendEdoRequest(SelfDeliveryDocument selfDeliveryDocument, CancellationToken cancellationToken);
		/// <summary>
		/// Установка количества тары, которую нужно вернуть, в документе самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocument">Документ самовывоза</param>
		/// <param name="tareToReturn">Количество тары, которую нужно вернуть</param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> SetTareToReturn(SelfDeliveryDocument selfDeliveryDocument, int tareToReturn);
	}
}
