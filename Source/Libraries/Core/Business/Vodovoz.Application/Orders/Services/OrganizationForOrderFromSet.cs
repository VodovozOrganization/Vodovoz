using System;
using System.Linq;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Application.Orders.Services
{
	public class OrganizationForOrderFromSet : IOrganizationForOrderFromSet
	{
		public Organization GetOrganizationForOrderFromSet(
			TimeSpan requestTime,
			IOrganizations organizationsSet,
			bool canReturnNull = false)
		{
			if(organizationsSet is null)
			{
				throw new ArgumentNullException(nameof(organizationsSet));
			}

			if(organizationsSet.Organizations.Any())
			{
				return GetOrganizationFromSet(requestTime, organizationsSet);
			}

			if(canReturnNull)
			{
				return null;
			}

			throw new ArgumentException($"Пустой список организаций у {typeof(IOrganizations)}", nameof(organizationsSet));
		}

		private Organization GetOrganizationFromSet(TimeSpan requestTime, IOrganizations organizationsSet)
		{
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
