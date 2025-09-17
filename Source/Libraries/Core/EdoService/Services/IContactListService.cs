using EdoService.Library.Dto;
using System;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Core.Domain.Clients;

namespace EdoService.Library.Services
{
	public interface IContactListService
	{
		void SetOrganizationId(int organizationId);
		Task<ContactList> CheckContragentAsync(IUnitOfWork uow, string inn, string kpp);
		Task<ContactList> GetContactListUpdatesAsync(IUnitOfWork uow, DateTime dateLastRequest, ContactStateCode? status = null);
		Task<ResultDto> SendContactsAsync(IUnitOfWork uow, string inn, string kpp, string email, string edxClientId, string organizationName);
		Task<ResultDto> SendContactsAsync(IUnitOfWork uow, ContactList invitationsList);
		ConsentForEdoStatus ConvertStateToConsentForEdoStatus(ContactStateCode stateCode);
		Task<ResultDto> SendContactsForManualInvitationAsync(
			IUnitOfWork uow,
			string inn,
			string kpp,
			string organizationName,
			string operatorId,
			string email,
			string scanFileName,
			byte[] scanFile);

		Task<ContactListItem> GetLastChangeOnDate(
			IUnitOfWork uow,
			DateTime dateLastRequest,
			string inn,
			string kpp,
			ContactStateCode? status = null);
	}
}
