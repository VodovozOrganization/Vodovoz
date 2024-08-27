using System.Collections.Generic;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdoApi.Config;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Payments;

namespace TaxcomEdoApi.Factories
{
	//TODO избавится от сущности Payment, переделав на простую Dto
	public interface IEdoUpdFactory
	{
		Fajl CreateNewUpdXml(
			OrderInfoForEdo orderInfoForEdo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject,
			IEnumerable<Payment> orderPayments);
	}
}
