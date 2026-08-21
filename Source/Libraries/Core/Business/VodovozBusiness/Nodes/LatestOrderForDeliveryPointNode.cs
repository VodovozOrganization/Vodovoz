using System;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Данные для отображения информации по последним заказам по конкретной ТД
	/// </summary>
	public class LatestOrderForDeliveryPointNode
	{
		private LatestOrderForDeliveryPointNode(DateTime? deliveryDate, PaymentType paymentType, int? total19LBottles)
		{
			DeliveryDate = deliveryDate;
			PaymentType = paymentType;
			Total19LBottlesToDeliver = total19LBottles ?? 0;
		}
		
		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
		/// <summary>
		/// Тип оплаты
		/// </summary>
		public PaymentType PaymentType { get; set; }
		/// <summary>
		/// Количество воды в 19л бутылях
		/// </summary>
		public int Total19LBottlesToDeliver { get; set; }
		
		public static LatestOrderForDeliveryPointNode Create(DateTime? deliveryDate, PaymentType paymentType, int? total19LBottles) =>
			new LatestOrderForDeliveryPointNode(deliveryDate, paymentType, total19LBottles);
	}
}
