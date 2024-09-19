using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Domain.Client;

namespace Edo.Scheduler.Service
{
	public class TaskScheduler
	{
		private readonly ILogger<TaskScheduler> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<OrderEntity> _orderRepository;

		public TaskScheduler(
			ILogger<TaskScheduler> logger,
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<OrderEntity> orderRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}

		public void CreateTask(int orderId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var order = _orderRepository.Get(uow, x => x.Id == orderId).SingleOrDefault();
				if(order == null)
				{
					_logger.LogError("Невозможно создать задачу ЭДО. Заказ {OrderId} не найден.", orderId);
					return;
				}

				EdoTask edoTask;

				switch(order.PaymentType)
				{
					case PaymentType.Cashless:
						edoTask = CreateEdoTask(order);
						break;
					case PaymentType.Cash:
					case PaymentType.Terminal:
					case PaymentType.DriverApplicationQR:
					case PaymentType.SmsQR:
						edoTask = CreateReceiptTask(order);
						break;
					case PaymentType.PaidOnline:
						edoTask = CreatePaidOnlineTask(order);
						break;
					case PaymentType.Barter:
					case PaymentType.ContractDocumentation:
					default:
						edoTask = CreateSaveCodeTask(order);
						break;
				}
				uow.Save(edoTask);
				uow.Commit();
			}
		}

		private EdoTask CreatePaidOnlineTask(OrderEntity order)
		{
			if(order.PaymentByCardFrom.ReceiptRequired)
			{
				return CreateReceiptTask(order);
			}
			else
			{
				return CreateSaveCodeTask(order);
			}
		}

		private EdoTask CreateEdoTask(OrderEntity order)
		{
			return new OrderEdoTask
			{
				OrderId = order.Id,

				//Уточнить дополнительные поля
				//OrganizationId = order.Organization.Id,
				//CounterpartyId = order.Client.Id,

				Status = EdoTaskStatus.New
			};
		}

		private EdoTask CreateReceiptTask(OrderEntity order)
		{
			return new ReceiptEdoTask
			{
				OrderId = order.Id,
				Status = EdoTaskStatus.New
			};
		}

		private EdoTask CreateSaveCodeTask(OrderEntity order)
		{
			return new SaveCodesEdoTask
			{
				OrderId = order.Id,
				Status = EdoTaskStatus.New
			};
		}
	}
}
