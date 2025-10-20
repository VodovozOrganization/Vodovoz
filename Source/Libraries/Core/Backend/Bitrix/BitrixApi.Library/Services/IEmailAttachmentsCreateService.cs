using Mailjet.Api.Abstractions;
using System.Collections.Generic;

namespace BitrixApi.Library.Services
{
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
	}
}
