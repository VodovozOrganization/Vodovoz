using System;
using System.Linq;
using Vodovoz.Core.Data.Organizations;
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
				INN = organization.INN,
				KPP = organization.KPP,
				TaxcomEdoAccountId = organization.TaxcomEdoAccountId,
				JurAddress = organizationVersion?.JurAddress
			};
		}
	}
}
