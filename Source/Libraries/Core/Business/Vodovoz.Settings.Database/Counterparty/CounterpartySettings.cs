using Vodovoz.Settings.Counterparty;

namespace Vodovoz.Settings.Database.Counterparty
{
	public class CounterpartySettings : ICounterpartySettings
	{
		private readonly ISettingsController _settingsController;

		public CounterpartySettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new System.ArgumentNullException(nameof(settingsController));
		}

		public int CounterpartyFromTenderId => _settingsController.GetValue<int>(nameof(CounterpartyFromTenderId).FromPascalCaseToSnakeCase());
	}
}
