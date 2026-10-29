using System;
using Vodovoz.Settings.Edo;

namespace Vodovoz.Settings.Database.Edo
{
	/// <inheritdoc cref="IEdoDeviationSettings"/>
	public class EdoDeviationSettings : IEdoDeviationSettings
	{
		private readonly ISettingsController _settingsController;

		public EdoDeviationSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		/// <inheritdoc/>
		public bool IsEnabled => _settingsController
			.GetBoolValue("edo.deviation.enabled");

		/// <inheritdoc/>
		public TimeSpan RegistrationWorkerInterval => _settingsController
			.GetValue<TimeSpan>("edo.deviation.registration-worker-interval");

		/// <inheritdoc/>
		public TimeSpan ResolvingWorkerInterval => _settingsController
			.GetValue<TimeSpan>("edo.deviation.resolving-worker-interval");

		/// <inheritdoc/>
		public int BatchSize => _settingsController
			.GetValue<int>("edo.deviation.batch-size");

		/// <inheritdoc/>
		public DateTime GisMtTrackingStartDate => _settingsController
			.GetValue<DateTime>("edo.deviation.gis-mt-tracking-start-date");
	}
}
