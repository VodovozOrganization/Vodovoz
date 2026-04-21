using Edo.Contracts.Messages.Events;
using Edo.Problem.Routine.Options;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Сервис обработки проблем с отправкой фискальных документов в ЭДО
	/// </summary>
	public class FiscalDocumentSendErrorProblemService
	{
		private readonly ILogger<FiscalDocumentSendErrorProblemService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptionsMonitor<FiscalDocumentSendErrorProblemWorkerOptions> _options;
		private readonly IEdoRepository _edoRepository;
		private readonly IBus _messageBus;

		public FiscalDocumentSendErrorProblemService(
			ILogger<FiscalDocumentSendErrorProblemService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptionsMonitor<FiscalDocumentSendErrorProblemWorkerOptions> options,
			IEdoRepository edoRepository,
			IBus messageBus)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		private DateTime _minFiscalDocumentCreationTime => DateTime.Today - _options.CurrentValue.ProblemTimeout;

		/// <summary>
		/// Обработчик фискальных документов с проблемой отправки в ЭДО
		/// </summary>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns></returns>
		public async Task ProcessProblemFiscalDocuments(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(FiscalDocumentSendErrorProblemService)))
			{
				var tasksIds =
					await _edoRepository.GetSendErrorFiscalDocumentsEdoTasksIds(uow, _minFiscalDocumentCreationTime, cancellationToken);

				await TryResendFiscalDocuments(tasksIds);
			}
		}

		private async Task TryResendFiscalDocuments(IEnumerable<int> receiptEdoTaskIds)
		{
			var successCount = 0;

			foreach(var taskId in receiptEdoTaskIds)
			{
				var success = await TryResendFiscalDocument(taskId);

				successCount =
					success
					? successCount++
					: successCount;
			}

			_logger.LogInformation("Повторная отправка фискальных документов завершена. " +
				"Всего задач для повторной отправки: {TotalCount}. " +
				"Успешно повторно отправлено: {SuccessCount}",
				receiptEdoTaskIds.Count(),
				successCount);
		}

		private async Task<bool> TryResendFiscalDocument(int receiptEdoTaskId)
		{
			try
			{
				_logger.LogInformation(
					"Пытаемся повторно отправить фискальный документ с задачей {ReceiptEdoTaskId}",
					receiptEdoTaskId);

				await _messageBus.Publish(new ReceiptReadyToSendEvent { ReceiptEdoTaskId = receiptEdoTaskId });

				_logger.LogInformation(
					"Повторная отправка фискального документа с задачей {ReceiptEdoTaskId} успешно инициирована",
					receiptEdoTaskId);

				return true;

			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при попытке повторной отправки фискального документа с задачей {ReceiptEdoTaskId}",
					receiptEdoTaskId);

				return false;
			}
		}
	}
}
