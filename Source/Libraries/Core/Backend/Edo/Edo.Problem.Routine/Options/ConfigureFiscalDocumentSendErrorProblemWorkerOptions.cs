using Microsoft.Extensions.Options;
using System;
using Vodovoz.Settings.Edo;

namespace Edo.Problem.Routine.Options
{
	public class ConfigureFiscalDocumentSendErrorProblemWorkerOptions : IConfigureOptions<FiscalDocumentSendErrorProblemWorkerOptions>
	{
		private readonly IEdoProblemRoutineSettings _edoProblemRoutineSettings;

		public ConfigureFiscalDocumentSendErrorProblemWorkerOptions(IEdoProblemRoutineSettings edoProblemRoutineSettings)
		{
			_edoProblemRoutineSettings = edoProblemRoutineSettings ?? throw new ArgumentNullException(nameof(edoProblemRoutineSettings));
		}

		public void Configure(FiscalDocumentSendErrorProblemWorkerOptions options)
		{
			options.ProblemTimeout = _edoProblemRoutineSettings.FiscalDocumentSendErrorProblemTimeout;
			options.WorkerInterval = _edoProblemRoutineSettings.FiscalDocumentSendErrorProblemWorkerInterval;
		}
	}
}
