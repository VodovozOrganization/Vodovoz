namespace Vodovoz.Services
{
	public interface IWageParametersProvider
	{
		int GetDaysWorkedForMinRatesLevel();
		decimal GetFixedWageForNewLargusDrivers();
	}
}
