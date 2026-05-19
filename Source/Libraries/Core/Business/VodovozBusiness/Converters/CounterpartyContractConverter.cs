using System;
using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Domain.Client;

namespace Vodovoz.Converters
{
	public class CounterpartyContractConverter : ICounterpartyContractConverter
	{
		private readonly IOrganizationConverter _organizationConverter;

		public CounterpartyContractConverter(IOrganizationConverter organizationConverter)
		{
			_organizationConverter = organizationConverter ?? throw new ArgumentNullException(nameof(organizationConverter));
		}
		
		public CounterpartyContractInfoForEdo ConvertCounterpartyContractToCounterpartyContractInfoForEdo(
			CounterpartyContract contract, DateTime dateTime)
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
