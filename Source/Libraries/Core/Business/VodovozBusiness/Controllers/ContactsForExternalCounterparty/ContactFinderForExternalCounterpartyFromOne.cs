using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Controllers.ContactsForExternalCounterparty
{
	public class ContactFinderForExternalCounterpartyFromOne : ContactsFinderForExternalCounterparty, IContactFinderForExternalCounterparty
	{
		public FoundContact FindContact(IList<Phone> contacts)
		{
			if(contacts.Count == 1)
			{
				if(contacts[0].Counterparty?.PersonType == PersonType.natural)
				{
					return new FoundContact
					{
						Phone = contacts[0],
						FoundContactStatus = FoundContactStatus.Success
					};
				}

				return NeedManualHandlingFoundContact();
			}

			if(NextHandler != null)
			{
				return NextHandler.FindContact(contacts);
			}
			
			return NeedManualHandlingFoundContact();
		}
	}
}
