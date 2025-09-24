using EdoService.Library.Dto;
using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Core.Domain.Clients;

namespace EdoService.Library.Services
{
	public interface IContactListService
	{
		Task<ContactList> CheckContragentAsync(IUnitOfWork uow, int organizationId, string inn, string kpp);
		Task<ContactList> GetContactListUpdatesAsync(IUnitOfWork uow, int organizationId, DateTime dateLastRequest, ContactStateCode? status = null);
		Task<ResultDto> SendContactsAsync(
			IUnitOfWork uow, int organizationId, string inn, string kpp, string email, string edxClientId, string organizationName);
		Task<ResultDto> SendContactsAsync(IUnitOfWork uow, int organizationId, ContactList invitationsList);
		ConsentForEdoStatus ConvertStateToConsentForEdoStatus(ContactStateCode stateCode);
		Task<ResultDto> SendContactsForManualInvitationAsync(
			IUnitOfWork uow,
			int organizationId,
			string inn,
			string kpp,
			string organizationName,
			string operatorId,
			string email,
			string scanFileName,
			byte[] scanFile);

		Task<ContactListItem> GetLastChangeOnDate(
			IUnitOfWork uow,
			int organizationId,
			DateTime dateLastRequest,
			string inn,
			string kpp,
			ContactStateCode? status = null);
	}
}
