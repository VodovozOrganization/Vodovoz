<<<<<<<< HEAD:Source/Libraries/Core/Backend/CustomerOrders.Contracts/Interfaces/IOnlineOrderedProduct.cs
﻿namespace CustomerOrders.Contracts.Interfaces
========
﻿namespace Vodovoz.Core.Data.V4
>>>>>>>> origin/5696_AddCreatingOnlineOrderFromTemplate:Source/Libraries/Core/Business/Vodovoz.Core.Data/V4/IOnlineOrderedProductV4.cs
{
	public interface IOnlineOrderedProductV4
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		int NomenclatureId { get; }
		/// <summary>
		/// Цена
		/// </summary>
		decimal Price { get; set; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Id промонабора
		/// </summary>
		int? PromoSetId { get; }
		/// <summary>
		/// Скидка в деньгах?
		/// </summary>
		bool IsDiscountInMoney { get; set; }
		/// <summary>
		/// Скидка
		/// </summary>
		decimal Discount { get; set; }
		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		int? DiscountReasonId { get; set; }
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; set; }
		/// <summary>
		/// Цена со скидкой
		/// </summary>
		decimal PriceWithDiscount { get; }
		/// <summary>
		/// Очистка данных по скидке
		/// </summary>
		void ClearDiscount();
	}
}
