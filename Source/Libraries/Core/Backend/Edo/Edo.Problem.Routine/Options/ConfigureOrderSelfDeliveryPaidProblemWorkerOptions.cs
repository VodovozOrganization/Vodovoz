using Microsoft.Extensions.Options;
using Vodovoz.Settings.Edo;

namespace Edo.Problem.Routine.Options
{
	public class ConfigureOrderSelfDeliveryPaidProblemWorkerOptions : IConfigureOptions<OrderSelfDeliveryPaidProblemWorkerOptions>
	{
		private readonly IEdoProblemRoutineSettings _edoProblemRoutineSettings;

		public ConfigureOrderSelfDeliveryPaidProblemWorkerOptions(IEdoProblemRoutineSettings edoProblemRoutineSettings)
		{
			_edoProblemRoutineSettings = edoProblemRoutineSettings ?? throw new System.ArgumentNullException(nameof(edoProblemRoutineSettings));
		}

		public void Configure(OrderSelfDeliveryPaidProblemWorkerOptions options)
		{
			options.ProblemTimeout = _edoProblemRoutineSettings.SelfDeliveryPaidProblemTimeout;
			options.WorkerInterval = _edoProblemRoutineSettings.SelfDeliveryPaidProblemWorkerInterval;
		}
	}
}
