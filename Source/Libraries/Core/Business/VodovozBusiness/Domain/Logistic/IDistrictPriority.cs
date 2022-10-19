using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic
{
	public interface IDistrictPriority
	{
		District District {get;}
		int Priority { get; }
	}
}
