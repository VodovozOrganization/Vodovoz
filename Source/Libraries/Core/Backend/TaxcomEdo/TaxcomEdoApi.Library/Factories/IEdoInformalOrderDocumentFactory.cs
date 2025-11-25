using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdoApi.Library.Factories
{
	public interface IEdoInformalOrderDocumentFactory
	{
		NonformalizedDocument CreateInformalOrderDocument(InfoForCreatingEdoInformalOrderDocument data);
	}
}
