using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories.Document
{
	public interface IDocumentOrganizationCounterRepository
	{
		/// <summary>
		/// Получить максимальный счетчик в выбранном году
		/// </summary>
		/// <param name="unitOfWork">IUnitOfWork</param>
		/// <param name="date">Выбранная дата</param>
		/// <param name="organizationEntity">Организация</param>
		/// <returns>Сущность (null если записи нет)</returns>
		DocumentOrganizationCounter GetMaxDocumentOrganizationCounterOnYear(
			IUnitOfWork unitOfWork,
			DateTime date, 
			OrganizationEntity organizationEntity);

		/// <summary>
		/// Получить максимальный счетчик в выбранном году (async)
		/// </summary>
		/// <param name="unitOfWork">IUnitOfWork</param>
		/// <param name="date">Выбранная дата</param>
		/// <param name="organizationEntity">Организация</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Сущность (null если записи нет)</returns>
		Task<DocumentOrganizationCounter> GetMaxDocumentOrganizationCounterOnYearAsync(
			IUnitOfWork unitOfWork,
			DateTime date,
			OrganizationEntity organizationEntity,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получить счетчик привязанный к заказу
		/// </summary>
		/// <param name="unitOfWork">IUnitOfWork</param>
		/// <param name="order">Заказ</param>
		/// <returns>Сущность (null если записи нет)</returns>
		DocumentOrganizationCounter GetDocumentOrganizationCounterByOrder(
			IUnitOfWork unitOfWork,
			OrderEntity order,
			int organizationId);

		/// <summary>
		/// Получить номер документа по номеру заказа
		/// </summary>
		/// <param name="unitOfWork">IUnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Номер документа</returns>
		Task<string> GetDocumentNumberByOrderId(IUnitOfWork unitOfWork, int orderId, CancellationToken cancellationToken);

		/// <summary>
		/// Получить словарь
		/// </summary>
		/// <param name="unitOfWork">IUnitOfWork</param>
		/// <param name="orderIds">Идентификаторы заказов</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Словарь: ключ - идентификатор заказа, значение - домер документа (при наличии)</returns>
		Task<Dictionary<int, string>> GetDocumentNumbersByOrderIds(IUnitOfWork unitOfWork, IEnumerable<int> orderIds, CancellationToken cancellationToken);
	}
}
