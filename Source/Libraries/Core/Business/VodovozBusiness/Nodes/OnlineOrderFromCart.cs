using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Онлайн заказ из корзины ИПЗ
	/// </summary>
	public class OnlineOrderFromCart : IOnlineOrderFromCart
	{
		public OnlineOrderFromCart(){ }

		protected OnlineOrderFromCart(
			DateTime? deliveryDate,
			DeliveryPoint deliveryPoint,
			IEnumerable<IGoods> goods,
			bool isSelfDelivery
			)
		{
			DeliveryDate = deliveryDate;
			DeliveryPoint = deliveryPoint;
			Goods = goods;
			IsSelfDelivery = isSelfDelivery;
		}

		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
		/// <summary>
		/// Точка доставки
		/// </summary>
		public DeliveryPoint DeliveryPoint { get; set; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		/// <summary>
		/// Товары
		/// </summary>
		public IEnumerable<IGoods> Goods { get; set; }
		
		public static IOnlineOrderFromCart Create(
			DeliveryPoint deliveryPoint,
			IEnumerable<IGoods> goods,
			bool isSelfDelivery,
			DateTime? deliveryDate) => new OnlineOrderFromCart(deliveryDate, deliveryPoint, goods, isSelfDelivery);
	}
}
