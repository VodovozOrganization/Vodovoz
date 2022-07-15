using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.EntityRepositories
{
	public interface IEmailRepository
	{
		List<StoredEmail> GetAllEmailsForOrder(IUnitOfWork uow, int orderId);
		List<CounterpartyEmail> GetEmailsForPreparingOrderDocuments(IUnitOfWork uow);
		StoredEmail GetStoredEmailByMessageId(IUnitOfWork uow, string messageId);
		bool HaveSendedEmailForBill(int orderId);
		bool CanSendByTimeout(string address, int orderId, OrderDocumentType type);
		int GetCurrentDatabaseId(IUnitOfWork uow);
		int GetCounterpartyIdByEmailGuidForUnsubscribing(IUnitOfWork uow, Guid emailGuid);
		IList<UnsubscribingReason> GetUnsubscribingReasons(IUnitOfWork uow);

		#region EmailType

		IList<EmailType> GetEmailTypes(IUnitOfWork uow);
		EmailType EmailTypeWithPurposeExists(IUnitOfWork uow, EmailPurpose emailPurpose);
		StoredEmail GetById(IUnitOfWork unitOfWork, int id);

		#endregion
	}
}
