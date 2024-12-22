using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Organizations;

namespace TaxcomEdoApi.Library.Converters
{
	public interface IParticipantDocFlowConverter
	{
		UchastnikTip ConvertCounterpartyToUchastnikTip(CounterpartyInfoForEdo client);
		UchastnikTip ConvertCounterpartyToUchastnikTip(CounterpartyInfoForEdo client, DeliveryPointInfoForEdo deliveryPoint);
		UchastnikTip ConvertOrganizationToUchastnikTip(OrganizationInfoForEdo org);
	}
}
