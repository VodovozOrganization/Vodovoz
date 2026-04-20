using System;
using Vodovoz.Settings.Edo;

namespace Vodovoz.Settings.Database.Edo
{
	public class EdoProblemRoutineSettings : IEdoProblemRoutineSettings
	{
		private readonly ISettingsController _settingsController;

		public EdoProblemRoutineSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new System.ArgumentNullException(nameof(settingsController));
		}

		public TimeSpan SelfDeliveryPaidProblemTimeout => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.self-delivery-paid-problem-timeout");

		public TimeSpan SelfDeliveryPaidProblemWorkerInterval => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.self-delivery-paid-worker-interval");
	}
}
