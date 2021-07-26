using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Tools.Logistic
{
    public static class DriverDistrictPriorityHelper
    {
        public static DriverDistrictPrioritySet CopyPrioritySetWithActiveDistricts(DriverDistrictPrioritySet districtPrioritySet, out IList<DriverDistrictPriority> notCopiedPriorities)
        {
            var copy = (DriverDistrictPrioritySet)districtPrioritySet.Clone();
            notCopiedPriorities = new List<DriverDistrictPriority>();
            
            // for(int i = 0; i < copy.DriverDistrictPriorities.Count; i++) {
            //     if(copy.DriverDistrictPriorities[i].Sector.IsActive) {
            //         continue;
            //     }
            //     var activeDistrict = Sector.GetDistrictFromActiveDistrictsSetOrNull(copy.DriverDistrictPriorities[i].Sector.SectorVersion);
            //     if(activeDistrict != null) {
            //         copy.DriverDistrictPriorities[i].Sector = activeDistrict;
            //     }
            //     else {
            //         notCopiedPriorities.Add(districtPrioritySet.DriverDistrictPriorities[i]);
            //         copy.DriverDistrictPriorities.RemoveAt(i);
            //         i--;
            //     }
            // }
            copy.CheckAndFixDistrictsPriorities();
            return copy;
        }
    }
}
