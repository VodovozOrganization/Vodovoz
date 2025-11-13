using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Controllers.ContactsForExternalCounterparty
{
	public class ContactFinderForExternalCounterpartyFromMany : ContactsFinderForExternalCounterparty, IContactFinderForExternalCounterparty
	{
		public virtual FoundContact FindContact(IList<Phone> contacts)
		{
			if(contacts.Count > 2)
			{
				var processedContacts = ProcessingContacts(contacts);

				if(processedContacts.CountCounterparties != 1 || processedContacts.CountDeliveryPoints <= 1)
				{
					return NeedManualHandlingFoundContact();
				}

				if(processedContacts.SumCounterpartiesIds !=
					processedContacts.SumDeliveryPointsCounterpartiesIds / processedContacts.CountDeliveryPoints)
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
		
		protected ProcessedContacts ProcessingContacts(IList<Phone> contacts)
		{
			var processedContacts = new ProcessedContacts();
			
			for(var i = 0; i < contacts.Count; i++)
			{
				if(contacts[i].Counterparty != null)
				{
					processedContacts.CountCounterparties++;
					processedContacts.SumCounterpartiesIds += contacts[i].Counterparty.Id;
					processedContacts.CounterpartyPhone = contacts[i];
				}
				else if(contacts[i].DeliveryPoint != null)
				{
					processedContacts.CountDeliveryPoints++;
					processedContacts.SumDeliveryPointsCounterpartiesIds += contacts[i].DeliveryPoint.Counterparty.Id;
				}
			}

			return processedContacts;
		}
	}
}
