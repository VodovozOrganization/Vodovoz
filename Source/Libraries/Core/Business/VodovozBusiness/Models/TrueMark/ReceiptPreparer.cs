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

		private async Task PrepareCodes(CashReceipt receipt, CancellationToken cancellationToken)
		{
			PrepareDefectiveCodes(receipt);

			var order = receipt.Order;

			if(order.Client.ReasonForLeaving == ReasonForLeaving.Unknown)
			{
				throw new TrueMarkException($"Невозможно обработать заказ {order.Id}. Неизвестная причина отпуска товара.");
			}

			var receiptNeeded = _cashReceiptRepository.CashReceiptNeeded(_uow, order.Id);
			if(receiptNeeded)
			{

				await PrepareForFirstReceipt(receipt, cancellationToken);
			}
			else
			{
				await PrepareForReceiptNotNeeded(receipt, cancellationToken);
			}
		}

		private async Task PrepareForReceiptNotNeeded(CashReceipt receipt, CancellationToken cancellationToken)
		{
			var order = receipt.Order;
			var codes = receipt.ScannedCodes.Where(x => !x.IsDefectiveSourceCode);

			//В первую очередь необходима проверка на первый чек,
			//если не пройдет тогда все остальное можно проверять будет
			//а если пройдет, то формируем чек
			if(order.PaymentType == PaymentType.cash)
			{
				await PrepareForFirstReceiptIfReceiptNotNeeded(receipt, cancellationToken);
				return;
			}

			TryPutCodesToPool(receipt);

			receipt.Status = CashReceiptStatus.ReceiptNotNeeded;
			receipt.ErrorDescription = null;
			return;
		}

		private async Task PrepareForFirstReceiptIfReceiptNotNeeded(CashReceipt receipt, CancellationToken cancellationToken)
		{
			var needReceiptForFirstSum = _cashReceiptRepository.CashReceiptNeededForFirstCashSum(_uow, receipt.Order.Id);
			if(needReceiptForFirstSum)
			{
				await PrepareForFirstReceipt(receipt, cancellationToken);
			}

			receipt.Status = CashReceiptStatus.ReceiptNotNeeded;
			receipt.ErrorDescription = null;
			return;
		}

		private async Task PrepareForFirstReceipt(CashReceipt receipt, CancellationToken cancellationToken)
		{
			var orderSum = receipt.Order.OrderPositiveOriginalSum;
			var hasReceiptBySum = _cashReceiptRepository.HasReceiptBySum(DateTime.Today, orderSum);
			if(hasReceiptBySum && !receipt.ManualSent)
			{
				receipt.Status = CashReceiptStatus.DuplicateSum;
				receipt.ErrorDescription = null;
				return;
			}
			else
			{
				await PrepareForReadyToSend(receipt, cancellationToken);
			}
		}

		private async Task PrepareForReadyToSend(CashReceipt receipt, CancellationToken cancellationToken)
		{
			var codes = receipt.ScannedCodes.Where(x => !x.IsDefectiveSourceCode);

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
			receipt.Status = CashReceiptStatus.ReadyToSend;
			receipt.ErrorDescription = null;
		}

		private void TryPutCodesToPool(CashReceipt receipt)
		{
			var order = receipt.Order;
			if(order.Client.PersonType != PersonType.legal)
			{
				return;
			}

			var codes = receipt.ScannedCodes.Where(x => !x.IsDefectiveSourceCode);
			switch(order.Client.ReasonForLeaving)
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

		private void PrepareDefectiveCodes(CashReceipt receipt)
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

		private void PrepareUnscannedAndExtraScannedCodes(CashReceipt receipt)
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
					var newCode = new CashReceiptProductCode();
					newCode.CashReceipt = receipt;
					newCode.OrderItem = orderItem;
					newCode.IsUnscannedSourceCode = true;
					newCode.ResultCode = GetCodeFromPool();

					_uow.Save(newCode);
					receipt.ScannedCodes.Add(newCode);
				}
			}
		}

		private void PutCodesToPool(IEnumerable<CashReceiptProductCode> codes)
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
				var trueMarkOrder = uow.GetById<CashReceipt>(_receiptId);
				trueMarkOrder.Status = CashReceiptStatus.CodeError;
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
