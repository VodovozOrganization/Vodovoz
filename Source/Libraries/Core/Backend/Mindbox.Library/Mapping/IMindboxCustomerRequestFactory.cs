using Mindbox.Library.Contracts;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;

namespace Mindbox.Library.Mapping
{
	/// <summary>
	/// Формирует тело запроса на создание/обновление клиента в Mindbox
	/// </summary>
	public interface IMindboxCustomerRequestFactory
	{
		MindboxCustomerRequest Create(IUnitOfWork unitOfWork, CounterpartyEntity counterparty);
	}
}
