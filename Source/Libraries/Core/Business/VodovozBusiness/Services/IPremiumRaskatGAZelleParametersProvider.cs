namespace Vodovoz.Services
{
	public interface IPremiumRaskatGAZelleParametersProvider
	{
		decimal PremiumRaskatGAZelleMoney { get; }
		int MinRecalculatedDistanceForPremiumRaskatGAZelle { get; }
	}
}