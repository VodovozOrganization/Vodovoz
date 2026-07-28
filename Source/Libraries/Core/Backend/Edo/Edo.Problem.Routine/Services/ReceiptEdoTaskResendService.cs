using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Сервис безопасного повторного запуска задачи ЭДО на отправку чека.
	/// </summary>
	public class ReceiptEdoTaskResendService : IReceiptEdoTaskResendService
	{
		private readonly ILogger<ReceiptEdoTaskResendService> _logger;
		private readonly IBus _messageBus;

		public ReceiptEdoTaskResendService(
			ILogger<ReceiptEdoTaskResendService> logger,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		/// <inheritdoc/>
		public bool CanResend(ReceiptEdoTask receiptTask)
		{
			if(receiptTask == null)
			{
				throw new ArgumentNullException(nameof(receiptTask));
			}

			if(receiptTask.Status == EdoTaskStatus.Completed
				|| receiptTask.Status == EdoTaskStatus.InCancellation
				|| receiptTask.Status == EdoTaskStatus.Cancelled)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} находится в терминальном статусе {TaskStatus} и не может быть запущена повторно",
					receiptTask.Id,
					receiptTask.Status);
				return false;
			}

			if(receiptTask.ReceiptStatus != EdoReceiptStatus.New)
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} находится в статусе чека {ReceiptStatus}. Повторный запуск возможен только в статусе New",
					receiptTask.Id,
					receiptTask.ReceiptStatus);
				return false;
			}

			if(receiptTask.Items.Any(x =>
				x.ProductCode != null
				&& x.ProductCode.SourceCodeStatus == SourceProductCodeStatus.SavedToPool))
			{
				_logger.LogWarning(
					"Задача ЭДО {EdoTaskId} содержит коды, уже сохраненные в пул, и не может быть запущена повторно",
					receiptTask.Id);
				return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public Task PublishResendEventAsync(
			ReceiptEdoTask receiptTask,
			CancellationToken cancellationToken)
		{
			if(receiptTask == null)
			{
				throw new ArgumentNullException(nameof(receiptTask));
			}

			return _messageBus.Publish(
				new ReceiptTaskCreatedEvent { ReceiptEdoTaskId = receiptTask.Id },
				cancellationToken);
		}
	}
}
