using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class GazelRaskatPremiumParametersProvider : IGazelRaskatPremiumParametersProvider
	{
		private readonly IParametersProvider parametersProvider;

		public GazelRaskatPremiumParametersProvider(IParametersProvider parametersProvider)
		{
			this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public decimal GazelRaskatPremiumMoney => parametersProvider.GetDecimalValue("gazel_raskat_premium_money");
		public int MinRecalculatedDistanceForGazelRaskatPremium => parametersProvider.GetIntValue("min_recalculated_distance_for_gazel_raskat_premium");
	}
}