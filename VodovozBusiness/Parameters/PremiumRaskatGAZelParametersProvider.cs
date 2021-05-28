using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class PremiumRaskatGAZelParametersProvider : IPremiumRaskatGAZelParametersProvider
	{
		private readonly IParametersProvider parametersProvider;

		public PremiumRaskatGAZelParametersProvider(IParametersProvider parametersProvider)
		{
			this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public decimal PremiumRaskatGAZelMoney => parametersProvider.GetDecimalValue("premium_raskat_gazel_money");
		
		public int MinRecalculatedDistanceForPremiumRaskatGAZel => parametersProvider.GetIntValue("min_recalculated_distance_for_premium_raskat_gazel");
	}
}