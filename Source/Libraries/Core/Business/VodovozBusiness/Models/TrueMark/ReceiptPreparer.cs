using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Models.TrueMark
{
	public class ReceiptPreparer : IDisposable
	{
		private readonly ILogger<ReceiptPreparer> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IUnitOfWork _uow;
		private readonly TrueMarkTransactionalCodesPool _codesPool;
		private readonly TrueMarkCodesChecker _codeChecker;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private readonly int _receiptId;
		private bool _disposed;

		public ReceiptPreparer(
			ILogger<ReceiptPreparer> logger,
			IUnitOfWorkFactory uowFactory,
			TrueMarkTransactionalCodesPool codesPool,
			TrueMarkCodesChecker codeChecker,
			ICashReceiptRepository cashReceiptRepository,
			int receiptId)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_codesPool = codesPool ?? throw new ArgumentNullException(nameof(codesPool));
			_codeChecker = codeChecker ?? throw new ArgumentNullException(nameof(codeChecker));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			if(receiptId <= 0)
			{
				throw new ArgumentException("Должен быть указан существующий Id чека.", nameof(receiptId));
			}
			_receiptId = receiptId;
			_uow = _uowFactory.CreateWithoutRoot();
		}

		public async Task PrepareAsync(CancellationToken cancellationToken)
		{
			if(_disposed)
			{
				throw new ObjectDisposedException(nameof(ReceiptPreparer));
			}

			try
			{
				await PrepareReceipt(cancellationToken);
			}
			catch(Exception ex)
			{
				RollbackPool();
				RegisterException(ex);
			}
			finally
			{
				Dispose();
			}
		}

		private async Task PrepareReceipt(CancellationToken cancellationToken)
		{
			var receipt = _cashReceiptRepository.LoadReceipt(_uow, _receiptId);

			await PrepareCodes(receipt, cancellationToken);

			_uow.Save(receipt);
			_uow.Commit();
			CommitPool();
		}

		private async Task PrepareCodes(TrueMarkCashReceiptOrder receipt, CancellationToken cancellationToken)
		{
			PrepareDefectiveCodes(receipt);

			//Если для перепродажи, то сразу забыли про отбирание кодов

			//Если перепродажа и не нужен чек, ставим чек не требуется, коды забываем
			//Если перепродажа и нужен чек, собираем коды клиента и формируем чек с указанием ИНН клиента. (РЕШЕНИЕ: Формируем так как обычно) ПРОБЛЕМА! Нужны точные коды которые будем передавать, нельзя подбирать коды из пула!

			//Если собст. нужд и не нужен чек, то берем себе его коды в пул.
			//Если для собст. нужд и нужен чек, то формируем чек из его кодов + недостающие из пула и отправляем обычный чек с кодами.

			var order = receipt.Order;
			var reasonForLeaving = order.Client.ReasonForLeaving;

			if(reasonForLeaving == ReasonForLeaving.Unknown)
			{
				throw new TrueMarkException($"Невозможно обработать заказ {order.Id}. Неизвестная причина отпуска товара.");
			}

			var codes = receipt.ScannedCodes.Where(x => !x.IsDefectiveSourceCode);

			var receiptNeeded = _cashReceiptRepository.CashReceiptNeeded(_uow, order.Id);
			if(!receiptNeeded)
			{
				if(order.Client.PersonType == PersonType.legal)
				{
					switch(reasonForLeaving)
					{
						case ReasonForLeaving.ForOwnNeeds:
						case ReasonForLeaving.Other:
							PutCodesToPool(codes);
							break;
						case ReasonForLeaving.Resale:
						case ReasonForLeaving.Unknown:
						default:
							break;
					}
				}

				receipt.Status = TrueMarkCashReceiptOrderStatus.ReceiptNotNeeded;
				receipt.ErrorDescription = null;
				return;
			}

			//valid codes
			var validCodes = codes.Where(x => x.IsValid);
			var checkResults = await _codeChecker.CheckCodesAsync(validCodes, cancellationToken);
			foreach(var checkResult in checkResults)
			{
				var code = checkResult.Code;
				if(checkResult.Introduced)
				{
					code.ResultCode = code.SourceCode;
				}
				else
				{
					code.ResultCode = GetCodeFromPool();
				}

				_uow.Save(code);
			}

			//invalid codes
			var invalidCodes = codes.Where(x => !x.IsValid);
			foreach(var invalidCode in invalidCodes)
			{
				invalidCode.ResultCode = GetCodeFromPool();
			}

			//unscanned codes
			PrepareUnscannedAndExtraScannedCodes(receipt);

			//states
			receipt.Status = TrueMarkCashReceiptOrderStatus.ReadyToSend;
			receipt.ErrorDescription = null;
		}

		private void PrepareDefectiveCodes(TrueMarkCashReceiptOrder receipt)
		{
			var defectiveCodes = receipt.ScannedCodes.Where(x => x.IsDefectiveSourceCode);
			foreach(var defectiveCode in defectiveCodes)
			{
				if(defectiveCode.SourceCode != null && !defectiveCode.SourceCode.IsInvalid)
				{
					_codesPool.PutDefectiveCode(defectiveCode.SourceCode.Id);
				}

				if(defectiveCode.ResultCode != null)
				{
					continue;
				}

				defectiveCode.ResultCode = GetCodeFromPool();

				_uow.Save(defectiveCode);
			}
		}

		private void PrepareUnscannedAndExtraScannedCodes(TrueMarkCashReceiptOrder receipt)
		{
			var orderItems = receipt.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark);
			foreach(var orderItem in orderItems)
			{
				var codes = receipt.ScannedCodes.Where(x => x.OrderItem.Id == orderItem.Id);
				var unscannedCodesCount = orderItem.Count - codes.Count();
				if(unscannedCodesCount == 0)
				{
					continue;
				}

				//Extra scanned codes
				if(unscannedCodesCount < 0)
				{
					var extraCodes = codes.Skip((int)orderItem.Count);
					foreach(var extraCode in extraCodes.ToList())
					{
						if(!extraCode.IsValid)
						{
							continue;
						}

						_codesPool.PutCode(extraCode.SourceCode.Id);
						receipt.ScannedCodes.Remove(extraCode);
						_uow.Delete(extraCode);
					}

					continue;
				}

				//Unscanned codes
				for(int i = 0; i < unscannedCodesCount; i++)
				{
					var newCode = new TrueMarkCashReceiptProductCode();
					newCode.CashReceipt = receipt;
					newCode.OrderItem = orderItem;
					newCode.IsUnscannedSourceCode = true;
					newCode.ResultCode = GetCodeFromPool();

					_uow.Save(newCode);
					receipt.ScannedCodes.Add(newCode);
				}
			}
		}

		private void PutCodesToPool(IEnumerable<TrueMarkCashReceiptProductCode> codes)
		{
			foreach(var code in codes)
			{
				if(!code.IsValid)
				{
					continue;
				}

				_codesPool.PutCode(code.SourceCode.Id);
			}
		}

		private TrueMarkWaterIdentificationCode GetCodeFromPool()
		{
			var codeId = _codesPool.TakeCode();
			return _uow.GetById<TrueMarkWaterIdentificationCode>(codeId);
		}

		private void CommitPool()
		{
			//ошибка пула кода не важна для основной работы
			try
			{
				_codesPool.Commit();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка коммита пула кодов.");
			}
		}

		private void RollbackPool()
		{
			//ошибка пула кода не важна для основной работы
			try
			{
				_codesPool.Rollback();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка отката пула кодов.");
			}
		}

		private void RegisterException(Exception ex)
		{
			_logger.LogError(ex, $"Ошибка обработки заказа честного знака {_receiptId}.");
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var trueMarkOrder = uow.GetById<TrueMarkCashReceiptOrder>(_receiptId);
				trueMarkOrder.Status = TrueMarkCashReceiptOrderStatus.CodeError;
				trueMarkOrder.ErrorDescription = ex.Message;
				uow.Save(trueMarkOrder);
				uow.Commit();
			}
		}

		public void Dispose()
		{
			_codesPool.Rollback();
			_uow?.Dispose();
			_disposed = true;
		}
	}
}
