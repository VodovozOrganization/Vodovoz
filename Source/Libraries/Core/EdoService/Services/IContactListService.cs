using EdoService.Library.Dto;
using System;
using System.Threading.Tasks;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Domain.Client;

namespace EdoService.Library.Services
{
	public interface IContactListService
	{
		Task<ContactList> CheckContragentAsync(string inn, string kpp);
		Task<ContactList> GetContactListUpdatesAsync(DateTime dateLastRequest, ContactStateCode? status = null);
		Task<ResultDto> SendContactsAsync(string inn, string kpp, string email, string edxClientId);
		Task<ResultDto> SendContactsAsync(ContactList invitationsList);
		ConsentForEdoStatus ConvertStateToConsentForEdoStatus(ContactStateCode stateCode);
		Task<ResultDto> SendContactsForManualInvitationAsync(string inn, string kpp, string organizationName, string operatorId, string email,
			string scanFileName, byte[] scanFile);
	}
}
