using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Organizations;

namespace TaxcomEdoApi.Library.Converters.Format5_01
{
	public interface IParticipantDocFlowConverter5_01
	{
		UchastnikTip ConvertCounterpartyToUchastnikTip(CounterpartyInfoForEdo client);
		UchastnikTip ConvertCounterpartyToUchastnikTip(CounterpartyInfoForEdo client, DeliveryPointInfoForEdo deliveryPoint);
		UchastnikTip ConvertOrganizationToUchastnikTip(OrganizationInfoForEdo org);
	}
}
