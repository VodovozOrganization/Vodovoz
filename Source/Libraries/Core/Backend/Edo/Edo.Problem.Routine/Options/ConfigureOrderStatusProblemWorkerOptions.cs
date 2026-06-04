using Microsoft.Extensions.Options;
using Vodovoz.Settings.Edo;

namespace Edo.Problem.Routine.Options
{
	public class ConfigureOrderStatusProblemWorkerOptions : IConfigureOptions<OrderStatusProblemWorkerOptions>
	{
		private readonly IEdoProblemRoutineSettings _edoProblemRoutineSettings;

		public ConfigureOrderStatusProblemWorkerOptions(IEdoProblemRoutineSettings edoProblemRoutineSettings)
		{
			_edoProblemRoutineSettings = edoProblemRoutineSettings ?? throw new System.ArgumentNullException(nameof(edoProblemRoutineSettings));
		}

		public void Configure(OrderStatusProblemWorkerOptions options)
		{
			options.ProblemTimeout = _edoProblemRoutineSettings.OrderStatusProblemTimeout;
			options.WorkerInterval = _edoProblemRoutineSettings.OrderStatusProblemWorkerInterval;
		}
	}
}
