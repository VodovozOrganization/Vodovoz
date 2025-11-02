using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Tools.Orders
{
	public class EdoContainerMainDocumentIdParser : IEdoContainerMainDocumentIdParser
	{
		private readonly ICounterpartyRepository _counterpartyRepository;

		public EdoContainerMainDocumentIdParser(ICounterpartyRepository counterpartyRepository)
		{
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
		}

		/// <summary>
		/// Получение клиента, который отправил документы по ЭДО
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="mainDocumentId">Id главного документа в контейнере(наименование)</param>
		/// <param name="isIncoming">входящий или исходящий контейнер</param>
		/// <returns>Клиент, кто отправил документы</returns>
		public Counterparty GetCounterpartyFromMainDocumentId(IUnitOfWork uow, string mainDocumentId, bool isIncoming = true)
		{
			var mainDocumentIdParts = mainDocumentId.Split('_');
			var edxClientId = isIncoming ? mainDocumentIdParts[3] : mainDocumentIdParts[2];

			var client = _counterpartyRepository.GetCounterpartyByPersonalAccountIdInEdo(uow, edxClientId);
			
			return client;
		}
	}
}
