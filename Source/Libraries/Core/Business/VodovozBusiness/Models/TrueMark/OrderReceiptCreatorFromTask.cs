using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Factories;
using Vodovoz.Models.TrueMark;

namespace VodovozBusiness.Models.TrueMark
{
	public abstract class OrderReceiptCreatorFromTask : IDisposable
	{
		protected IList<CashReceipt> CashReceiptsToSave;

		protected OrderReceiptCreatorFromTask(
			ILogger<OrderReceiptCreatorFromTask> logger,
			IUnitOfWorkFactory uowFactory,
			TrueMarkTransactionalCodesPool codesPool,
			ICashReceiptFactory cashReceiptFactory)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			UowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			CashReceiptFactory = cashReceiptFactory ?? throw new ArgumentNullException(nameof(cashReceiptFactory));
			CodesPool = codesPool ?? throw new ArgumentNullException(nameof(codesPool));
		}
		
		protected ILogger<OrderReceiptCreatorFromTask> Logger { get; }
		protected IUnitOfWorkFactory UowFactory { get; }
		protected TrueMarkTransactionalCodesPool CodesPool { get; }
		protected ICashReceiptFactory CashReceiptFactory { get; }

		public virtual async Task TryCreateCashReceiptsAsync(IEnumerable<int> ordersIds, CancellationToken cancellationToken)
		{
			Order order = null;
			BulkAccountingEdoTask task = null;
			
			foreach(var orderId in ordersIds)
			{
				try
				{
					using(var uow = UowFactory.CreateWithoutRoot($"Создание чека(ов) по заказу {orderId}"))
					{
						task = uow.GetAll<BulkAccountingEdoTask>()
							.FirstOrDefault(x => x.FormalEdoRequest.Order.Id == orderId
								&& x.Status == EdoTaskStatus.New);

						if(task is null)
						{
							Logger.LogWarning("Не найдена таска для создания чека по заказу {OrderId}", orderId);
							continue;
						}

						order = GetOrder(uow, orderId);
						var countMarkedNomenclaturesWithPositiveSum =
							(int)order.OrderItems
								.Where(x => x.Nomenclature.IsAccountableInTrueMark && x.Sum > 0)
								.Sum(x => x.Count);

						if(countMarkedNomenclaturesWithPositiveSum > CashReceipt.MaxMarkCodesInReceipt)
						{
							CreateCashReceipts(uow, order, task, countMarkedNomenclaturesWithPositiveSum);
						}
						else
						{
							CreateCashReceipt(uow, order, task);
						}

						await uow.CommitAsync(cancellationToken);
						CommitPool();
					}
				}
				catch(Exception ex)
				{
					RollbackPool();
					RegisterException(order, task?.Id, ex);
					Logger.LogError(ex, "Ошибка создания чека для заказа {OrderId}.", orderId);
				}
			}
			
			RollbackPool();
		}

		private void RegisterException(Order order, int? taskId, Exception ex)
		{
			Logger.LogError(ex, $"Ошибка обработки заказа честного знака для заказа {order.Id}.");
			using(var uow = UowFactory.CreateWithoutRoot())
			{
				var cashReceipt = CashReceiptFactory.CreateNewCashReceipt(order, taskId);
				cashReceipt.Status = CashReceiptStatus.CodeError;
				cashReceipt.ErrorDescription = ex.Message;
				uow.Save(cashReceipt);
				uow.Commit();
			}
		}

		protected virtual void CreateCashReceipts(
			IUnitOfWork uow,
			Order order,
			BulkAccountingEdoTask task,
			int unprocessedCodes)
		{
			var receiptNumber = 1;
			var codesInReceipt = 0;
			CashReceiptsToSave = new List<CashReceipt>();
			var taskCodes = task.Items.Select(x => x.ProductCode).ToList();
			var receipt = CreateCashReceiptForBigOrder(order, task.Id, receiptNumber);
			var positiveSumItems = order.OrderItems.Where(x => x.Sum > 0);

			foreach(var orderItem in positiveSumItems)
			{
				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					continue;
				}

				for(var i = 0; i < orderItem.Count; i++)
				{
					CreateCashReceiptProductCode(uow, receipt, taskCodes, orderItem);
					codesInReceipt++;
					unprocessedCodes--;
					
					if(codesInReceipt < CashReceipt.MaxMarkCodesInReceipt)
					{
						continue;
					}

					if(unprocessedCodes != 0)
					{
						receiptNumber++;
						receipt = CreateCashReceiptForBigOrder(order, task.Id, receiptNumber);
						codesInReceipt = 0;
					}
				}
			}

			SaveReceipts(uow);
		}

		protected virtual void CreateCashReceipt(IUnitOfWork uow, Order order, BulkAccountingEdoTask task)
		{
			var receipt = CashReceiptFactory.CreateNewCashReceipt(order, task.Id);
			var positiveSumItems = order.OrderItems.Where(x => x.Sum > 0);
			var taskCodes = task.Items.Select(x => x.ProductCode).ToList();
			
			foreach(var orderItem in positiveSumItems)
			{
				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					continue;
				}

				for(var i = 1; i <= orderItem.Count; i++)
				{
					CreateCashReceiptProductCode(uow, receipt, taskCodes, orderItem);
				}
			}

			uow.Save(receipt);
		}

		protected void CreateCashReceiptProductCode(
			IUnitOfWork uow,
			CashReceipt receipt,
			ICollection<TrueMarkProductCode> taskCodes,
			OrderItem orderItem)
		{
			var orderProductCode = new CashReceiptProductCode
			{
				CashReceipt = receipt,
				OrderItem = orderItem,
			};

			TrueMarkWaterIdentificationCode code = null;
			
			var taskCode = taskCodes
				.FirstOrDefault(x => x.SourceCode != null
					&& !string.IsNullOrWhiteSpace(x.SourceCode.CheckCode)
					&& orderItem.Nomenclature.Gtins.Any(y => y.GtinNumber == x.SourceCode.Gtin));
			
			if(taskCode != null)
			{
				code = taskCode.SourceCode;
				taskCodes.Remove(taskCode);
				
				orderProductCode.DuplicatesCount = taskCode.DuplicatesCount;
				orderProductCode.IsDuplicateSourceCode = taskCode.Problem == ProductCodeProblem.Duplicate;
				orderProductCode.IsDefectiveSourceCode = taskCode.Problem == ProductCodeProblem.Defect;
			}
			else
			{
				code = GetCodeFromPool(uow, orderItem.Nomenclature.Gtin);
			}
			
			orderProductCode.SourceCode = code;
			receipt.ScannedCodes.Add(orderProductCode);
		}

		protected TrueMarkWaterIdentificationCode GetCodeFromPool(IUnitOfWork uow, string gtin)
		{
			var codeId = CodesPool.TakeCode(gtin);
			return uow.GetById<TrueMarkWaterIdentificationCode>(codeId);
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
		
		protected Order GetOrder(IUnitOfWork uow, int orderId) => uow.GetById<Order>(orderId);

		protected CashReceipt CreateCashReceiptForBigOrder(Order order, int? taskId, int receiptNumber)
		{
			var receipt = CashReceiptFactory.CreateNewCashReceipt(order, taskId);
			receipt.InnerNumber = receiptNumber;
			CashReceiptsToSave.Add(receipt);
			return receipt;
		}

		protected void SaveReceipts(IUnitOfWork uow)
		{
			foreach(var receipt in CashReceiptsToSave)
			{
				uow.Save(receipt);
			}
		}

		public void Dispose()
		{
			RollbackPool();
		}
	}
}
