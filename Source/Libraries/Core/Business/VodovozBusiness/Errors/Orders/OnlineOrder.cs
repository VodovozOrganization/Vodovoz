using VodovozInfrastructure.Extensions;

namespace Vodovoz.Errors.Orders
{
	public static partial class OnlineOrder
	{
		public static Error IsEmptyCounterparty =>
			new Error(
				typeof(OnlineOrder),
				nameof(IsEmptyCounterparty),
				"Контрагент не заполнен");
		
		public static Error IsEmptySelfDeliveryGeoGroup =>
			new Error(
				typeof(OnlineOrder),
				nameof(IsEmptySelfDeliveryGeoGroup),
				"Не указана гео группа для самовывоза");
		
		public static Error IsEmptyDeliveryPoint =>
			new Error(
				typeof(OnlineOrder),
				nameof(IsEmptyDeliveryPoint),
				"Не указана точка доставки");
		
		public static Error IsEmptyDeliverySchedule =>
			new Error(
				typeof(OnlineOrder),
				nameof(IsEmptyDeliverySchedule),
				"Не заполнен график доставки");
		
		public static Error FastDeliveryNotAvailable =>
			new Error(
				typeof(OnlineOrder),
				nameof(FastDeliveryNotAvailable),
				"Быстрая доставка не доступна");
		
		public static Error IncorrectCountNomenclatureInOnlineOrderPromoSet(
			string promoSetTitle, int position, string nomenclature, int countFromPromoSet, int countFromOnlineOrderItem) =>
			new Error(
				typeof(OnlineOrder),
				nameof(IncorrectCountNomenclatureInOnlineOrderPromoSet),
				$"В переданном промонаборе {promoSetTitle} в позиции {position} {nomenclature}" +
				$"\nнеправильно указано количество: должно быть {countFromPromoSet}, а передано {countFromOnlineOrderItem}");
		
		public static Error IncorrectPriceNomenclatureInOnlineOrder(
			string nomenclature, decimal price, decimal onlineOrderItemPrice) =>
			new Error(
				typeof(OnlineOrder),
				nameof(IncorrectPriceNomenclatureInOnlineOrder),
				$"Номенклатура {nomenclature} пришла с неправильно установленной ценой" +
				$"\nДолжно быть {price}, а передано {onlineOrderItemPrice}");
			
		public static Error IncorrectDiscountTypeInOnlineOrderPromoSet(
			string promoSetTitle, int position, string nomenclature, bool discountInMoneyFromPromoSet, bool onlineOrderItemDiscountInMoney) =>
			new Error(
				typeof(OnlineOrder),
				nameof(IncorrectDiscountInOnlineOrderPromoSet),
				$"В переданном промонаборе {promoSetTitle} в позиции {position} {nomenclature}" +
				$"\nнеправильно указан тип скидки, должно быть скидка в рублях: {discountInMoneyFromPromoSet.ConvertToYesOrNo()}, а передано {onlineOrderItemDiscountInMoney.ConvertToYesOrNo()}");
		
		public static Error IncorrectDiscountInOnlineOrderPromoSet(
			string promoSetTitle, int position, string nomenclature, decimal discountItemFromPromoSet, decimal onlineOrderItemDiscount) =>
			new Error(
				typeof(OnlineOrder),
				nameof(IncorrectDiscountInOnlineOrderPromoSet),
				$"В переданном промонаборе {promoSetTitle} в позиции {position} {nomenclature}" +
				$"\nнеправильно указана скидка: должно быть {discountItemFromPromoSet}, а передано {onlineOrderItemDiscount}");
		
		public static Error IncorrectRentPackagePriceInOnlineOrder(
			int rentPackageId, decimal onlineRentPackagePrice, decimal depositFromRentPackage) =>
			new Error(
				typeof(OnlineOrder),
				nameof(IncorrectRentPackagePriceInOnlineOrder),
				$"В переданном пакете аренды {rentPackageId} не совпадает цена." +
				$"\nВ онлайн заказе {onlineRentPackagePrice}" +
				$"\nВ Пакете аренды {depositFromRentPackage}");
	}
}
