using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.InfoMessages;
using CustomerOrders.Contracts.V5.Orders;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using CustomerOrders.Contracts.V5.Orders.PromoSets;

namespace CustomerOrders.Contracts.V5.Carts
{
	/// <summary>
	/// Ответ по проверке корзины
	/// </summary>
	public class CheckUsersBasketResponse
	{
		public CheckUsersBasketResponse() { }

		private CheckUsersBasketResponse(
			Guid checkId,
			OnlineOrderSumDto orderSum,
			NextStepCheckUsersBasket nextStep,
			IEnumerable<CheckedOnlineOrderItemDto> onlineOrderItems,
			IEnumerable<CheckedPromoSetDto> promoSets,
			IEnumerable<CheckedOnlineRentPackageDto> rentPackages,
			IEnumerable<InfoMessage> infoMessages,
			IEnumerable<WarningMessage> warnings)
		{
			CheckId = checkId;
			OrderSum = orderSum;
			NextStep = nextStep;
			OnlineOrderItems = onlineOrderItems;
			PromoSets = promoSets;
			OnlineRentPackages = rentPackages;
			InfoMessages = infoMessages;
			Warnings = warnings;
		}
		
		/// <summary>
		/// Идентификатор проверки
		/// </summary>
		public Guid CheckId { get; }
		/// <summary>
		/// Данные по сумме заказа
		/// </summary>
		public OnlineOrderSumDto OrderSum { get; }
		/// <summary>
		/// Следующий шаг
		/// </summary>
		public NextStepCheckUsersBasket NextStep { get; }
		/// <summary>
		/// Информация по товарам
		/// </summary>
		public IEnumerable<CheckedOnlineOrderItemDto> OnlineOrderItems { get; }
		/// <summary>
		/// Информация по промонаборам
		/// </summary>
		public IEnumerable<OrderingPromoSetDto> PromoSets { get; }
		/// <summary>
		/// Информация по пакетам аренды
		/// </summary>
		public IEnumerable<OnlineRentPackageDto> OnlineRentPackages { get; }
		/// <summary>
		/// Информационное сообщение
		/// </summary>
		public IEnumerable<InfoMessage> InfoMessages { get; }
		/// <summary>
		/// Предупреждающая информация
		/// </summary>
		public IEnumerable<WarningMessage> Warnings { get; }

		public static CheckUsersBasketResponse Create(
			Guid checkId,
			OnlineOrderSumDto orderSum,
			NextStepCheckUsersBasket nextStep,
			IEnumerable<CheckedOnlineOrderItemDto> onlineOrderItems,
			IEnumerable<CheckedPromoSetDto> promoSets,
			IEnumerable<CheckedOnlineRentPackageDto> rentPackages,
			IEnumerable<InfoMessage> infoMessages,
			IEnumerable<WarningMessage> warnings
		) => new CheckUsersBasketResponse(checkId, orderSum, nextStep, onlineOrderItems, promoSets, rentPackages, infoMessages, warnings);
	}
}
