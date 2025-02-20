using System;
using System.Linq;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Application.Orders.Services
{
	public class OrganizationForOrderFromSet
	{
		public Organization GetOrganizationForOrderFromSet(int orderId, IOrganizations organizationsSet)
		{
			if(organizationsSet is null)
			{
				throw new ArgumentNullException(nameof(organizationsSet));
			}

			if(!organizationsSet.Organizations.Any())
			{
				throw new ArgumentException("Organizations set is empty");
			}
			
			return organizationsSet.Organizations[orderId % organizationsSet.Organizations.Count];
		}
	}
}
