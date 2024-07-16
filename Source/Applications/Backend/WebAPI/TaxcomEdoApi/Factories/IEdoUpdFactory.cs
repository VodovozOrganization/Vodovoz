using System.Collections.Generic;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdoApi.Config;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;

namespace TaxcomEdoApi.Factories
{
	public interface IEdoUpdFactory
	{
		Fajl CreateNewUpdXml(Order order, WarrantOptions warrantOptions, string organizationAccountId, string certificateSubject, IEnumerable<Payment> orderPayments);
	}
}
