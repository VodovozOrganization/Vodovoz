using QS.Report;
using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Orders.Documents;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Core.Domain.Orders.OrdersWithoutShipment
{
	/// <summary>
	/// Документ, который можно отправить по электронной почте
	/// </summary>
	public interface IEmailableDocument : ISignableDocument
	{
		/// <summary>
		/// Id документа
		/// </summary>
		//Пока не перенесём все документы на новый интерфейс, чтобы не ломать старый код.
		int DocumentId { get; }
		/// <summary>
		/// Заголовок
		/// </summary>
		string Title { get; }
		/// <summary>
		/// Дата документа
		/// </summary>
		DateTime? DocumentDate { get; }
		/// <summary>
		/// Контрагент
		/// </summary>
		CounterpartyEntity Counterparty { get; }
		/// <summary>
		/// Получить шаблон письма для отправки документа
		/// </summary>
		/// <returns></returns>
		EmailTemplate GetEmailTemplate(
			ICounterpartyEdoAccountEntityController edoAccountController = null,
			IOrganizationSettings organizationSettings = null,
			IDeliveryScheduleSettings deliveryScheduleSettings = null);
		/// <summary>
		/// Получить информацию по отчету для формирования документа
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		ReportInfo GetReportInfo(string connectionString = null);
	}
}
