using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class PremiumRaskatGAZelleParametersProvider : IPremiumRaskatGAZelleParametersProvider
	{
		private readonly IParametersProvider parametersProvider;

		public PremiumRaskatGAZelleParametersProvider(IParametersProvider parametersProvider)
		{
			this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public decimal PremiumRaskatGAZelleMoney => parametersProvider.GetDecimalValue("premium_raskat_gazelle_money");
		
		public int MinRecalculatedDistanceForPremiumRaskatGAZelle => parametersProvider.GetIntValue("min_recalculated_distance_for_premium_raskat_gazelle");
	}
}