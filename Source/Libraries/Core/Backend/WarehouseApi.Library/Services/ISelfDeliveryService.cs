﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
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
		/// Изменение кодов маркировки честного знака самовывоза в документе самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocument"></param>
		/// <param name="codesToChange"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> ChangeCodes(SelfDeliveryDocument selfDeliveryDocument, IDictionary<string, string> codesToChange, CancellationToken cancellationToken);

		/// <summary>
		/// Удаление кодов маркировки честного знака самовывоза в документе самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocument"></param>
		/// <param name="codesToDelete"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> RemoveCodes(SelfDeliveryDocument selfDeliveryDocument, IEnumerable<string> codesToDelete, CancellationToken cancellationToken);

		/// <summary>
		/// Завершение отгрузки документа самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocument"></param>
		/// <returns></returns>
		Task<Result<SelfDeliveryDocument>> EndLoad(SelfDeliveryDocument selfDeliveryDocument, CancellationToken cancellationToken);
	}
}
