using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Models.TrueMark
{

	public class SelfdeliveryReceiptCreator : IDisposable
	{
		private readonly ILogger<SelfdeliveryReceiptCreator> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private readonly TrueMarkTransactionalCodesPool _codesPool;
		private readonly int _orderId;
		private readonly IUnitOfWork _uow;
		private bool _disposed;


		public SelfdeliveryReceiptCreator(
			ILogger<SelfdeliveryReceiptCreator> logger,
			IUnitOfWorkFactory uowFactory,
			ICashReceiptRepository cashReceiptRepository, 
			TrueMarkTransactionalCodesPool codesPool,
			int orderId)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			_codesPool = codesPool ?? throw new ArgumentNullException(nameof(codesPool));
			if(orderId <= 0)
			{
				throw new ArgumentException("Должен быть указан существующий Id заказа.", nameof(orderId));
			}

			_orderId = orderId;
			_uow = _uowFactory.CreateWithoutRoot();
		}

		public async Task CreateReceiptAsync(CancellationToken cancellationToken)
		{
			if(_disposed)
			{
				throw new ObjectDisposedException(nameof(SelfdeliveryReceiptCreator));
			}

			await Task.Run(() => TryCreateCashReceipt(), cancellationToken);
		}

		private void TryCreateCashReceipt()
		{
			try
			{
				CreateCashReceipt();
				_uow.Commit();
				CommitPool();
			}
			catch(Exception ex)
			{
				RollbackPool();
				_logger.LogError(ex, $"Ошибка создания чека для заказа самовывоза {_orderId}.");
			}
		}

		private void CreateCashReceipt()
		{
			var order = _uow.GetById<Order>(_orderId);

			var receipt = new CashReceipt
			{
				Order = order,
				CreateDate = DateTime.Now,
				Status = CashReceiptStatus.New
			};

			_uow.Save(receipt);

			foreach(var orderItem in order.OrderItems)
			{
				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					continue;
				}

				for(int i = 1; i <= orderItem.Count; i++)
				{
					CreateTrueMarkCodeEntity(receipt, orderItem);
				}
			}

			_uow.Save(receipt);
		}

		private void CreateTrueMarkCodeEntity(CashReceipt receipt, OrderItem orderItem)
		{
			var orderProductCode = new CashReceiptProductCode
			{
				CashReceipt = receipt,
				OrderItem = orderItem,
				SourceCode = GetCodeFromPool()
			};

			receipt.ScannedCodes.Add(orderProductCode);
			_uow.Save(orderProductCode);
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

		public void Dispose()
		{
			RollbackPool();
			_uow.Dispose();
			_disposed = true;
		}
	}
}
