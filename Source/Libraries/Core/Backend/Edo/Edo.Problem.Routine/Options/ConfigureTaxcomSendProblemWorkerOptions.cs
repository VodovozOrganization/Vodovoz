using Microsoft.Extensions.Options;
using Vodovoz.Settings.Edo;

namespace Edo.Problem.Routine.Options
{
	public class ConfigureTaxcomSendProblemWorkerOptions : IConfigureOptions<TaxcomSendProblemWorkerOptions>
	{
		private readonly IEdoProblemRoutineSettings _edoProblemRoutineSettings;

		public ConfigureTaxcomSendProblemWorkerOptions(IEdoProblemRoutineSettings edoProblemRoutineSettings)
		{
			_edoProblemRoutineSettings = edoProblemRoutineSettings ?? throw new System.ArgumentNullException(nameof(edoProblemRoutineSettings));
		}

		public void Configure(TaxcomSendProblemWorkerOptions options)
		{
			options.WorkerInterval = _edoProblemRoutineSettings.TaxcomSendProblemWorkerInterval;
		}
	}
}
