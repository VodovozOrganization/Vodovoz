using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Application.Orders.Services
{
	public class OrganizationForOrderFromSet : IOrganizationForOrderFromSet
	{
		private const decimal _dayHours = 24m;

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

		public IReadOnlyDictionary<int, (TimeSpan From, TimeSpan To)> GetChoiceTimeOrganizationFromSet(
			IEnumerable<INamedDomainObject> organizations)
		{
			var dictionary = new Dictionary<int, (TimeSpan From, TimeSpan To)>();
			var organizationsCount = organizations.Count();

			if(organizationsCount <= 0)
			{
				return dictionary;
			}
			
			var sliceHour = Math.Round(_dayHours / organizationsCount, 1);
			var second = new TimeSpan(0, 0, 1);

			var i = 0;
			var fromHour = 0m;
			var toHour = 0m;

			foreach(var organization in organizations)
			{
				if(i == organizationsCount - 1)
				{
					dictionary.Add(
						organizations.Last().Id,
						(TimeSpan.FromHours((double)toHour), new TimeSpan(23, 59, 59)));
				}
				else
				{
					fromHour = sliceHour * i;
					toHour = sliceHour * (i + 1);
					dictionary.Add(
						organization.Id,
						(TimeSpan.FromHours((double)fromHour), TimeSpan.FromHours((double)toHour).Subtract(second)));
				}

				i++;
			}

			return dictionary;
		}

		private Organization GetOrganizationFromSet(TimeSpan requestTime, IOrganizations organizationsSet)
		{
			var organizationsCount = organizationsSet.Organizations.Count;

			if(organizationsCount == 1)
			{
				return organizationsSet.Organizations[0];
			}

			var sliceHour = _dayHours / organizationsCount;
			var organizationIndex = (int)Math.Floor((decimal)requestTime.TotalHours / sliceHour);

			if(organizationIndex >= organizationsCount)
			{
				organizationIndex = organizationsCount - 1;
			}
			
			return organizationsSet.Organizations[organizationIndex];
		}
	}
}
