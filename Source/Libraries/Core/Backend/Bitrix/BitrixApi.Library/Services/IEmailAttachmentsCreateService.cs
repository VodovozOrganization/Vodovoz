using Mailjet.Api.Abstractions;
using System.Collections.Generic;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace BitrixApi.Library.Services
{
	/// <summary>
	/// Сервис для создания вложений писем
	/// </summary>
	public interface IEmailAttachmentsCreateService
	{
		/// <summary>
		/// Создает вложения для письма актом сверки
		/// </summary>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <param name="organizationId">Id организации</param>
		/// <returns>Вложения с файлами</returns>
		IEnumerable<EmailAttachment> CreateRevisionAttachments(int counterpartyId, int organizationId);
		/// <summary>
		/// Создает вложения для письма со счетами по заказам
		/// </summary>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <param name="organizationId">Id организации</param>
		/// <param name="orderIds">Список Id заказов</param>
		/// <returns>Вложения с файлами</returns>
		IEnumerable<EmailAttachment> CreateOrdersBillsAttachments(int counterpartyId, int organizationId, IEnumerable<int> orderIds);
		/// <summary>
		/// Создает вложения для письма с общим счетом по заказам
		/// </summary>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <param name="organizationId">Id организации</param>
		/// <param name="orderIds">Список Id заказов</param>
		/// <returns>Вложения с файлами</returns>
		IEnumerable<EmailAttachment> CreateGeneralBillAttachments(int counterpartyId, int organizationId, IEnumerable<int> orderIds);

		/// <summary>
		/// Создает вложения для письма с претензией по задолженности (Letter of claim)
		/// </summary>
		/// <param name="organizationId">Id организации</param>
		/// <param name="clientId">Id клиента</param>
		/// <param name="debtSumFormatted">Сумма задолженности в формате строки</param>
		/// <param name="hideSignature">Флаг скрытия подписи</param>
		/// <returns>Вложения с файлами</returns>
		IEnumerable<EmailAttachment> CreateLetterOfClaimAttachments(int organizationId, int clientId, string debtSumFormatted, bool hideSignature = false);

		/// <summary>
		/// Создаёт вложение для письма со счётом без отгрузки на долг
		/// </summary>
		/// <param name="orderWithoutShipmentForDebt">Счёт без отгрузки на долг</param>
		/// <returns>Вложения с файлами</returns>
		IEnumerable<EmailAttachment> CreateOrderWithoutShipmentForDebtAttachments(OrderWithoutShipmentForDebt orderWithoutShipmentForDebt);
	}
}
