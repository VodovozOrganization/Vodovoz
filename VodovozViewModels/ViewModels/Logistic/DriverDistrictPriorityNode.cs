using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.ViewModels.Logistic
{
    public class DriverDistrictPriorityNode
    {
        public Sector Sector { get; set; }
        public int Priority { get; set; }
        public SectorVersion SectorVersion => Sector.GetActiveSectorVersion();
    }
}
