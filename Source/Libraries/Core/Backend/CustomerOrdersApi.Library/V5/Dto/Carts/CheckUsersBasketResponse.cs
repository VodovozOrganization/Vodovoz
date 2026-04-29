using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.V5.Dto.Orders;
using CustomerOrdersApi.Library.V5.Dto.Orders.OrderItem;
using CustomerOrdersApi.Library.V5.Dto.Orders.PromoSets;
using Vodovoz.Core.Data.InfoMessages;

namespace CustomerOrdersApi.Library.V5.Dto.Carts
{
	/// <summary>
	/// Ответ по проверке корзины
	/// </summary>
	public class CheckUsersBasketResponse
	{
		public CheckUsersBasketResponse() { }

		private CheckUsersBasketResponse(
			OnlineOrderSumDto orderSum,
			NextStepCheckUsersBasket nextStep,
			IEnumerable<CheckedOnlineOrderItemDto> onlineOrderItems,
			IEnumerable<CheckedPromoSetDto> promoSets,
			IEnumerable<CheckedOnlineRentPackageDto> rentPackages,
			IEnumerable<InfoMessage> infoMessages,
			IEnumerable<WarningMessage> warnings)
		{
			OrderSum = orderSum;
			NextStep = nextStep;
			OnlineOrderItems = onlineOrderItems;
			PromoSets = promoSets;
			OnlineRentPackages = rentPackages;
			InfoMessages = infoMessages;
			Warnings = warnings;
		}
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
			OnlineOrderSumDto orderSum,
			NextStepCheckUsersBasket nextStep,
			IEnumerable<CheckedOnlineOrderItemDto> onlineOrderItems,
			IEnumerable<CheckedPromoSetDto> promoSets,
			IEnumerable<CheckedOnlineRentPackageDto> rentPackages,
			IEnumerable<InfoMessage> infoMessages,
			IEnumerable<WarningMessage> warnings
		) => new CheckUsersBasketResponse(orderSum, nextStep, onlineOrderItems, promoSets, rentPackages, infoMessages, warnings);
	}
}
