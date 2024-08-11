using System;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using Vodovoz.Core.Data.Clients;
using Vodovoz.Core.Data.Organizations;

namespace TaxcomEdoApi.Converters
{
	public interface IParticipantDocFlowConverter
	{
		UchastnikTip ConvertCounterpartyToUchastnikTip(CounterpartyInfoForEdo client);
		UchastnikTip ConvertCounterpartyToUchastnikTip(CounterpartyInfoForEdo client, DeliveryPointInfoForEdo deliveryPoint);
		UchastnikTip ConvertOrganizationToUchastnikTip(OrganizationInfoForEdo org, DateTime? deliveryDate);
	}
}
