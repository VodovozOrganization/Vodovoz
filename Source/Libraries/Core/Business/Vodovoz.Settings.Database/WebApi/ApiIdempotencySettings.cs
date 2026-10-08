using System;
using Vodovoz.Settings.WebApi;

namespace Vodovoz.Settings.Database.WebApi
{
	public class ApiIdempotencySettings : IApiIdempotencySettings
	{
		private readonly string _parametersPrefix = "Idempotency.";

		private readonly ISettingsController _settingsController;

		public ApiIdempotencySettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		/// <inheritdoc/>
		public TimeSpan DriverApiResponseLifetime =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}DriverApi.ResponseLifetime");

		/// <inheritdoc/>
		public TimeSpan DriverApiMarkerLifetime =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}DriverApi.MarkerLifetime");

		/// <inheritdoc/>
		public TimeSpan DriverApiReplayWaitTimeout =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}DriverApi.ReplayWaitTimeout");
	}
}
