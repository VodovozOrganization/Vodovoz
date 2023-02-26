using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkSelfDeliveriesHandler
	{
		private readonly ILogger<TrueMarkSelfDeliveriesHandler> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly TrueMarkTransactionalCodesPool _codePool;

		public TrueMarkSelfDeliveriesHandler(
			ILogger<TrueMarkSelfDeliveriesHandler> logger,
			IUnitOfWorkFactory uowFactory, 
			IOrderRepository orderRepository, 
			IOrderParametersProvider orderParametersProvider, 
			TrueMarkTransactionalCodesPool codePool)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			_codePool = codePool ?? throw new ArgumentNullException(nameof(codePool));
		}

		public void HandleOrders()
		{
			var selfDeliveryOrderIds = GetSelfDeliveryIds();
			foreach(var selfDeliveryOrderId in selfDeliveryOrderIds)
			{
				try
				{
					ProcessOrder(selfDeliveryOrderId);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, $"Ошибка обработки заказа {selfDeliveryOrderId}.");
				}
			}
		}

		private IEnumerable<int> GetSelfDeliveryIds()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var orderIds = _orderRepository.GetSelfdeliveryOrderIdsForCashReceipt(uow, _orderParametersProvider);
				return orderIds;
			}
		}

		private void ProcessOrder(int selfDeliveryOrderrId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var selfDeliveryOrder = uow.GetById<Order>(selfDeliveryOrderrId);
				CreateTrueMarkOrder(uow, selfDeliveryOrder);

				try
				{
					uow.Save(selfDeliveryOrder);
					uow.Commit();
				}
				catch(Exception)
				{
					_codePool.Rollback();
					throw;
				}

				//не мешаем сохранению сущности, ошибка пула кода не важна если сущности сохранилась
				try
				{
					_codePool.Commit();
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка коммита пула кодов.");
				}
			}
		}

		private void CreateTrueMarkOrder(IUnitOfWork uow, Order order)
		{
			var trueMarkCashReceiptOrder = new TrueMarkCashReceiptOrder();
			trueMarkCashReceiptOrder.Order = order;
			trueMarkCashReceiptOrder.Date = DateTime.Now;
			trueMarkCashReceiptOrder.Status = TrueMarkCashReceiptOrderStatus.New;
			uow.Save(trueMarkCashReceiptOrder);

			foreach(var orderItem in order.OrderItems)
			{
				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					continue;
				}

				for(int i = 1; i <= orderItem.Count; i++)
				{
					CreateTrueMarkCodeEntity(uow, trueMarkCashReceiptOrder, orderItem);
				}
			}

			uow.Save(trueMarkCashReceiptOrder);
		}

		private void CreateTrueMarkCodeEntity(IUnitOfWork uow, TrueMarkCashReceiptOrder trueMarkCashReceiptOrder, OrderItem orderItem)
		{
			var orderProductCode = new TrueMarkCashReceiptProductCode();
			orderProductCode.TrueMarkCashReceiptOrder = trueMarkCashReceiptOrder;
			trueMarkCashReceiptOrder.ScannedCodes.Add(orderProductCode);
			orderProductCode.OrderItem = orderItem;
			orderProductCode.SourceCode = GetCodeFromPool(uow);

			uow.Save(orderProductCode);
		}

		private TrueMarkWaterIdentificationCode GetCodeFromPool(IUnitOfWork uow)
		{
			var codeId = _codePool.TakeCode();
			return uow.GetById<TrueMarkWaterIdentificationCode>(codeId);
		}
	}
}
