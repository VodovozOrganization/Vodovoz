using System;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using Vodovoz.Core.Data.Clients;
using Vodovoz.Core.Data.Organizations;

namespace TaxcomEdoApi.Converters
{
	public interface IParticipantDocFlowConverter
	{
		UchastnikTip ConvertCounterpartyToUchastnikTip(Counterparty client);
		UchastnikTip ConvertCounterpartyToUchastnikTip(Counterparty client, int? deliveryPointId);
		UchastnikTip ConvertOrganizationToUchastnikTip(Organization org, DateTime? deliveryDate);
	}
}
