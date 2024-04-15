using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using Vodovoz.Domain.Orders;

namespace TaxcomEdoApi.Factories
{
	public interface IEdoUpdFactory
	{
		Fajl CreateNewUpdXml(Order order, string organizationAccountId, string certificateSubject);
	}
}
