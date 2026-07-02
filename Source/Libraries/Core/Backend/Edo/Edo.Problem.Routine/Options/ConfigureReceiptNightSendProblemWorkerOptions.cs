using Microsoft.Extensions.Options;
using System;
using Vodovoz.Settings.Edo;

namespace Edo.Problem.Routine.Options
{
	public class ConfigureReceiptNightSendProblemWorkerOptions : IConfigureOptions<ReceiptNightSendProblemWorkerOptions>
	{
		private readonly IEdoProblemRoutineSettings _edoProblemRoutineSettings;

		public ConfigureReceiptNightSendProblemWorkerOptions(IEdoProblemRoutineSettings edoProblemRoutineSettings)
		{
			_edoProblemRoutineSettings = edoProblemRoutineSettings ?? throw new ArgumentNullException(nameof(edoProblemRoutineSettings));
		}

		public void Configure(ReceiptNightSendProblemWorkerOptions options)
		{
			options.ProblemTimeout = _edoProblemRoutineSettings.ReceiptNightSendProblemTimeout;
			options.WorkerInterval = _edoProblemRoutineSettings.ReceiptNightSendProblemWorkerInterval;
		}
	}
}
