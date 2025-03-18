using System;
using System.Linq;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Application.Orders.Services
{
	public class OrganizationForOrderFromSet : IOrganizationForOrderFromSet
	{
		public Organization GetOrganizationForOrderFromSet(TimeSpan requestTime, IOrganizations organizationsSet)
		{
			if(organizationsSet is null)
			{
				throw new ArgumentNullException(nameof(organizationsSet));
			}

			if(!organizationsSet.Organizations.Any())
			{
				throw new ArgumentException("Organizations set is empty", nameof(organizationsSet));
			}

			var countOrganizations = organizationsSet.Organizations.Count;

			if(countOrganizations == 1)
			{
				return organizationsSet.Organizations[0];
			}

			var sliceHour = 24m / countOrganizations;
			var organizationIndex = (int)Math.Floor((decimal)requestTime.TotalHours / sliceHour);

			if(organizationIndex > countOrganizations)
			{
				organizationIndex = countOrganizations - 1;
			}
			
			return organizationsSet.Organizations[organizationIndex];
		}
	}
}
