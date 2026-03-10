using QS.Report;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Controllers;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	public interface IEmailableDocument : IDocument, ISignableDocument
	{
		string Title { get; }
		DateTime? DocumentDate { get; }
		Counterparty Counterparty { get; }
		EmailTemplate GetEmailTemplate(ICounterpartyEdoAccountController edoAccountController = null, IOrganizationSettings organizationSettings = null);
		ReportInfo GetReportInfo(string connectionString = null);
	}

	public interface ICustomResendTemplateEmailableDocument : IEmailableDocument
	{
		EmailTemplate GetResendDocumentEmailTemplate();
	}
}
