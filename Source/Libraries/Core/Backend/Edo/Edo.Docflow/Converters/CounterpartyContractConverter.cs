using System;
using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace Edo.Docflow.Converters
{
	public class CounterpartyContractConverter : ICounterpartyContractConverter
	{
		private readonly IOrganizationConverter _organizationConverter;

		public CounterpartyContractConverter(IOrganizationConverter organizationConverter)
		{
			_organizationConverter = organizationConverter ?? throw new ArgumentNullException(nameof(organizationConverter));
		}
		
		public CounterpartyContractInfoForEdo ConvertCounterpartyContractToCounterpartyContractInfoForEdo(
			CounterpartyContractEntity contract, DateTime dateTime)
		{
			var organizationInfo = _organizationConverter.ConvertOrganizationToOrganizationInfoForEdo(contract.Organization, dateTime);

			return new CounterpartyContractInfoForEdo
			{
				Id = contract.Id,
				Number = contract.Number,
				IssueDate = contract.IssueDate,
				OrganizationInfoForEdo = organizationInfo
			};
		}
	}
}
