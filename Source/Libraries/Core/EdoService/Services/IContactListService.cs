using EdoService.Dto;
using System;
using System.Threading.Tasks;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Domain.Client;

namespace EdoService.Services
{
	public interface IContactListService
	{
		Task<ContactList> CheckContragentAsync(string inn, string kpp);
		Task<ContactList> GetContactListUpdatesAsync(DateTime dateLastRequest, ContactStateCode? status = null);
		Task<ResultDto> SendContactsAsync(string inn, string kpp, string email);
		ConsentForEdoStatus ConvertStateToConsentForEdoStatus(ContactStateCode stateCode);
	}
}
