using Edo.Problem.Routine.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Сервис обработки проблем с оплатой при самовывозе в ЭДО
	/// </summary>
	public class OrderSelfDeliveryPaidProblemService
	{
		private readonly ILogger<OrderSelfDeliveryPaidProblemService> _logger;
		private readonly IOptionsMonitor<OrderSelfDeliveryPaidProblemWorkerOptions> _options;

		public OrderSelfDeliveryPaidProblemService(
			ILogger<OrderSelfDeliveryPaidProblemService> logger,
			IOptionsMonitor<OrderSelfDeliveryPaidProblemWorkerOptions> options)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_options = options;
		}

		/// <summary>
		/// Обработчик задач с проблемой оплаты при самовывозе в ЭДО
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		public async Task ProcessProblemTasks(CancellationToken cancellationToken)
		{
			var timeout = _options.CurrentValue.ProblemTimeout;
		}
	}
}
