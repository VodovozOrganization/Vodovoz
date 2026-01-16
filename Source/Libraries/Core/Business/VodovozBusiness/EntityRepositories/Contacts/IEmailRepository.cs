using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
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
		bool NeedSendDocumentsByEmailOnFinish(IUnitOfWork uow, Order currentOrder, IDeliveryScheduleSettings deliveryScheduleSettings, bool isForBill = false);
		bool CanSendByTimeout(string address, int orderId, OrderDocumentType type);
		int GetCurrentDatabaseId(IUnitOfWork uow);
		int GetCounterpartyIdByEmailGuidForUnsubscribing(IUnitOfWork uow, Guid emailGuid);
		IList<BulkEmailEventReason> GetUnsubscribingReasons(IUnitOfWork uow, IEmailSettings emailSettings, bool isForUnsubscribePage = false);
		BulkEmailEvent GetLastBulkEmailEvent(IUnitOfWork uow, int counterpartyId);
		BulkEmailEventReason GetBulkEmailEventOtherReason(IUnitOfWork uoW, IEmailSettings emailSettings);
		BulkEmailEventReason GetBulkEmailEventOperatorReason(IUnitOfWork uoW, IEmailSettings emailSettings);
		Email GetEmailForExternalCounterparty(IUnitOfWork uow, int counterpartyId);

		/// <summary>
		/// Получение всех просроченных заказов, сгруппированных по клиентам и организациям, для рассылки уведомлений о задолженности
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="maxClients">Максимальное количество клиентов для обработки</param>
		/// <param name="cancellationToken"></param>
		/// <returns>Словарь: клиент - список его просроченных заказов</returns>
		Task<Dictionary<Order, (Counterparty Counterparty, Organization Organization)>> GetAllOverdueOrderForDebtNotificationAsync(IUnitOfWork uow, int maxClients, CancellationToken cancellationToken);

		#region EmailType

		IList<EmailType> GetEmailTypes(IUnitOfWork uow);
		EmailType GetEmailTypeForReceipts(IUnitOfWork uow);
		/// <summary>
		/// Получение типа электронной почты Аккаунт юр лица в ИПЗ
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <returns></returns>
		EmailType GetEmailTypeForExternalAccount(IUnitOfWork uow);
		EmailType EmailTypeWithPurposeExists(IUnitOfWork uow, EmailPurpose emailPurpose);
		StoredEmail GetById(IUnitOfWork unitOfWork, int id);

		/// <summary>
		/// Проверка уже отправленных писем, за исключением определенного письма
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <param name="emailId">Идентификатор письма</param>
		/// <returns></returns>
		bool HasSendedEmailsForBillExceptOf(int orderId, int emailId);

		#endregion
		
		/// <summary>
		/// Получение адреса почты для связки с аккаунтом юр лица в ИПЗ
		/// </summary>
		/// <param name="uow">Unit Of Work</param>
		/// <param name="legalCounterpartyId">Идентификатор юр лица</param>
		/// <param name="email">Адрес электронной почты</param>
		/// <returns></returns>
		IEnumerable<Email> GetEmailForLinkingLegalCounterparty(IUnitOfWork uow, int legalCounterpartyId, string email);
	}
}
