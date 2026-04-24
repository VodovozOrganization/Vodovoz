using System;
using Vodovoz.Settings.Edo;

namespace Vodovoz.Settings.Database.Edo
{
	public class EdoProblemRoutineSettings : IEdoProblemRoutineSettings
	{
		private readonly ISettingsController _settingsController;

		public EdoProblemRoutineSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public TimeSpan SelfDeliveryPaidProblemTimeout => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.self-delivery-paid-problem-timeout");

		public TimeSpan SelfDeliveryPaidProblemWorkerInterval => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.self-delivery-paid-worker-interval");

		public TimeSpan FiscalDocumentSendErrorProblemTimeout => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.fiscal-doc-send-error-problem-timeout");

		public TimeSpan FiscalDocumentSendErrorProblemWorkerInterval => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.fiscal-doc-send-error-worker-interval");

		public TimeSpan OrderStatusProblemTimeout => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.order-status-problem-timeout");

		public TimeSpan OrderStatusProblemWorkerInterval => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.order-status-worker-interval");
	}
}
