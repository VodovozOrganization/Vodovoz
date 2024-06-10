using EdoService.Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TISystems.TTC.CRM.BE.Serialization;

namespace EdoService.Library
{
	public class ContactListParser
	{
		public async Task<ContactListItem> GetLastChangeOnDate(IContactListService contactListService, DateTime dateLastRequest, string inn, string kpp, ContactStateCode? status = null)
		{
			List<ContactListItem> items = new List<ContactListItem>();
			ContactList contactList;
			var date = dateLastRequest;

			do
			{
				contactList = await contactListService.GetContactListUpdatesAsync(date, status);

				if(contactList.Contacts != null && contactList.Contacts.LastOrDefault() is ContactListItem item)
				{
					date = item.State.Changed;
					items.AddRange(contactList.Contacts);
				}

			} while(contactList.Contacts != null && contactList.Contacts.Length >= 100);


			return items
				.Where(x => x.Inn == inn
							&& (string.IsNullOrWhiteSpace(x.Kpp) || x.Kpp == kpp))
				.OrderByDescending(x => x.State.Changed)
				.FirstOrDefault();
		}
	}
}
