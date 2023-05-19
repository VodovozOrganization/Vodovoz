using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Factories;

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
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly ICashReceiptFactory _cashReceiptFactory;
		private readonly int _receiptId;
		private ISet<string> _ownersInn;
		private IList<CashReceipt> _cashReceiptsToSave = new List<CashReceipt>();
		private bool _disposed;

		public ReceiptPreparer(
			ILogger<ReceiptPreparer> logger,
			IUnitOfWorkFactory uowFactory,
			TrueMarkTransactionalCodesPool codesPool,
			TrueMarkCodesChecker codeChecker,
			ICashReceiptRepository cashReceiptRepository,
			ITrueMarkRepository trueMarkRepository,
			ICashReceiptFactory cashReceiptFactory,
			int receiptId)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_codesPool = codesPool ?? throw new ArgumentNullException(nameof(codesPool));
			_codeChecker = codeChecker ?? throw new ArgumentNullException(nameof(codeChecker));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_cashReceiptFactory = cashReceiptFactory ?? throw new ArgumentNullException(nameof(cashReceiptFactory));

			if(receiptId <= 0)
			{
				throw new ArgumentException("Должен быть указан существующий Id чека.", nameof(receiptId));
			}
			_receiptId = receiptId;
			_uow = _uowFactory.CreateWithoutRoot();

			_ownersInn = _trueMarkRepository.GetAllowedCodeOwnersInn();
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
			_cashReceiptsToSave.Add(receipt);

			await PrepareCodes(receipt, cancellationToken);

			foreach(var cashReceipt in _cashReceiptsToSave)
			{
				_uow.Save(cashReceipt);
			}
			
			_uow.Commit();
			CommitPool();
		}

		private async Task PrepareCodes(CashReceipt receipt, CancellationToken cancellationToken)
		{
			PrepareDefectiveCodes(receipt);

			var receiptNeeded = _cashReceiptRepository.CashReceiptNeeded(_uow, receipt.Order.Id);
			if(receiptNeeded && !receipt.ManualSent)
			{
				await PrepareForFirstReceipt(receipt, cancellationToken);
			}
			else if(receipt.ManualSent)
			{
				await PrepareForReSendReceipt(receipt, cancellationToken);
			}
			else
			{
				await PrepareForReceiptNotNeeded(receipt, cancellationToken);
			}
		}

		private async Task PrepareForReSendReceipt(CashReceipt receipt, CancellationToken cancellationToken)
		{
			await ProcessingReceiptBeforeSending(receipt, cancellationToken);
		}

		private async Task PrepareForReceiptNotNeeded(CashReceipt receipt, CancellationToken cancellationToken)
		{
			if(receipt.Order.PaymentType == PaymentType.cash)
			{
				await PrepareForFirstReceiptIfReceiptNotNeeded(receipt, cancellationToken);
				return;
			}

			TryPutCodesToPool(receipt);

			receipt.Status = CashReceiptStatus.ReceiptNotNeeded;
			receipt.ErrorDescription = null;
		}

		private async Task PrepareForFirstReceiptIfReceiptNotNeeded(CashReceipt receipt, CancellationToken cancellationToken)
		{
			var needReceiptForFirstSum = _cashReceiptRepository.CashReceiptNeededForFirstCashSum(_uow, receipt.Order.Id);
			if(needReceiptForFirstSum)
			{
				await PrepareForFirstReceipt(receipt, cancellationToken);
				return;
			}

			TryPutCodesToPool(receipt);

			receipt.Status = CashReceiptStatus.ReceiptNotNeeded;
			receipt.ErrorDescription = null;
		}

		private async Task PrepareForFirstReceipt(CashReceipt receipt, CancellationToken cancellationToken)
		{
			if(receipt.Order.PaymentType == PaymentType.cash && !receipt.Order.Client.AlwaysSendReceipts)
			{
				var orderSum = receipt.Order.OrderPositiveOriginalSum;
				//не проверяем дубли по сумме у чеков под заказы  с 128+ позиций
				var hasReceiptBySum = !receipt.InnerNumber.HasValue && _cashReceiptRepository.HasReceiptBySum(DateTime.Today, orderSum);
				if(hasReceiptBySum)
				{
					TryPutCodesToPool(receipt);

					receipt.Status = CashReceiptStatus.DuplicateSum;
					receipt.ErrorDescription = null;
					return;
				}
			}

			await ProcessingReceiptBeforeSending(receipt, cancellationToken);
		}

		private async Task PrepareForReadyToSend(CashReceipt receipt, CancellationToken cancellationToken)
		{
			var order = receipt.Order;
			if(order.Client.ReasonForLeaving == ReasonForLeaving.Unknown
				&& order.OrderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark))
			{
				throw new TrueMarkException($"Невозможно обработать заказ {order.Id}. Неизвестная причина отпуска товара.");
			}

			var codes = receipt.ScannedCodes.Where(x => !x.IsDefectiveSourceCode);

			//valid codes
			var validCodes = codes.Where(x => x.IsValid).ToList();
			var checkResults = await _codeChecker.CheckCodesAsync(validCodes, cancellationToken);
			foreach(var checkResult in checkResults)
			{
				var code = checkResult.Code;
				var isOurOrganizationOwner = _ownersInn.Contains(checkResult.OwnerInn);
				if(!isOurOrganizationOwner)
				{
					_logger.LogInformation("У проверенного кода {serialNumber} владелец не наша организация {organizationINN}. Код исключается из обработки.", 
						code?.SourceCode?.SerialNumber, 
						checkResult.OwnerInn);
				}

				if(checkResult.Introduced && isOurOrganizationOwner)
				{
					if(code.ResultCode != null && !code.ResultCode.Equals(code.SourceCode))
					{
						_codesPool.PutCode(code.ResultCode.Id);
					}

					code.ResultCode = code.SourceCode;
				}
				else
				{
					if(code.ResultCode != null)
					{
						_codesPool.PutCode(code.ResultCode.Id);
					}

					code.ResultCode = GetCodeFromPool(code.OrderItem.Nomenclature.Gtin);
				}
			}

			//invalid codes
			ProcessingInvalidCodes(receipt, codes, validCodes.Count);

			//unscanned codes
			PrepareUnscannedAndExtraScannedCodes(receipt);

			//states
			receipt.Status = CashReceiptStatus.ReadyToSend;
			receipt.ErrorDescription = null;
		}

		private void ProcessingInvalidCodes(CashReceipt receipt, IEnumerable<CashReceiptProductCode> codes, int validCodesCount)
		{
			var invalidCodes = codes.Where(x => !x.IsValid).ToList();
			var receiptCodesCount = validCodesCount + invalidCodes.Count;
			var startNumber = CashReceipt.MaxMarkCodesInReceipt - validCodesCount;
			var startCode = startNumber < 0 ? 0 : startNumber;

			//Если у чека количество кодов превышает норму - обновляем те, которые входят в 128, остальное отправляем в пул
			if(receiptCodesCount > CashReceipt.MaxMarkCodesInReceipt)
			{
				for(var i = 0; i < invalidCodes.Count; i++)
				{
					if(i < startCode)
					{
						if(invalidCodes[i].ResultCode != null)
						{
							_codesPool.PutCode(invalidCodes[i].ResultCode.Id);
						}
						invalidCodes[i].ResultCode = GetCodeFromPool(invalidCodes[i].OrderItem.Nomenclature.Gtin);
					}
					else
					{
						if(invalidCodes[i].ResultCode != null)
						{
							_codesPool.PutCode(invalidCodes[i].ResultCode.Id);
						}
						receipt.ScannedCodes.Remove(invalidCodes[i]);
					}
				}
			}
			else
			{
				foreach(var invalidCode in invalidCodes)
				{
					if(invalidCode.ResultCode != null)
					{
						_codesPool.PutCode(invalidCode.ResultCode.Id);
					}
					invalidCode.ResultCode = GetCodeFromPool(invalidCode.OrderItem.Nomenclature.Gtin);
				}
			}
		}

		private void TryPutCodesToPool(CashReceipt receipt)
		{
			var order = receipt.Order;
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

				defectiveCode.ResultCode = GetCodeFromPool(defectiveCode.OrderItem.Nomenclature.Gtin);

				_uow.Save(defectiveCode);
			}
		}

		private void PrepareUnscannedAndExtraScannedCodes(CashReceipt receipt)
		{
			var markedOrderItems = receipt.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).ToList();
			var markedOrderItemsCount = markedOrderItems.Sum(x => x.Count);
			var codesToSkip = (CashReceipt.MaxMarkCodesInReceipt * receipt.InnerNumber ?? 0) - CashReceipt.MaxMarkCodesInReceipt;

			if(markedOrderItemsCount > CashReceipt.MaxMarkCodesInReceipt && !receipt.InnerNumber.HasValue)
			{
				throw new TrueMarkException($"Невозможно обработать коды по чеку {receipt.Id}. Не указан порядковый номер чека");
			}
			
			if(markedOrderItemsCount <= CashReceipt.MaxMarkCodesInReceipt)
			{
				PrepareUnscannedAndExtraScannedCodes(receipt, markedOrderItems);
			}
			else
			{
				PrepareUnscannedAndExtraScannedCodes(receipt, markedOrderItems, codesToSkip);
			}
		}

		private void PrepareUnscannedAndExtraScannedCodes(CashReceipt receipt, IList<OrderItem> markedOrderItems)
		{
			foreach(var orderItem in markedOrderItems)
			{
				var codes = receipt.ScannedCodes.Where(x => x.OrderItem.Id == orderItem.Id).ToList();
				var unscannedCodesCount = orderItem.Count - codes.Count;
				if(unscannedCodesCount == 0)
				{
					continue;
				}

				//Extra scanned codes
				if(unscannedCodesCount < 0)
				{
					PutValidSourceCodesToPoolAndRemoveFromReceipt(codes.Skip((int)orderItem.Count).ToList(), receipt);
					continue;
				}

				//Unscanned codes
				for(int i = 0; i < unscannedCodesCount; i++)
				{
					AddCodeToReceipt(receipt, orderItem);
				}
			}
		}

		private void PrepareUnscannedAndExtraScannedCodes(CashReceipt receipt, IList<OrderItem> markedOrderItems, int codesToSkip)
		{
			var unprocessedCodesCount = CashReceipt.MaxMarkCodesInReceipt;
			
			foreach(var orderItem in markedOrderItems)
			{
				var orderItemCount = (int)orderItem.Count;

				if(codesToSkip > 0)
				{
					if(codesToSkip > orderItemCount)
					{
						codesToSkip -= orderItemCount;
						continue;
					}

					if(codesToSkip == orderItemCount)
					{
						codesToSkip = 0;
						continue;
					}

					if(codesToSkip < orderItemCount)
					{
						orderItemCount -= codesToSkip;
						codesToSkip = 0;
					}
				}
				
				if(unprocessedCodesCount <= 0)
				{
					break;
				}
				
				var codes = receipt.ScannedCodes.Where(x => x.OrderItem.Id == orderItem.Id);
				var needCodes = orderItemCount <= unprocessedCodesCount ? orderItemCount : unprocessedCodesCount;
				var unscannedCodesCount = needCodes - codes.Count();

				if(unscannedCodesCount == 0)
				{
					unprocessedCodesCount -= needCodes;
					continue;
				}

				//Extra scanned codes
				if(unscannedCodesCount < 0)
				{
					PutValidSourceCodesToPoolAndRemoveFromReceipt(codes.Skip(needCodes).ToList(), receipt);
					unprocessedCodesCount -= needCodes;
					continue;
				}

				//Unscanned codes
				for(var i = 0; i < unscannedCodesCount; i++)
				{
					AddCodeToReceipt(receipt, orderItem);
				}
				
				unprocessedCodesCount -= needCodes;
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

		private TrueMarkWaterIdentificationCode GetCodeFromPool(string gtin)
		{
			var codeId = _codesPool.TakeCode(gtin);
			return _uow.GetById<TrueMarkWaterIdentificationCode>(codeId);
		}

		private bool CheckNeedCreateMoreReceiptsForOrder(Order order, out decimal countReceiptsNeeded)
		{
			var countMarkedNomenclatures = order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.Count);
			countReceiptsNeeded = Math.Ceiling(countMarkedNomenclatures / CashReceipt.MaxMarkCodesInReceipt);
			var countReceiptsForOrder = _cashReceiptRepository.GetCashReceiptsCountForOrder(_uow, order.Id);
			
			return countReceiptsNeeded > countReceiptsForOrder;
		}
		
		private void CreateReceipts(Order order, decimal countReceiptsNeeded)
		{
			for(var i = 1; i < countReceiptsNeeded; i++)
			{
				var receipt = _cashReceiptFactory.CreateNewCashReceipt(order);
				receipt.InnerNumber = i + 1;
				_cashReceiptsToSave.Add(receipt);
			}
		}

		private decimal GetOrderSum(CashReceipt receipt)
		{
			var cashReceiptSum = 0m;
			var maxCodesCount = CashReceipt.MaxMarkCodesInReceipt;
			var receiptNumber = receipt.InnerNumber ?? 0;
			var unprocessedCodesCount = CashReceipt.MaxMarkCodesInReceipt * receiptNumber;
			
			foreach(var orderItem in receipt.Order.OrderItems)
			{
				if(orderItem.Count <= 0)
				{
					continue;
				}

				if(!orderItem.Nomenclature.IsAccountableInTrueMark && receiptNumber == 1)
				{
					cashReceiptSum += orderItem.Sum;
					continue;
				}
				
				if(unprocessedCodesCount == 0)
				{
					continue;
				}

				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					continue;
				}

				if(orderItem.Count == 1)
				{
					if(unprocessedCodesCount > maxCodesCount)
					{
						unprocessedCodesCount -= 1;
						continue;
					}
					
					cashReceiptSum += orderItem.Sum;
					unprocessedCodesCount -= 1;
					continue;
				}

				var orderItemCountWithoutLast = orderItem.Count - 1;
				var partDiscount = Math.Round(orderItem.DiscountMoney / orderItem.Count, 1);
				var lastPartDiscount = Math.Round(orderItem.DiscountMoney - (orderItemCountWithoutLast * partDiscount), 2);

				for(var i = 0; i < orderItem.Count; i++)
				{
					if(unprocessedCodesCount > maxCodesCount)
					{
						unprocessedCodesCount -= 1;
						continue;
					}
					if(unprocessedCodesCount == 0)
					{
						break;
					}

					var discount = i == orderItemCountWithoutLast ? lastPartDiscount : partDiscount;
					cashReceiptSum += orderItem.Price - discount;
					unprocessedCodesCount -= 1;
				}
			}

			return cashReceiptSum;
		}
		
		private async Task ProcessingReceiptBeforeSending(CashReceipt receipt, CancellationToken cancellationToken)
		{
			var needMoreReceipts = CheckNeedCreateMoreReceiptsForOrder(receipt.Order, out var countReceiptsNeeded);
			if(needMoreReceipts)
			{
				receipt.InnerNumber = 1;
				CreateReceipts(receipt.Order, countReceiptsNeeded);
			}

			foreach(var cashReceipt in _cashReceiptsToSave)
			{
				cashReceipt.Sum = cashReceipt.InnerNumber.HasValue ? GetOrderSum(cashReceipt) : cashReceipt.Order.OrderPositiveOriginalSum;
				await PrepareForReadyToSend(cashReceipt, cancellationToken);
			}
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

		private void PutValidSourceCodesToPoolAndRemoveFromReceipt(IEnumerable<CashReceiptProductCode> codes, CashReceipt receipt)
		{
			foreach(var code in codes)
			{
				if(!code.IsValid)
				{
					continue;
				}

				_codesPool.PutCode(code.SourceCode.Id);
				receipt.ScannedCodes.Remove(code);
			}
		}
		
		private void AddCodeToReceipt(CashReceipt receipt, OrderItem orderItem)
		{
			var newCode = new CashReceiptProductCode
			{
				CashReceipt = receipt,
				OrderItem = orderItem,
				IsUnscannedSourceCode = true,
				ResultCode = GetCodeFromPool(orderItem.Nomenclature.Gtin)
			};

			receipt.ScannedCodes.Add(newCode);
		}

		public void Dispose()
		{
			_codesPool.Rollback();
			_uow?.Dispose();
			_disposed = true;
		}
	}
}
