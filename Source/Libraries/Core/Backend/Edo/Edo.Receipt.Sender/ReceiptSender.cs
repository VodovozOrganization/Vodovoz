using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;
using ModulKassa;
using ModulKassa.DTO;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Receipt.Sender
{
	public class ReceiptSender : IDisposable
	{
		private readonly ILogger<ReceiptSender> _logger;
		private readonly IUnitOfWork _uow;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly CashboxClientProvider _cashboxClientProvider;
		private readonly FiscalDocumentFactory _fiscalDocumentFactory;
		private readonly EdoTaskValidator _edoTaskValidator;
		private readonly IBus _messageBus;

		public ReceiptSender(
			ILogger<ReceiptSender> logger,
			IUnitOfWork uow,
			EdoProblemRegistrar edoProblemRegistrar,
			CashboxClientProvider cashboxClientProvider,
			FiscalDocumentFactory fiscalDocumentFactory,
			EdoTaskValidator edoTaskValidator,
			IBus messageBus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
			_fiscalDocumentFactory = fiscalDocumentFactory ?? throw new ArgumentNullException(nameof(fiscalDocumentFactory));
			_edoTaskValidator = edoTaskValidator ?? throw new ArgumentNullException(nameof(edoTaskValidator));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task HandleReceiptSendEvent(int edoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = _uow.GetById<ReceiptEdoTask>(edoTaskId);
			if(edoTask == null)
			{
				_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} не найдена.",
					edoTaskId);
				return;
			}

			if(edoTask.ReceiptStatus != EdoReceiptStatus.Sending)
			{
				_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} " +
					"находится в статусе {receiptTaskStatus} , " +
					"а должна быть в статусе {sendingStatus}.",
					edoTaskId, edoTask.ReceiptStatus, EdoReceiptStatus.Sending);
				return;
			}

			switch(edoTask.Status)
			{
				case EdoTaskStatus.New:
					_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} " +
						"новая и не находится в работе.",
						edoTaskId);
					return;
				case EdoTaskStatus.Waiting:
					_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} " +
						"находится в ожидании решения внешних факторов и не находится в работе.",
						edoTaskId);
					return;
				case EdoTaskStatus.Completed:
					_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} " +
						"уже завершена.",
						edoTaskId);
					return;
				case EdoTaskStatus.InProgress:
					// Корректный статус задачи, продолжаем выполнение
					break;
				default:
					throw new InvalidOperationException($"Неизвестный статус задачи ЭДО {edoTask.Status}");
			}

			var isValid = await _edoTaskValidator.Validate(edoTask, cancellationToken);
			if(!isValid)
			{
				return;
			}

			if(edoTask.CashboxId == null)
			{
				// зарегистрировать проблему
				// перенести в валидацию
				// добавить валидацию для отправки чеков
				throw new InvalidOperationException("Не указана касса для отправки чека.");
			}

			object message = null;

			try
			{
				var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(edoTask.CashboxId.Value, cancellationToken);
					
				// отправка чеков
				foreach(var edoFiscalDocument in edoTask.FiscalDocuments)
				{
					var fiscalDocument =  _fiscalDocumentFactory.CreateFiscalDocument(edoFiscalDocument);
					var result = await cashboxClient.CheckFiscalDocument(fiscalDocument, cancellationToken);
					if(result.SendStatus == SendStatus.Error)
					{
						result = await cashboxClient.SendFiscalDocument(fiscalDocument, cancellationToken);
					}
					if(result.SendStatus == SendStatus.Error)
					{
						edoFiscalDocument.FailureMessage = result.ErrorMessage;
						edoFiscalDocument.Status = Vodovoz.Core.Domain.Edo.FiscalDocumentStatus.Failed;
						continue;
					}

					_logger.LogInformation("Чек №{documentNumber} отправлен успешно.", edoFiscalDocument.DocumentNumber);

					edoFiscalDocument.Stage = FiscalDocumentStage.Sent;
					edoFiscalDocument.Status = ReceiptConverters.ConvertFiscalDocumentStatus(result.FiscalDocumentInfo.Status);
					edoFiscalDocument.StatusChangeTime = DateTime.Parse(result.FiscalDocumentInfo.TimeStatusChangedString);
					if(result.FiscalDocumentInfo.FiscalInfo != null)
					{
						edoFiscalDocument.FiscalTime = DateTime.Parse(result.FiscalDocumentInfo.FiscalInfo.Date);
						edoFiscalDocument.FiscalNumber = result.FiscalDocumentInfo.FiscalInfo.FnDocNumber.ToString();
						edoFiscalDocument.FiscalMark = result.FiscalDocumentInfo.FiscalInfo.FnDocMark.ToString();
						edoFiscalDocument.FiscalKktNumber = result.FiscalDocumentInfo.FiscalInfo.KktNumber;
					}
				}

				var hasFailure = edoTask.FiscalDocuments.Any(x => x.Status == Vodovoz.Core.Domain.Edo.FiscalDocumentStatus.Failed);
				if(hasFailure)
				{
					_logger.LogWarning("Не удалось отправить некоторые чеки по задаче №{edoTaskId}.", edoTask.Id);
					await _edoProblemRegistrar.RegisterCustomProblem<NotAllReceiptsWasSended>(
						edoTask,
						cancellationToken
					);
				}
				else
				{
					_edoProblemRegistrar.SolveCustomProblem<NotAllReceiptsWasSended>(edoTask);

					edoTask.ReceiptStatus = EdoReceiptStatus.Sent;
					message = new ReceiptSentEvent { ReceiptEdoTaskId = edoTask.Id };
				}
			}
			catch(CashboxException ex)
			{
				_logger.LogWarning("Ошибка при отправке чека по задаче №{edoTaskId}.", edoTask.Id);
				throw;
			}

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			_logger.LogInformation("Все чеки по задаче №{edoTaskId} отправлены успешно.", edoTask.Id);

			if(message != null)
			{
				await _messageBus.Publish(message, cancellationToken);
			}
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
