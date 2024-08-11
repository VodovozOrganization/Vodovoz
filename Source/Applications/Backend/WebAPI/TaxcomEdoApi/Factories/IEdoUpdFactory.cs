using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdoApi.Config;
using Vodovoz.Core.Data.Orders;

namespace TaxcomEdoApi.Factories
{
	public interface IEdoUpdFactory
	{
		Fajl CreateNewUpdXml(OrderInfoForEdo orderInfoForEdo, WarrantOptions warrantOptions, string organizationAccountId, string certificateSubject);
	}
}
