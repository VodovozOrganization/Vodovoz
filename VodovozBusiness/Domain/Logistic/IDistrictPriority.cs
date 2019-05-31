using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic
{
	public interface IDistrictPriority
	{
		ScheduleRestrictedDistrict District {get;}
		int Priority { get; }
	}
}
