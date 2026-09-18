using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Application.Orders.Services.OrderCancellation;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Orders;

namespace Vodovoz.ViewModels.Services.Orders
{
	/// <summary>
	/// Сервис получения разрешения на отмену заказа
	/// Обеспечивает проверки, которые должны выполняться перед открытием окна недовоза
	/// </summary>
	public class OrderCancellationPermitService
	{
		private readonly OrderCancellationService _orderCancellationService;
		private readonly IOrderRepository _orderRepository;
		private readonly ICashRepository _cashRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly ICommonServices _commonServices;
		private readonly IOrderSettings _orderSettings;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;

		public OrderCancellationPermitService(
			OrderCancellationService orderCancellationService,
			IOrderRepository orderRepository,
			ICashRepository cashRepository,
			IInteractiveService interactiveService,
			ICommonServices commonServices,
			IOrderSettings orderSettings,
			IDeliveryRulesSettings deliveryRulesSettings)
		{
			_orderCancellationService = orderCancellationService ?? throw new ArgumentNullException(nameof(orderCancellationService));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
		}

		/// <summary>
		/// Проверяет возможность отмены заказа по маркированной продукции и документообороту
		/// Если для отмены заказа сначала нужно аннулировать документооборот с клиентом,
		/// запускает аннулирование
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="order">Отменяемый заказ</param>
		/// <returns>Разрешение на отмену заказа</returns>
		public OrderCancellationPermit GetPermit(IUnitOfWork uow, Order order)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			var permit = _orderCancellationService.CanCancelOrder(uow, order);

			if(permit.Type == OrderCancellationPermitType.AllowCancelDocflow)
			{
				if(permit.EdoTaskToCancellationId == null)
				{
					throw new InvalidOperationException("Для аннулирования документооборота должен быть указан идентификатор ЭДО задачи.");
				}

				_orderCancellationService.CancelDocflowByUser(
					$"Отмена заказа №{order.Id}",
					permit.EdoTaskToCancellationId.Value
				);
			}

			return permit;
		}

		/// <summary>
		/// То же, что и <see cref="GetPermit"/>, но дополнительно проверяет сам заказ:
		/// запрещает отменять отгруженный или оплаченный самовывоз и валидирует заказ
		/// на перевод в статус "Отменён"
		/// Используется там, где отмену запускает пользователь по обращению клиента.
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="order">Отменяемый заказ</param>
		/// <returns>Разрешение на отмену заказа. Окно недовоза открывается</returns>
		public OrderCancellationPermit GetPermitWithOrderChecks(IUnitOfWork uow, Order order)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			if(IsShippedOrPaidSelfDelivery(uow, order))
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Вы не можете отменить отгруженный или оплаченный самовывоз. " +
						"Для продолжения необходимо удалить отгрузку."
				);

				return CreateDenyPermit();
			}

			if(!ValidateOrderCancellation(order))
			{
				return CreateDenyPermit();
			}

			return GetPermit(uow, order);
		}

		private bool IsShippedOrPaidSelfDelivery(IUnitOfWork uow, Order order)
		{
			if(!order.SelfDelivery)
			{
				return false;
			}

			var isShipped = !_orderRepository.IsSelfDeliveryOrderWithoutShipment(uow, order.Id);
			var orderHasIncome = _cashRepository.OrderHasIncome(uow, order.Id);

			return isShipped || orderHasIncome;
		}

		private bool ValidateOrderCancellation(Order order)
		{
			var validationContext = new ValidationContext(
				order,
				null,
				new Dictionary<object, object>
				{
					{ "NewStatus", OrderStatus.Canceled }
				}
			);

			validationContext.ServiceContainer.AddService(_orderSettings);
			validationContext.ServiceContainer.AddService(_deliveryRulesSettings);

			return _commonServices.ValidationService.Validate(order, validationContext);
		}

		private OrderCancellationPermit CreateDenyPermit() =>
			new OrderCancellationPermit { Type = OrderCancellationPermitType.Deny };
	}
}
