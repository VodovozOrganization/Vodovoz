using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Factories;

namespace Vodovoz.Models.TrueMark
{
	public abstract class OrderReceiptCreator : IDisposable
	{
		protected readonly ICashReceiptRepository CashReceiptRepository;
		protected IList<CashReceipt> CashReceiptsToSave;
		protected bool Disposed;

		protected OrderReceiptCreator(
			ILogger<OrderReceiptCreator> logger,
			IUnitOfWorkFactory uowFactory,
			ICashReceiptRepository cashReceiptRepository, 
			TrueMarkTransactionalCodesPool codesPool,
			ICashReceiptFactory cashReceiptFactory,
			int orderId)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			UowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			CashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			CashReceiptFactory = cashReceiptFactory ?? throw new ArgumentNullException(nameof(cashReceiptFactory));
			CodesPool = codesPool ?? throw new ArgumentNullException(nameof(codesPool));
			if(orderId <= 0)
			{
				throw new ArgumentException("Должен быть указан существующий Id заказа.", nameof(orderId));
			}

			OrderId = orderId;
			Uow = UowFactory.CreateWithoutRoot();
		}
		
		protected ILogger<OrderReceiptCreator> Logger { get; }
		protected IUnitOfWorkFactory UowFactory { get; }
		protected TrueMarkTransactionalCodesPool CodesPool { get; }
		protected ICashReceiptFactory CashReceiptFactory { get; }
		protected int OrderId { get; }
		protected IUnitOfWork Uow { get; }

		public async Task CreateReceiptAsync(CancellationToken cancellationToken)
		{
			if(Disposed)
			{
				throw new ObjectDisposedException(nameof(OrderReceiptCreator));
			}

			await Task.Run(TryCreateCashReceipt, cancellationToken);
		}
		
		protected virtual void TryCreateCashReceipt()
		{
			Order order = null;
			try
			{
				order = GetOrder();
				var countMarkedNomenclaturesWithPositiveSum = 
					(int)order.OrderItems
						.Where(x => x.Nomenclature.IsAccountableInTrueMark && x.Sum > 0)
						.Sum(x => x.Count);

				if(countMarkedNomenclaturesWithPositiveSum > CashReceipt.MaxMarkCodesInReceipt)
				{
					CreateCashReceipts(order, countMarkedNomenclaturesWithPositiveSum);
				}
				else
				{
					CreateCashReceipt(order);
				}
				
				Uow.Commit();
				CommitPool();
			}
			catch(Exception ex)
			{
				RollbackPool();
				RegisterException(order, ex);
				Logger.LogError(ex, "Ошибка создания чека для заказа самовывоза {OrderId}.", OrderId);
			}
		}

		private void RegisterException(Order order, Exception ex)
		{
			Logger.LogError(ex, $"Ошибка обработки заказа честного знака для заказа {order.Id}.");
			using(var uow = UowFactory.CreateWithoutRoot())
			{
				var cashReceipt = CashReceiptFactory.CreateNewCashReceipt(order);
				cashReceipt.Status = CashReceiptStatus.CodeError;
				cashReceipt.ErrorDescription = ex.Message;
				uow.Save(cashReceipt);
				uow.Commit();
			}
		}

		protected virtual void CreateCashReceipts(Order order, int unprocessedCodes)
		{
			var receiptNumber = 1;
			var codesInReceipt = default(int);
			CashReceiptsToSave = new List<CashReceipt>();
			var receipt = CreateCashReceiptForBigOrder(order, receiptNumber);
			var positiveSumItems = order.OrderItems.Where(x => x.Sum > 0);

			foreach(var orderItem in positiveSumItems)
			{
				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					continue;
				}

				for(var i = 0; i < orderItem.Count; i++)
				{
					CreateCashReceiptProductCode(receipt, orderItem);
					codesInReceipt++;
					unprocessedCodes--;
					
					if(codesInReceipt < CashReceipt.MaxMarkCodesInReceipt)
					{
						continue;
					}

					if(unprocessedCodes != 0)
					{
						receiptNumber++;
						receipt = CreateCashReceiptForBigOrder(order, receiptNumber);
						codesInReceipt = default(int);
					}
				}
			}

			SaveReceipts();
		}

		protected virtual void CreateCashReceipt(Order order)
		{
			var receipt = CashReceiptFactory.CreateNewCashReceipt(order);
			var positiveSumItems = order.OrderItems.Where(x => x.Sum > 0);
			
			foreach(var orderItem in positiveSumItems)
			{
				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					continue;
				}

				for(int i = 1; i <= orderItem.Count; i++)
				{
					CreateCashReceiptProductCode(receipt, orderItem);
				}
			}

			Uow.Save(receipt);
		}

		protected void CreateCashReceiptProductCode(CashReceipt receipt, OrderItem orderItem)
		{
			var orderProductCode = new CashReceiptProductCode
			{
				CashReceipt = receipt,
				OrderItem = orderItem,
				SourceCode = GetCodeFromPool(orderItem.Nomenclature.Gtin)
			};

			receipt.ScannedCodes.Add(orderProductCode);
		}

		protected TrueMarkWaterIdentificationCode GetCodeFromPool(string gtin)
		{
			var codeId = CodesPool.TakeCode(gtin);
			return Uow.GetById<TrueMarkWaterIdentificationCode>(codeId);
		}
		
		protected void CommitPool()
		{
			//ошибка пула кода не важна для основной работы
			try
			{
				CodesPool.Commit();
			}
			catch(Exception ex)
			{
				Logger.LogError(ex, "Ошибка коммита пула кодов.");
			}
		}

		protected void RollbackPool()
		{
			//ошибка пула кода не важна для основной работы
			try
			{
				CodesPool.Rollback();
			}
			catch(Exception ex)
			{
				Logger.LogError(ex, "Ошибка отката пула кодов.");
			}
		}
		
		protected Order GetOrder() => Uow.GetById<Order>(OrderId);

		protected CashReceipt CreateCashReceiptForBigOrder(Order order, int receiptNumber)
		{
			var receipt = CashReceiptFactory.CreateNewCashReceipt(order);
			receipt.InnerNumber = receiptNumber;
			CashReceiptsToSave.Add(receipt);
			return receipt;
		}

		protected void SaveReceipts()
		{
			foreach(var receipt in CashReceiptsToSave)
			{
				Uow.Save(receipt);
			}
		}

		public void Dispose()
		{
			RollbackPool();
			Uow.Dispose();
			Disposed = true;
		}
	}
}
