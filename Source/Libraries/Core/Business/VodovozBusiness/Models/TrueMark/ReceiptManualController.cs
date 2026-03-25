using CashReceiptApi;
using CashReceiptApi.Client.Framework;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.TrueMark;
using CashReceiptErrors = Vodovoz.Errors.CashReceipts.CashReceiptErrors;
using RequestProcessingResult = CashReceiptApi.RequestProcessingResult;

namespace Vodovoz.Models.TrueMark
{
	public class ReceiptManualController
	{
		private readonly ILogger<ReceiptManualController> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly CashReceiptClientChannelFactory _cashReceiptClientChannelFactory;
		private readonly IInteractiveMessage _interactiveMessage;

		public ReceiptManualController(
			ILogger<ReceiptManualController> logger,
			IUnitOfWorkFactory uowFactory,
			CashReceiptClientChannelFactory cashReceiptClientChannelFactory,
			IInteractiveMessage interactiveMessage)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_cashReceiptClientChannelFactory = cashReceiptClientChannelFactory ?? throw new ArgumentNullException(nameof(cashReceiptClientChannelFactory));
			_interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		public void ForceSendDuplicatedReceipt(int receiptId)
		{
			if(receiptId <= 0)
			{
				throw new ArgumentException("Должен быть указан валидный код чека", nameof(receiptId));
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var cashReceipt = uow.GetById<CashReceipt>(receiptId);
				if(cashReceipt.Status != CashReceiptStatus.DuplicateSum && cashReceipt.Status != CashReceiptStatus.ReceiptNotNeeded)
				{
					throw new InvalidOperationException(
						"Принудительная отправка чека возможна если это чек дубль или чек не требуется");
				}

				cashReceipt.ManualSent = true;
				cashReceipt.Status = CashReceiptStatus.New;
				uow.Save(cashReceipt);
				uow.Commit();
			}
		}

		public Result RefreshFiscalDoc(int receiptId)
		{
			_logger.LogInformation("Начинаем обновление статуса фискального документа чека. Id чека: {ReceiptId}", receiptId);

			if(receiptId <= 0)
			{
				_logger.LogCritical(
					"Ошибка при обновлении статуса фискального документа чека. Должен быть указан валидный код чека. Переданное значение: {ReceiptId}",
					receiptId);

				return Result.Failure(CashReceiptErrors.CashReceiptIdNotValid);
			}

			var request = new RefreshReceiptRequest
			{
				CashReceiptId = receiptId
			};

			RequestProcessingResult response = null;

			try
			{
				using(var receiptServiceChannel = _cashReceiptClientChannelFactory.OpenChannel())
				{
					response = receiptServiceChannel.Client.RefreshFiscalDocument(request);

					if(response.IsSuccess)
					{
						_logger.LogInformation("Обновление статуса фискального документа чека выполнено успешно");

						return Result.Success();
					}

					_logger.LogCritical("Обновление статуса фискального документа чека завершилось неудачей. Детали: {ResponseError}", response.Error);
					return Result.Failure(CashReceiptErrors.CreateCashReceiptApiRequestProcessingError(response.Error));
				}
			}
			catch(RpcException ex)
			{
				Error error = null;

				if(ex.StatusCode == StatusCode.Unauthenticated)
				{
					error = CashReceiptErrors.CashReceiptApiUnauthenticatedError;
				}
				else
				{
					error = CashReceiptErrors.CashReceiptApiServiceUnavailableError;
				}

				_logger.LogCritical(ex, error.Message);

				return Result.Failure(error);
			}
			catch(Exception ex)
			{
				var message = $"При попытке обновления статуса фискального документа чека произошла ошибка:\n{ex.Message}";
				_logger.LogCritical(ex, message);

				return Result.Failure(CashReceiptErrors.CreateCashReceiptApiRequestProcessingError(message));
			}
		}

		public Result RequeueFiscalDoc(int receiptId)
		{
			_logger.LogInformation("Начинаем повторное проведение чека. Id чека: {ReceiptId}", receiptId);

			if(receiptId <= 0)
			{
				_logger.LogCritical(
					"Ошибка при повторном проведении чека. Должен быть указан валидный код чека. Переданное значение: {ReceiptId}",
					receiptId);

				return Result.Failure(CashReceiptErrors.CashReceiptIdNotValid);
			}

			var request = new RequeueDocumentRequest
			{
				CashReceiptId = receiptId
			};

			RequestProcessingResult response = null;

			try
			{
				using(var receiptServiceChannel = _cashReceiptClientChannelFactory.OpenChannel())
				{
					response = receiptServiceChannel.Client.RequeueFiscalDocument(request);

					if(response.IsSuccess)
					{
						_logger.LogInformation("Повторное проведение чека выполнено успешно");

						return Result.Success();
					}

					_logger.LogCritical("Повторное проведение чека завершилось неудачей. Детали: {ResponseError}", response.Error);
					return Result.Failure(CashReceiptErrors.CreateCashReceiptApiRequestProcessingError(response.Error));
				}
			}
			catch(RpcException ex)
			{
				Error error = null;

				if(ex.StatusCode == StatusCode.Unauthenticated)
				{
					error = CashReceiptErrors.CashReceiptApiUnauthenticatedError;
				}
				else
				{
					error = CashReceiptErrors.CashReceiptApiServiceUnavailableError;
				}

				_logger.LogCritical(ex, error.Message);

				return Result.Failure(error);
			}
			catch(Exception ex)
			{
				var message = $"При попытке повторного проведения чека произошла ошибка:\n{ex.Message}";
				_logger.LogCritical(ex, message);

				return Result.Failure(CashReceiptErrors.CreateCashReceiptApiRequestProcessingError(message));
			}
		}
	}
}
