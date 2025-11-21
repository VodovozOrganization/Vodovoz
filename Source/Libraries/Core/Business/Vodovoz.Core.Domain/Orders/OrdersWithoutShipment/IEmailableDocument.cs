using QS.Report;
using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Orders.Documents;
using Vodovoz.Core.Domain.StoredEmails;

namespace Vodovoz.Core.Domain.Orders.OrdersWithoutShipment
{
	/// <summary>
	/// Документ, который можно отправить по электронной почте
	/// </summary>
	public interface IEmailableDocument : IDocument, ISignableDocument
	{
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
		/// <param name="edoAccountController"></param>
		/// <returns></returns>
		EmailTemplateEntity GetEmailTemplate(ICounterpartyEdoAccountEntityController edoAccountController = null);
		/// <summary>
		/// Получить информацию по отчету для формирования документа
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		ReportInfo GetReportInfo(string connectionString = null);
	}

	/// <summary>
	/// Кастомный документ с возможностью повторной отправки
	/// </summary>
	public interface ICustomResendTemplateEmailableDocument : IEmailableDocument
	{
		EmailTemplateEntity GetResendDocumentEmailTemplate();
	}
}
