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
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOptionsMonitor<FiscalDocumentSendErrorProblemWorkerOptions> _options;
		private readonly IEdoRepository _edoRepository;
		private readonly IPublishEndpoint _publishEndpoint;

		public FiscalDocumentSendErrorProblemService(
			ILogger<FiscalDocumentSendErrorProblemService> logger,
			IUnitOfWork unitOfWork,
			IOptionsMonitor<FiscalDocumentSendErrorProblemWorkerOptions> options,
			IEdoRepository edoRepository,
			IPublishEndpoint publishEndpoint)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		private DateTime _minFiscalDocumentCreationTime => DateTime.Today - _options.CurrentValue.ProblemTimeout;

		/// <summary>
		/// Обработчик фискальных документов с проблемой отправки в ЭДО
		/// </summary>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns></returns>
		public async Task ProcessProblemFiscalDocuments(CancellationToken cancellationToken)
		{
			var tasksIds =
				await _edoRepository.GetSendErrorFiscalDocumentsEdoTasksIds(_unitOfWork, _minFiscalDocumentCreationTime, cancellationToken);

			await TryResendFiscalDocuments(tasksIds, cancellationToken);

			await _unitOfWork.CommitAsync(cancellationToken);
		}

		private async Task TryResendFiscalDocuments(IEnumerable<int> receiptEdoTaskIds, CancellationToken cancellationToken)
		{
			var successCount = 0;

			foreach(var taskId in receiptEdoTaskIds)
			{
				var success = await TryResendFiscalDocument(taskId, cancellationToken);

				if(success)
				{
					successCount++;
				}
			}

			_logger.LogInformation("Повторная отправка фискальных документов завершена. " +
				"Всего задач для повторной отправки: {TotalCount}. " +
				"Успешно повторно отправлено: {SuccessCount}",
				receiptEdoTaskIds.Count(),
				successCount);
		}

		private async Task<bool> TryResendFiscalDocument(int receiptEdoTaskId, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation(
					"Пытаемся повторно отправить фискальный документ с задачей {ReceiptEdoTaskId}",
					receiptEdoTaskId);

				await _publishEndpoint.Publish(
					new ReceiptReadyToSendEvent { ReceiptEdoTaskId = receiptEdoTaskId },
					cancellationToken);

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
