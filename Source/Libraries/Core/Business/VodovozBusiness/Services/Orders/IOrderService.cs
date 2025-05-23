using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Results;

namespace VodovozBusiness.Services.Orders
{
	public interface IOrderService
	{
		int PaidDeliveryNomenclatureId { get; }

		void CheckAndAddBottlesToReferrerByReferFriendPromo(IUnitOfWork uow, Order order, bool canChangeDiscountValue);
		Result<int, Exception> CreateAndAcceptOrder(CreateOrderRequest roboatsOrderArgs);
		Result<(int OrderId, int AuthorId, OrderStatus OrderStatus), Exception> CreateIncompleteOrder(CreateOrderRequest roboatsOrderArgs);

		/// <summary>
		/// Рассчитывает и возвращает цену заказа по имеющимся данным о заказе
		/// </summary>
		Result<decimal, Exception> GetOrderPrice(CreateOrderRequest roboatsOrderArgs);
		Result<Order, Exception> UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order);
		int TryCreateOrderFromOnlineOrderAndAccept(IUnitOfWork uow, OnlineOrder onlineOrder);
		(int OrderId, int AuthorId, OrderStatus OrderStatus) AcceptOrder(int orderId, int roboatsEmployeeId);
		bool NeedResendByEdo(IUnitOfWork unitOfWork, Order entity);
		void AutoCancelAutoTransfer(IUnitOfWork uow, Order order);

		/// <summary>
		/// Рассчитывает и возвращает цену заказа и цену доставки по имеющимся данным о заказе
		/// </summary>
		Result<(decimal OrderPrice, decimal DeliveryPrice, decimal ForfeitPrice), Exception> GetOrderAndDeliveryPrices(CreateOrderRequest createOrderRequest);
		
		/// <summary>
		/// Получение логистических требований для заказа
		/// </summary>
		LogisticsRequirements GetLogisticsRequirements(Order order);

		/// <summary>
		/// Создает и подтверждает заказ
		/// Возвращает результат с номером сохраненного заказа
		/// </summary>
		/// <param name="createOrderRequest"></param>
		Task<Result<int, Exception>> CreateAndAcceptOrderAsync(CreateOrderRequest createOrderRequest);

		/// <summary>
		/// Создает заказ с имеющимися данными в статусе Новый, для обработки его оператором вручную.
		/// Возвращает данные по заказу
		/// </summary>
		Task<Result<(int OrderId, int AuthorId, OrderStatus OrderStatus), Exception>> CreateIncompleteOrderAsync(CreateOrderRequest createOrderRequest);
		void AddLogisticsRequirements(Order order);
	}
}
