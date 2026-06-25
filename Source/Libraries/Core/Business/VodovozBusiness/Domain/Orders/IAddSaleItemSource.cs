using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Domain.Orders
{
	public interface IAddSaleItemSource : IGetFixedPriceSource
	{
		int Id { get; }
		object Source { get; }
		PaymentTypeSource PaymentTypeSource { get; }
		DateTime? DeliveryDate { get; }
		bool IsSelfDelivery { get; }
		bool IsFastDelivery { get; }
		bool IsLoadedFrom1C { get; }
		bool HasDeposits { get; }
		bool HasNonPaidDeliveries { get; }
		ICollection<IProduct> Products { get; }
		IEnumerable<IPromoSetForNewClient> PromoSets { get; }
		//ICollection<IPromoSet> PromoSets { get; }
		int GetTotalWater19LCount(bool doNotCountWaterFromPromoSets = false, bool doNotCountPresentsDiscount = false);
		/// <summary>
		/// Пересчет цены
		/// </summary>
		void RecalculateItemsPrice();
		/// <summary>
		/// Обновление количества аренды
		/// </summary>
		void UpdateRentsCount();

		void OnSumPropertiesChanged();
	}
	
	/// <summary>
	/// Контракт расчета фиксы в источнике(заказ, шаблон и т.д.)
	/// </summary>
	public interface IGetFixedPriceSource
	{
		/// <summary>
		/// Количество позиции в заказе
		/// </summary>
		/// <param name="addingItem">Добавляемая позиция</param>
		/// <returns>Количество</returns>
		decimal TotalItemCount(INomenclatureCount addingItem);
		/// <summary>
		/// Клиент <see cref="Vodovoz.Domain.Client.Counterparty"/>
		/// </summary>
		Counterparty Counterparty { get; }
		/// <summary>
		/// Точка доставки <see cref="Vodovoz.Domain.Client.DeliveryPoint"/>
		/// </summary>
		DeliveryPoint DeliveryPoint { get; }
	}
}
