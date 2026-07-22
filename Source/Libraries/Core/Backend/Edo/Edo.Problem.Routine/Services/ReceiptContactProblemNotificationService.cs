using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Temporary mock notification service until Edo notification pipeline is ready.
	/// </summary>
	public class MockReceiptContactProblemNotificationService : IReceiptContactProblemNotificationService
	{
		private readonly ILogger<MockReceiptContactProblemNotificationService> _logger;

		public MockReceiptContactProblemNotificationService(ILogger<MockReceiptContactProblemNotificationService> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public Task NotifyAsync(
			ReceiptEdoTask receiptTask,
			EdoTaskProblem problem,
			int retryCount,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			_logger.LogWarning(
				"[MOCK] Требуется уведомление по проблеме контакта чека. EdoTaskId: {EdoTaskId}, ProblemId: {ProblemId}, RetryCount: {RetryCount}",
				receiptTask.Id,
				problem.Id,
				retryCount);

			return Task.CompletedTask;
		}
	}
}
