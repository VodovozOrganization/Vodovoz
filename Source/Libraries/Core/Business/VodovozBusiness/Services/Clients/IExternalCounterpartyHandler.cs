using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;

namespace VodovozBusiness.Services.Clients
{
	public interface IExternalCounterpartyHandler
	{
		bool HasExternalCounterparties(IUnitOfWork uow, Phone phone);
	}
}
