using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Controllers.ContactsForExternalCounterparty
{
	public class ContactFinderForExternalCounterpartyFromTwo : ContactFinderForExternalCounterpartyFromMany
	{
		public override FoundContact FindContact(IList<Phone> contacts)
		{
			if(contacts.Count == 2)
			{
				var processedContacts = ProcessingContacts(contacts);

				if(processedContacts.CountCounterparties != 1 || processedContacts.CountDeliveryPoints != 1)
				{
					return NeedManualHandlingFoundContact();
				}

				if(processedContacts.SumCounterpartiesIds != processedContacts.SumDeliveryPointsCounterpartiesIds)
				{
					return NeedManualHandlingFoundContact();
				}

				if(processedContacts.CounterpartyPhone.Counterparty.PersonType == PersonType.legal)
				{
					return NeedManualHandlingFoundContact();
				}
				
				return new FoundContact
				{
					Phone = processedContacts.CounterpartyPhone,
					FoundContactStatus = FoundContactStatus.Success
				};
			}

			if(NextHandler != null)
			{
				return NextHandler.FindContact(contacts);
			}
			
			return NeedManualHandlingFoundContact();
		}
	}
}
