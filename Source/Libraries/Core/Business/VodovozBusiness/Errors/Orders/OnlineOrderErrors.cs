using Vodovoz.Core.Domain.Results;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.Errors.Orders
{
	public static partial class OnlineOrderErrors
	{
		public static Error IsEmptyCounterparty =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsEmptyCounterparty),
				"Контрагент не заполнен");
		
		public static Error IsEmptySelfDeliveryGeoGroup =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsEmptySelfDeliveryGeoGroup),
				"Не указана гео группа для самовывоза");
		
		public static Error IsEmptyDeliveryPoint =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsEmptyDeliveryPoint),
				"Не указана точка доставки");
		
		public static Error DeliveryPointNotBelongCounterparty =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(DeliveryPointNotBelongCounterparty),
				$"Точка доставки не принадлежит клиенту");
		
		public static Error IncorrectDeliveryDate =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectDeliveryDate),
				"Дата доставки не может быть раньше сегодняшнего дня");
		
		public static Error IsEmptyDistrictFromDeliveryPoint =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsEmptyDistrictFromDeliveryPoint),
				"Не указан логистический район у точки доставки, невозможно расcчитать доставку");
		
		public static Error IsEmptyDeliverySchedule =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsEmptyDeliverySchedule),
				"Не заполнен график доставки");
		
		public static Error NeedPaidDelivery =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(NeedPaidDelivery),
				"В онлайн заказе отсутствует платная доставка");
		
		public static Error NotNeedPaidDelivery =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(NotNeedPaidDelivery),
				"В онлайн заказе не должно быть платной доставки");
		
		public static Error OrderMustBeNull =>
			new Error("400", "У онлайн заказа не должно быть реального заказа");
		
		public static Error IsEmptyOnlineOrder =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsEmptyOnlineOrder),
				"Не найден онлайн заказ");
		public static Error OnlineOrderNotFound =>
			new Error("400", "Онлайн заказ не найден");
		public static Error OnlineOrderIsPaidButOnlinePaymentIsEmpty =>
			new Error("400", "Онлайн заказ оплачен, но не заполнен номер оплаты");
		public static Error OnlineOrderCanceled =>
			new Error("400", "Онлайн заказ отменен");
		public static Error IsOnlineOrderNotWaitForPayment =>
			new Error("400", "Онлайн заказ не находится в ожидании оплаты");
		public static Error IsOrderAlreadyProcessingAndCannotChanged =>
			new Error("400", "Заказ уже в обработке и не может быть изменен");
		public static Error IsOnlineOrderPaid =>
			new Error("400", "Онлайн заказ уже оплачен");
		public static Error CantChangePaymentType =>
			new Error("400", "Нельзя менять тип оплаты");
		public static Error IsUnknownDeliverySchedule =>
			new Error("400", "Неизвестный график доставки");
		public static Error HasTimeToPayOrderExpired =>
			new Error("408", "Время на оплату заказа истекло. В ближайшее время с Вами свяжется менеджер для оформления заказа");
		public static Error IsOnlineOrderTimersEmpty =>
			new Error("500", "Не найдены таймеры для онлайн заказов");
		
		public static Error IncorrectPricePaidDelivery(decimal price, decimal onlineOrderItemPrice) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectPricePaidDelivery),
				$"Платная доставка с неверной ценой: пришла {onlineOrderItemPrice}, а должна быть {price}");
		
		public static Error FastDeliveryNotAvailable =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(FastDeliveryNotAvailable),
				"Быстрая доставка не доступна");
		
		public static Error IsArchivedOnlineOrderPromoSet(string promoSetTitle) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsArchivedOnlineOrderPromoSet),
				$"Промонабор {promoSetTitle} является архивным");
		
		public static Error IsIncorrectOnlineOrderPromoSetItemsCount(string promoSetTitle) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsIncorrectOnlineOrderPromoSetItemsCount),
				$"Переданный промонабор {promoSetTitle} содержит неверное количество товаров");
		
		public static Error IsIncorrectOnlineOrderPromoSetForNewClientsCount() =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsIncorrectOnlineOrderPromoSetForNewClientsCount),
				$"В онлайн заказе содержится больше одного промонабора для новых клиентов");
		
		public static Error IncorrectCountNomenclatureInOnlineOrderPromoSet(
			string promoSetTitle, int position, string nomenclature, int countFromPromoSet, int countFromOnlineOrderItem) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectCountNomenclatureInOnlineOrderPromoSet),
				$"В переданном промонаборе {promoSetTitle} в позиции {position} {nomenclature}" +
				$"\nнеправильно указано количество: должно быть {countFromPromoSet}, а передано {countFromOnlineOrderItem}");
		
		public static Error IsIncorrectNomenclatureInOnlineOrder(int? nomenclatureId) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsIncorrectNomenclatureInOnlineOrder),
				$"Номенклатура с Id {nomenclatureId} не найдена в базе");
		
		public static Error IsArchivedNomenclatureInOnlineOrder(string nomenclature) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsArchivedNomenclatureInOnlineOrder),
				$"Номенклатура {nomenclature} является архивной");
		
		public static Error IncorrectPriceNomenclatureInOnlineOrder(
			string nomenclature, decimal price, decimal onlineOrderItemPrice) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectPriceNomenclatureInOnlineOrder),
				$"Номенклатура {nomenclature} пришла с неправильно установленной ценой" +
				$"\nДолжно быть {price}, а передано {onlineOrderItemPrice}");
		
		public static Error NotApplicableDiscountToNomenclatureOnlineOrder(string nomenclature) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(NotApplicableDiscountToNomenclatureOnlineOrder),
				$"Номенклатура {nomenclature} пришла со скидкой, хотя у нее" +
				"\nне должно быть скидки на товар, кроме промо наборов");
		
		public static Error IncorrectDiscountNomenclatureInOnlineOrder(
			string nomenclature, decimal discount, decimal onlineOrderItemDiscount) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectDiscountNomenclatureInOnlineOrder),
				$"Номенклатура {nomenclature} пришла с неправильно установленной скидкой" +
				$"\nДолжно быть {discount}, а передано {onlineOrderItemDiscount}");
		
		public static Error IncorrectCountNomenclatureInOnlineOrder(string nomenclature, decimal count) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectDiscountNomenclatureInOnlineOrder),
				$"Номенклатура {nomenclature} пришла с неправильно установленным количеством" +
				$"\nДолжно быть больше 0, а передано {count}");
		
		public static Error IncorrectDiscountTypeInOnlineOrder(
			string nomenclature, bool needsValue, bool onlineOrderItemDiscountInMoney) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectDiscountTypeInOnlineOrder),
				$"У номенклатуры {nomenclature} неправильно указан тип скидки," +
				$"\nдолжно быть скидка в рублях: {needsValue.ConvertToYesOrNo()}," +
				$" а передано {onlineOrderItemDiscountInMoney.ConvertToYesOrNo()}");
			
		public static Error IncorrectDiscountTypeInOnlineOrderPromoSet(
			string promoSetTitle, int position, string nomenclature, bool discountInMoneyFromPromoSet, bool onlineOrderItemDiscountInMoney) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectDiscountInOnlineOrderPromoSet),
				$"В переданном промонаборе {promoSetTitle} в позиции {position} {nomenclature}" +
				$"\nнеправильно указан тип скидки, должно быть скидка в рублях: {discountInMoneyFromPromoSet.ConvertToYesOrNo()}," +
				$" а передано {onlineOrderItemDiscountInMoney.ConvertToYesOrNo()}");
		
		public static Error IncorrectDiscountInOnlineOrderPromoSet(
			string promoSetTitle, int position, string nomenclature, decimal discountItemFromPromoSet, decimal onlineOrderItemDiscount) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectDiscountInOnlineOrderPromoSet),
				$"В переданном промонаборе {promoSetTitle} в позиции {position} {nomenclature}" +
				$"\nнеправильно указана скидка: должно быть {discountItemFromPromoSet}, а передано {onlineOrderItemDiscount}");
		
		public static Error IncorrectRentPackageIdInOnlineOrder(int? rentPackageId) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectRentPackageIdInOnlineOrder),
				$"Переданный пакет аренды с Id {rentPackageId} не найден в базе");
		
		public static Error IsArchivedRentPackageIdInOnlineOrder(int? rentPackageId) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IsArchivedRentPackageIdInOnlineOrder),
				$"Переданный пакет аренды {rentPackageId} является архивным");
		
		public static Error IncorrectRentPackagePriceInOnlineOrder(
			int? rentPackageId, decimal onlineRentPackagePrice, decimal depositFromRentPackage) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectRentPackagePriceInOnlineOrder),
				$"В переданном пакете аренды {rentPackageId} не совпадает цена." +
				$"\nВ онлайн заказе {onlineRentPackagePrice} а в Пакете аренды {depositFromRentPackage}");
		
		public static Error InvalidPhone(string phone) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(InvalidPhone),
				$"Невалидный телефон {phone}");

		public static Error OnlineOrderContainsGoodsSoldFromSeveralOrganizations() =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(OnlineOrderContainsGoodsSoldFromSeveralOrganizations),
				"Данный заказ содержит товары, продаваемые от нескольких организаций");
		
		public static Error IncorrectOrderSum(decimal producedOrderSum, decimal currentOrderSum) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(IncorrectOrderSum),
				$"Некорректная сумма заказа! Пришло {producedOrderSum}, а должно быть {currentOrderSum}");
		
		public static Error ClientHasOrdersForThisDate(string orderIds) =>
			new Error(
				typeof(OnlineOrderErrors),
				nameof(ClientHasOrdersForThisDate),
				$"У клиента уже есть оформленные заказы: { orderIds } на этот день!");
	}
}
