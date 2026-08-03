using System;
using Microsoft.Extensions.Options;
using Vodovoz.Settings.Edo;

namespace Edo.Problem.Routine.Options
{
	public class ConfigureReceiptContactProblemWorkerOptions : IConfigureOptions<ReceiptContactProblemWorkerOptions>
	{
		private readonly IEdoProblemRoutineSettings _edoProblemRoutineSettings;

		public ConfigureReceiptContactProblemWorkerOptions(IEdoProblemRoutineSettings edoProblemRoutineSettings)
		{
			_edoProblemRoutineSettings = edoProblemRoutineSettings
				?? throw new ArgumentNullException(nameof(edoProblemRoutineSettings));
		}

		public void Configure(ReceiptContactProblemWorkerOptions options)
		{
			options.ProblemTimeout = _edoProblemRoutineSettings.ReceiptContactProblemTimeout;
			options.WorkerInterval = _edoProblemRoutineSettings.ReceiptContactProblemWorkerInterval;
			options.RetryAttemptsBeforeNotification =
				_edoProblemRoutineSettings.ReceiptContactProblemRetryAttemptsBeforeNotification;

			if(options.ProblemTimeout <= TimeSpan.Zero)
			{
				throw new InvalidOperationException("Таймаут обработки проблем с контактом чека должен быть больше нуля");
			}

			if(options.WorkerInterval <= TimeSpan.Zero)
			{
				throw new InvalidOperationException("Интервал работы воркера проблем с контактом чека должен быть больше нуля");
			}

			if(options.RetryAttemptsBeforeNotification < 1)
			{
				throw new InvalidOperationException("Количество повторов до уведомления должно быть не меньше одного");
			}
		}
	}
}
