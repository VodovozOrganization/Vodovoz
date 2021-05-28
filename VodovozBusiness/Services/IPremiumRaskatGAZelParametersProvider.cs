namespace Vodovoz.Services
{
	public interface IPremiumRaskatGAZelParametersProvider
	{
		decimal PremiumRaskatGAZelMoney { get; }
		int MinRecalculatedDistanceForPremiumRaskatGAZel { get; }
	}
}