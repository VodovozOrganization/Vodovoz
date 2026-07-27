using Microsoft.Extensions.Options;
using Vodovoz.Settings.Edo;

namespace Edo.Problem.Routine.Options
{
	public class ConfigureCodeDuplicatedProblemWorkerOptions : IConfigureOptions<CodeDuplicatedProblemWorkerOptions>
	{
		private readonly IEdoProblemRoutineSettings _edoProblemRoutineSettings;

		public ConfigureCodeDuplicatedProblemWorkerOptions(IEdoProblemRoutineSettings edoProblemRoutineSettings)
		{
			_edoProblemRoutineSettings = edoProblemRoutineSettings ?? throw new System.ArgumentNullException(nameof(edoProblemRoutineSettings));
		}

		public void Configure(CodeDuplicatedProblemWorkerOptions options)
		{
			options.ProblemTimeout = _edoProblemRoutineSettings.CodeDuplicatedProblemTimeout;
			options.WorkerInterval = _edoProblemRoutineSettings.CodeDuplicatedProblemWorkerInterval;
		}
	}
}
