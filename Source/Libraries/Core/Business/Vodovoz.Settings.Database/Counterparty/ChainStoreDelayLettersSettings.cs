using System;
using Vodovoz.Settings.Counterparty;

namespace Vodovoz.Settings.Database.Counterparty
{
	/// <inheritdoc/>
	public class ChainStoreDelayLettersSettings : IChainStoreDelayLettersSettings
	{
		private readonly ISettingsController _settingsController;
		private readonly string _parametersPrefix = "ChainStoreDelayLetters.";

		public ChainStoreDelayLettersSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public bool SendEnabled =>
			_settingsController.GetBoolValue($"{_parametersPrefix}SendEnabled");

		public TimeSpan SendInterval =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}SendInterval");

		public TimeSpan SendTimeFrom =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}SendTimeFrom");

		public TimeSpan SendTimeTo =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}SendTimeTo");

		public int DaysBeforePaymentDate =>
			_settingsController.GetIntValue($"{_parametersPrefix}DaysBeforePaymentDate");

		public string ContactPhone =>
			_settingsController.GetStringValue($"{_parametersPrefix}ContactPhone");
	}
}
