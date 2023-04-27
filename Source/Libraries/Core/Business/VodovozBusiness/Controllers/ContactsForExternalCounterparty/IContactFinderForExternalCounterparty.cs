using System.Collections.Generic;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Controllers.ContactsForExternalCounterparty
{
	public interface IContactFinderForExternalCounterparty
	{
		void SetNextHandler(IContactFinderForExternalCounterparty nextContactFinder);
		FoundContact FindContact(IList<Phone> contacts);
	}
}
