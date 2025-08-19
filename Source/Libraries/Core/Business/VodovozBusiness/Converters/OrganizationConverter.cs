using System;
using System.Linq;
using TaxcomEdo.Contracts.Organizations;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Converters
{
	public class OrganizationConverter : IOrganizationConverter
	{
		public OrganizationInfoForEdo ConvertOrganizationToOrganizationInfoForEdo(Organization organization, DateTime dateTime)
		{
			var organizationVersion = organization.OrganizationVersions.SingleOrDefault(
				x => x.StartDate <= dateTime
					&& (x.EndDate == null || x.EndDate >= dateTime));
			
			return new OrganizationInfoForEdo
			{
				Id = organization.Id,
				Name = organization.Name,
				FullName = organization.FullName,
				Inn = organization.INN,
				Kpp = organization.KPP,
				TaxcomEdoAccountId = organization.TaxcomEdoSettings.EdoAccount,
				JurAddress = organizationVersion?.JurAddress
			};
		}
	}
}
