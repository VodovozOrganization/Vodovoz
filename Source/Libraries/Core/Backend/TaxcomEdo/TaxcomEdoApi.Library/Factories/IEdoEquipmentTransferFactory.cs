using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdoApi.Library.Factories
{
	public interface IEdoEquipmentTransferFactory
	{
		NonformalizedDocument CreateEquipmentTransferDocument(InfoForCreatingEdoEquipmentTransfer data);
	}
}
