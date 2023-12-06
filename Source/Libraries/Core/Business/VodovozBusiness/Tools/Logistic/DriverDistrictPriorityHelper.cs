using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Tools.Logistic
{
	public static class DriverDistrictPriorityHelper
	{
		public static DriverDistrictPrioritySet CopyPrioritySetWithActiveDistricts(IUnitOfWork uow, DriverDistrictPrioritySet districtPrioritySet, out IList<DriverDistrictPriority> notCopiedPriorities)
		{
			var copy = (DriverDistrictPrioritySet)districtPrioritySet.Clone();
			notCopiedPriorities = new List<DriverDistrictPriority>();

			for(int i = 0; i < copy.DriverDistrictPriorities.Count; i++)
			{
				if(copy.DriverDistrictPriorities[i].District.IsActive)
				{
					continue;
				}

				var activeDistrict = District.GetDistrictFromActiveDistrictsSetOrNull(uow, copy.DriverDistrictPriorities[i].District);

				if(activeDistrict != null)
				{
					copy.DriverDistrictPriorities[i].District = activeDistrict;
				}
				else
				{
					notCopiedPriorities.Add(districtPrioritySet.DriverDistrictPriorities[i + notCopiedPriorities.Count]);
					copy.DriverDistrictPriorities.RemoveAt(i);
					i--;
				}
			}
			copy.CheckAndFixDistrictsPriorities();
			return copy;
		}
	}
}
