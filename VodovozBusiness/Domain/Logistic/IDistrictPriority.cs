using System;
namespace Vodovoz.Domain.Logistic
{
	public interface IDistrictPriority
	{
		LogisticsArea District {get;}
		int Priority { get; }
	}
}
