using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModulKassa;
using ModulKassa.DTO;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using FiscalDocumentStatus = Vodovoz.Core.Domain.Edo.FiscalDocumentStatus;

namespace Edo.Receipt.Callback.Controllers
{
	[ApiController]
	[Route("callback")]
	public class ReceiptCallbackController : ControllerBase
	{
		private readonly ILogger<ReceiptCallbackController> _logger;
		private readonly CashboxClientProvider _cashboxClientProvider;

		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IPublishEndpoint _publishEndpoint;

		public ReceiptCallbackController(
			ILogger<ReceiptCallbackController> logger,
			CashboxClientProvider cashboxClientProvider,
			IUnitOfWorkFactory uowFactory,
			IPublishEndpoint publishEndpoint
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		[HttpGet()]
		[Route("complete/{receiptGuid}")]
		public async Task CallEvent(
			[FromRoute] string receiptGuid,
			[FromQuery] string status,
			[FromQuery] string qr,
			CancellationToken cancellationToken
			)
		{
			if(status != "SUCCESS")
			{
				_logger.LogWarning("Неизвестный статус уведомления о завершении чека. Статус: {status}", status);
				HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return;
			}

			if(!Guid.TryParse(receiptGuid, out Guid guid))
			{
				_logger.LogWarning("Неверный формат GUID чека: {receiptGuid}", receiptGuid);
				HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return;
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var document = uow.Session.QueryOver<EdoFiscalDocument>()
					.Where(x => x.DocumentGuid == guid)
					.SingleOrDefault();

				if(document == null)
				{
					_logger.LogWarning("Чек с GUID {receiptGuid} не найден.", receiptGuid);
					HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
					return;
				}

				if(document.Status == FiscalDocumentStatus.Completed)
				{
					_logger.LogWarning("Чек с GUID {receiptGuid} уже завершен.", receiptGuid);
					HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
					return;
				}

				var updated = await TryUpdateReceiptInfoFromCashbox(document, cancellationToken);
				if(!updated)
				{
					_logger.LogWarning("Не удалось получить подробную информацию о чеке из кассы, " +
						"поэтому записываем только имеющиеся номер и марку документа из события завершения чека.");

					var qrParts = qr.Split("&");
					var fiscalDocNumber = qrParts.First(x => x.StartsWith("i")).Substring(2).TrimStart('0');
					var fiscalDocMark = qrParts.First(x => x.StartsWith("fp")).Substring(3).TrimStart('0');

					document.FiscalNumber = fiscalDocNumber;
					document.FiscalMark = fiscalDocMark;

					document.Status = FiscalDocumentStatus.Completed;
					document.Stage = FiscalDocumentStage.Completed;
					document.StatusChangeTime = DateTime.Now;
				}

				await uow.SaveAsync(document, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				try
				{
					if(document.Status is FiscalDocumentStatus.Completed or FiscalDocumentStatus.Printed
						&& document.Stage == FiscalDocumentStage.Completed)
					{
						await _publishEndpoint.Publish(
							new ReceiptCompleteEvent
							{
								ReceiptEdoTaskId =  document.ReceiptEdoTask.Id
							},
							cancellationToken);
					}
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Не удалось отправить событие завершения задачи по чеку {FiscalDocument}", document.DocumentGuid);
				}

				HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
			}
		}

		private async Task<bool> TryUpdateReceiptInfoFromCashbox(EdoFiscalDocument document, CancellationToken cancellationToken)
		{
			var cashboxId = document.ReceiptEdoTask.CashboxId;
			if(cashboxId == null)
			{
				_logger.LogWarning("Невозможно загрузить подробную информацию о чеке, " +
					"так как в ЭДО задаче {edoTaskId} не указан id кассы.",
					document.ReceiptEdoTask.Id);
				return false;
			}

			var cashbox = await _cashboxClientProvider.GetCashboxAsync(cashboxId.Value, cancellationToken);
			var checkResult = await cashbox.CheckFiscalDocument(document.DocumentGuid.ToString(), cancellationToken);

			if(checkResult.SendStatus == SendStatus.Error)
			{
				_logger.LogWarning("Возникла проблема с проверкой состояния чека: {errorMessage}",
					checkResult.ErrorMessage);
			}

			document.FiscalNumber = checkResult.FiscalDocumentInfo.FiscalInfo.FnDocNumber.ToString();
			document.FiscalMark = checkResult.FiscalDocumentInfo.FiscalInfo.FnDocMark.ToString();
			document.FiscalKktNumber = checkResult.FiscalDocumentInfo.FiscalInfo.EcrRegistrationNumber;

			document.Status = ReceiptConverters.ConvertFiscalDocumentStatus(checkResult.FiscalDocumentInfo.Status);
			document.StatusChangeTime = DateTime.Parse(checkResult.FiscalDocumentInfo.TimeStatusChangedString);
			document.FiscalTime = DateTime.Parse(checkResult.FiscalDocumentInfo.FiscalInfo.Date);

			document.Stage = FiscalDocumentStage.Completed;

			return true;
		}
	}
}
