using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface IExternalCounterpartyMatchingRepository
	{
		bool ExternalCounterpartyMatchingExists(IUnitOfWork uow, Guid externalCounterpartyGuid, string phoneNumber);
		IEnumerable<ExternalCounterpartyMatching> GetExternalCounterpartyMatching(
			IUnitOfWork uow, Guid externalCounterpartyGuid, string phoneNumber);

		/// <summary>Получить связанные заявки и несопоставленные заявки с совпадающей парой внешнего идентификатора и источника.</summary>
		/// <param name="uow">Текущая единица работы.</param>
		/// <param name="externalCounterpartyIds">Идентификаторы выбранных пользователей ИПЗ в ERP.</param>
		/// <returns>Заявки без дубликатов. Номер несопоставленной заявки дополнительно проверяет вызывающий сервис.</returns>
		IList<ExternalCounterpartyMatching> GetForExternalCounterparties(
			IUnitOfWork uow, IEnumerable<int> externalCounterpartyIds);
	}
}
