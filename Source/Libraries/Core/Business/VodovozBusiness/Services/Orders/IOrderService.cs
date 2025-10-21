using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOrderService
	{
		int PaidDeliveryNomenclatureId { get; }

		void CheckAndAddBottlesToReferrerByReferFriendPromo(IUnitOfWork uow, Order order, bool canChangeDiscountValue);
		int CreateAndAcceptOrder(CreateOrderRequest roboatsOrderArgs);
		(int OrderId, int AuthorId, OrderStatus OrderStatus) CreateIncompleteOrder(CreateOrderRequest roboatsOrderArgs);

		/// <summary>
		/// Рассчитывает и возвращает цену заказа по имеющимся данным о заказе
		/// </summary>
		decimal GetOrderPrice(CreateOrderRequest roboatsOrderArgs);
		void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order);
		(int OrderId, int AuthorId, OrderStatus OrderStatus) AcceptOrder(int orderId, int roboatsEmployeeId);
		bool NeedResendByEdo(IUnitOfWork unitOfWork, Order entity);
		void AutoCancelAutoTransfer(IUnitOfWork uow, Order order);

		/// <summary>
		/// Рассчитывает и возвращает цену заказа и цену доставки по имеющимся данным о заказе
		/// </summary>
		[Obsolete("Метод использовался в RobotMiaApi, должен быть удален")]
		(decimal OrderPrice, decimal DeliveryPrice, decimal ForfeitPrice) GetOrderAndDeliveryPrices(CreateOrderRequest createOrderRequest);
		
		/// <summary>
		/// Получение логистических требований для заказа
		/// </summary>
		LogisticsRequirements GetLogisticsRequirements(Order order);

		/// <summary>
		/// Создает и подтверждает заказ
		/// Возвращает результат с номером сохраненного заказа
		/// </summary>
		/// <param name="createOrderRequest"></param>
		[Obsolete("Метод использовался в RobotMiaApi, должен быть удален")]
		Task<Result<int>> CreateAndAcceptOrderAsync(CreateOrderRequest createOrderRequest);

		/// <summary>
		/// Создает заказ с имеющимися данными в статусе Новый, для обработки его оператором вручную.
		/// Возвращает данные по заказу
		/// </summary>
		[Obsolete("Метод использовался в RobotMiaApi, должен быть удален")]
		Task<Result<(int OrderId, int AuthorId, OrderStatus OrderStatus)>> CreateIncompleteOrderAsync(CreateOrderRequest createOrderRequest);
		void AddLogisticsRequirements(Order order);
		void UpdatePaymentStatus(IUnitOfWork uow, Order order);
		Task<int> TryCreateOrderFromOnlineOrderAndAcceptAsync(IUnitOfWork uow, OnlineOrder onlineOrder, CancellationToken cancellationToken);
	}
}
