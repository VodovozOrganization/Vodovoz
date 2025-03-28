using QS.DomainModel.UoW;
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
		int TryCreateOrderFromOnlineOrderAndAccept(IUnitOfWork uow, OnlineOrder onlineOrder);
		(int OrderId, int AuthorId, OrderStatus OrderStatus) AcceptOrder(int orderId, int roboatsEmployeeId);
		bool NeedResendByEdo(IUnitOfWork unitOfWork, Order entity);
		void AutoCancelAutoTransfer(IUnitOfWork uow, Order order);

		/// <summary>
		/// Рассчитывает и возвращает цену заказа и цену доставки по имеющимся данным о заказе
		/// </summary>
		(decimal OrderPrice, decimal DeliveryPrice) GetOrderAndDeliveryPrices(CreateOrderRequest createOrderRequest);
		
		/// <summary>
		/// Получение логистических требований для заказа
		/// </summary>
		LogisticsRequirements GetLogisticsRequirements(Order order);
	}
}
