using Bitrix.DTO;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace BitrixIntegration.Processors
{
	public interface ICounterpartyProcessor
	{
		Counterparty ProcessCounterparty(IUnitOfWork uow, Deal deal);
	}
}
