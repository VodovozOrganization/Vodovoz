using Bitrix.DTO;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace BitrixIntegration.Processors
{
	public interface IDeliveryPointProcessor
	{
		DeliveryPoint ProcessDeliveryPoint(IUnitOfWork uow, Deal deal, Counterparty counterparty);
	}
}