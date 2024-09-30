﻿using QS.Report;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	public interface IEmailableDocument : IDocument, ISignableDocument
	{
		string Title { get; }
		DateTime? DocumentDate { get; }
		Counterparty Counterparty { get; }
		EmailTemplate GetEmailTemplate();
		ReportInfo GetReportInfo(string connectionString = null);
	}

	public interface IManuallyResendEmailableDocument : IEmailableDocument
	{
		EmailTemplate GetResendDocumentEmailTemplate();
	}
}
