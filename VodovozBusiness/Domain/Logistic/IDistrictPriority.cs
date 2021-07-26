using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Domain.Logistic
{
	public interface IDistrictPriority
	{
		Sector Sector {get;}
		int Priority { get; }
	}
}
