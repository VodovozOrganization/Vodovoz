using Edo.Problem.Routine;
using Microsoft.Extensions.DependencyInjection;

namespace Edo.Receipt.NightSend.Worker
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoReceiptNightSend(this IServiceCollection services)
		{
			services
				.AddEdoProblemRoutine()
				.AddHostedService<ReceiptNightSendProblemWorker>();

			return services;
		}
	}
}
