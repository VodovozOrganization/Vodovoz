using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Validation;
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

		public ReceiptSender(
			ILogger<ReceiptSender> logger,
			IUnitOfWork uow,
			EdoProblemRegistrar edoProblemRegistrar,
			CashboxClientProvider cashboxClientProvider,
			FiscalDocumentFactory fiscalDocumentFactory,
			EdoTaskValidator edoTaskValidator
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
			_fiscalDocumentFactory = fiscalDocumentFactory ?? throw new ArgumentNullException(nameof(fiscalDocumentFactory));
			_edoTaskValidator = edoTaskValidator ?? throw new ArgumentNullException(nameof(edoTaskValidator));
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

			var isValid = await _edoTaskValidator.Validate(edoTask, cancellationToken);
			if(!isValid)
			{
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
				case EdoTaskStatus.Problem:
					_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} " +
						"имеет не решенную проблему.",
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

			if(edoTask.CashboxId == null)
			{
				throw new InvalidOperationException("Не указана касса для отправки чека. Должна проверяться валидацией задачи");
			}

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
					return;
				}
				else
				{
					_edoProblemRegistrar.SolveCustomProblem<NotAllReceiptsWasSended>(edoTask);

					edoTask.ReceiptStatus = EdoReceiptStatus.Sent;
				}
			}
			catch(CashboxException ex)
			{
				_logger.LogWarning(ex, "Ошибка при отправке чека по задаче №{edoTaskId}", edoTask.Id);
				throw;
			}

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			_logger.LogInformation("Все чеки по задаче №{edoTaskId} отправлены успешно.", edoTask.Id);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
