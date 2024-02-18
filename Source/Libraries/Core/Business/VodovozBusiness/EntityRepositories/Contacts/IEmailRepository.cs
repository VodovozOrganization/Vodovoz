using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.EntityRepositories
{
	public interface IEmailRepository
	{
		List<StoredEmail> GetAllEmailsForOrder(IUnitOfWork uow, int orderId);
		List<CounterpartyEmail> GetEmailsForPreparingOrderDocuments(IUnitOfWork uow);
		StoredEmail GetStoredEmailByMessageId(IUnitOfWork uow, string messageId);
		bool HaveSendedEmailForBill(int orderId);
		bool HasSendedEmailForUpd(int orderId);
		bool NeedSendDocumentsByEmailOnFinish(IUnitOfWork uow, Order order, IDeliveryScheduleSettings deliveryScheduleParametersProvider);
		bool CanSendByTimeout(string address, int orderId, OrderDocumentType type);
		int GetCurrentDatabaseId(IUnitOfWork uow);
		int GetCounterpartyIdByEmailGuidForUnsubscribing(IUnitOfWork uow, Guid emailGuid);
		IList<BulkEmailEventReason> GetUnsubscribingReasons(IUnitOfWork uow, IEmailSettings emailParametersProvider, bool isForUnsubscribePage = false);
		BulkEmailEvent GetLastBulkEmailEvent(IUnitOfWork uow, int counterpartyId);
		BulkEmailEventReason GetBulkEmailEventOtherReason(IUnitOfWork uoW, IEmailSettings emailParametersProvider);
		BulkEmailEventReason GetBulkEmailEventOperatorReason(IUnitOfWork uoW, IEmailSettings emailParametersProvider);
		Email GetEmailForExternalCounterparty(IUnitOfWork uow, int counterpartyId);

		#region EmailType

		IList<EmailType> GetEmailTypes(IUnitOfWork uow);
		EmailType GetEmailTypeForReceipts(IUnitOfWork uow);
		EmailType EmailTypeWithPurposeExists(IUnitOfWork uow, EmailPurpose emailPurpose);
		StoredEmail GetById(IUnitOfWork unitOfWork, int id);

		#endregion
	}
}
