using System;
using Vodovoz.Settings.Logistics;

namespace Vodovoz.Settings.Database.Logistics
{
	public class PremiumRaskatGAZelleSettings : IPremiumRaskatGAZelleSettings
	{
		private readonly ISettingsController _settingsController;

		public PremiumRaskatGAZelleSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public decimal PremiumRaskatGAZelleMoney => _settingsController.GetDecimalValue("premium_raskat_gazelle_money");

		public int MinRecalculatedDistanceForPremiumRaskatGAZelle => _settingsController.GetIntValue("min_recalculated_distance_for_premium_raskat_gazelle");
	}
}
