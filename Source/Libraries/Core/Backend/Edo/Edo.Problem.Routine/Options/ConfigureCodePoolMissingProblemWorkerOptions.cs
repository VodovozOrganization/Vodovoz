using System;
using Microsoft.Extensions.Options;
using Vodovoz.Settings.Edo;

namespace Edo.Problem.Routine.Options
{
	public class ConfigureCodePoolMissingProblemWorkerOptions : IConfigureOptions<CodePoolMissingProblemWorkerOptions>
	{
		private readonly IEdoProblemRoutineSettings _edoProblemRoutineSettings;

		public ConfigureCodePoolMissingProblemWorkerOptions(IEdoProblemRoutineSettings edoProblemRoutineSettings)
		{
			_edoProblemRoutineSettings = edoProblemRoutineSettings
				?? throw new ArgumentNullException(nameof(edoProblemRoutineSettings));
		}

		public void Configure(CodePoolMissingProblemWorkerOptions options)
		{
			options.WorkerInterval = _edoProblemRoutineSettings.CodePoolMissingProblemWorkerInterval;
			options.MaxAttempts = _edoProblemRoutineSettings.CodePoolMissingProblemWorkerMaxAttempts;
			options.BatchSize = _edoProblemRoutineSettings.CodePoolMissingProblemWorkerBatchSize;
			options.RetryIntervalHours = _edoProblemRoutineSettings.CodePoolMissingProblemWorkerRetryIntervalHours;

			if(options.WorkerInterval <= TimeSpan.Zero)
			{
				throw new InvalidOperationException("Интервал работы воркера проблем с отсутствующим кодом в пуле должен быть больше нуля");
			}

			if(options.MaxAttempts < 1)
			{
				throw new InvalidOperationException("Количество повторов до уведомления должно быть не меньше одного");
			}

			if(options.BatchSize < 1) 
			{ 
				throw new InvalidOperationException("Размер батча задач должен быть не меньше одного"); 
			}

			if(options.RetryIntervalHours < 1)
			{
				throw new InvalidOperationException("Интервал повторной попытки должен быть не меньше одного часа");
			}
		}
	}
}
