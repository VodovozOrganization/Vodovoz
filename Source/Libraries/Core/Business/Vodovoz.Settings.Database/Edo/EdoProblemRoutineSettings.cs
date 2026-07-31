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

		public TimeSpan ReceiptNightSendProblemTimeout => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.receipt-night-send-problem-timeout");

		public TimeSpan ReceiptNightSendProblemWorkerInterval => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.receipt-night-send-worker-interval");

		public TimeSpan CodeDuplicatedProblemTimeout => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.code-duplicated-problem-timeout");

		public TimeSpan CodeDuplicatedProblemWorkerInterval => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.code-duplicated-worker-interval");

		public TimeSpan ReceiptContactProblemTimeout => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.receipt-contact-problem-timeout");

		public TimeSpan ReceiptContactProblemWorkerInterval => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.receipt-contact-worker-interval");

		public int ReceiptContactProblemRetryAttemptsBeforeNotification => _settingsController
			.GetValue<int>("edo.problem.routine.receipt-contact-retries");

		public TimeSpan CodePoolMissingProblemWorkerInterval => _settingsController
			.GetValue<TimeSpan>("edo.problem.routine.code-pool-missing-worker-interval");

		public int CodePoolMissingProblemWorkerMaxAttempts => _settingsController
			.GetValue<int>("edo.problem.routine.code-pool-missing-problem-max-attempts");

		public int CodePoolMissingProblemWorkerBatchSize => _settingsController
			.GetValue<int>("edo.problem.routine.code-pool-missing-worker-batch-size");
	}
}
