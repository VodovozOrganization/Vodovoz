using Edo.Contracts.Messages.Events;
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
	public class ReceiptSender
	{
		private readonly ILogger<ReceiptSender> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly CashboxClientProvider _cashboxClientProvider;
		private readonly FiscalDocumentFactory _fiscalDocumentFactory;
		private readonly IBus _messageBus;

		public ReceiptSender(
			ILogger<ReceiptSender> logger,
			IUnitOfWorkFactory uowFactory,
			CashboxClientProvider cashboxClientProvider,
			FiscalDocumentFactory fiscalDocumentFactory,
			IBus messageBus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
			_fiscalDocumentFactory = fiscalDocumentFactory ?? throw new ArgumentNullException(nameof(fiscalDocumentFactory));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task HandleReceiptSendEvent(int edoTaskId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var edoTask = uow.GetById<ReceiptEdoTask>(edoTaskId);
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



					// Это скорее всего не нужно, будет блокироваать повторную отправку проблемных задач
					case EdoTaskStatus.Problem:
						_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} " +
							"имеет не решенные проблемы.",
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
					// зарегистрировать проблему
					// перенести в валидацию
					// добавить валидацию для отправки чеков
				}

				object message = null;

				try
				{
					var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(edoTask.CashboxId.Value, cancellationToken);
					
					// отправка чеков
					foreach(var edoFiscalDocument in edoTask.FiscalDocuments)
					{
						var fiscalDocument =  _fiscalDocumentFactory.CreateFiscalDocument(edoFiscalDocument);
						var result = await cashboxClient.SendFiscalDocument(fiscalDocument, cancellationToken);
						if(result.SendStatus == SendStatus.Error)
						{
							edoFiscalDocument.FailureMessage = result.ErrorMessage;
							edoFiscalDocument.Status = Vodovoz.Core.Domain.Edo.FiscalDocumentStatus.Failed;
							continue;
						}

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
						edoTask.Status = EdoTaskStatus.Problem;
						edoTask.Problems.Add(new CustomEdoTaskProblem
						{
							EdoTask = edoTask,
							CustomMessage = "Не удалось отправить некоторые чеки"
						});
					}
					else
					{
						// НЕТ ОЧИСТКИ РЕШЕННЫХ ПРОБЛЕМ
						// может тут было бы правильно из перевести в Solved

						edoTask.ReceiptStatus = EdoReceiptStatus.Sent;
						message = new ReceiptSentEvent { EdoTaskId = edoTask.Id };
					}
				}
				catch(CashboxException ex)
				{
					// ТУТ ЕСТЬ ПРОБЛЕМА
					// задача не сможет сама повторить отправку
					edoTask.Status = EdoTaskStatus.Problem;
					edoTask.Problems.Add(new CustomEdoTaskProblem
					{
						EdoTask = edoTask,
						//Exception = ex.GetType().Name,
						CustomMessage = ex.Message
					});
				}

				await uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				if(message != null)
				{
					await _messageBus.Publish(message, cancellationToken);
				}
			}
		}

		
	}
}
