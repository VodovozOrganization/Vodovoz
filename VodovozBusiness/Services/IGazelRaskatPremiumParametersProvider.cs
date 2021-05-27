namespace Vodovoz.Services
{
	public interface IGazelRaskatPremiumParametersProvider
	{
		decimal GazelRaskatPremiumMoney { get; }
		int MinRecalculatedDistanceForGazelRaskatPremium { get; }
	}
}