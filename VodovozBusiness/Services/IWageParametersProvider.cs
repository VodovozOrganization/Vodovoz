using System;
namespace Vodovoz.Services
{
	public interface IWageParametersProvider
	{
		decimal GetFixedWageForNewLargusDrivers();
	}
}
