using System;
using System.Threading.Tasks;
using TISystems.TTC.CRM.BE.Serialization;

namespace EdoApi.Library.Services
{
	public interface IContactListService
	{
		Task<ContactList> CheckContragentAsync(byte[] contacts, string assistantKey);
		Task SendContactsAsync(byte[] contacts, string assistantKey);
		Task<ContactList> GetContactListUpdatesAsync(DateTime dateLastRequest, string assistantKey, ContactStateCode? status);
	}
}
