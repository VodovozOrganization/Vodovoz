using QS.DomainModel.UoW;
using Vodovoz.Controllers.ContactsForExternalCounterparty;

namespace Vodovoz.Controllers
{
	public interface IContactManagerForExternalCounterparty
	{
		FoundContact FindContactForRegisterExternalCounterparty(IUnitOfWork uow, string phoneNumber);
	}
}
