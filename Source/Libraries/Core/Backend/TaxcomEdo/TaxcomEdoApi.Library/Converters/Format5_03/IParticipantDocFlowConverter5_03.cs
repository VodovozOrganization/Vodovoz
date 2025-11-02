using Taxcom.Client.Api.Document.DocumentByFormat1115131_5_03;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Organizations;

namespace TaxcomEdoApi.Library.Converters.Format5_03
{
	public interface IParticipantDocFlowConverter5_03
	{
		UchastnikTip ConvertCounterpartyToUchastnikTip(CounterpartyInfoForEdo client);
		UchastnikTip ConvertCounterpartyToUchastnikTip(CounterpartyInfoForEdo client, DeliveryPointInfoForEdo deliveryPoint);
		UchastnikTip ConvertOrganizationToUchastnikTip(OrganizationInfoForEdo org);
	}
}
