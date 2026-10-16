using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Заказ в составе группы отправки сделки по отсрочке сети.
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Nominative = "заказ отправки сделки по отсрочке сети",
		NominativePlural = "заказы отправок сделок по отсрочке сети")]
	public class ChainStoreDelayBitrixDealOrder : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private ChainStoreDelayBitrixDeal _deal;
		private int _orderId;
		private int? _deliveryPointId;
		private bool _isSelfDelivery;
		private DateTime _estimatedPaymentDate;
		private decimal _orderSum;
		private decimal _debt;

		/// <summary>
		/// Идентификатор записи.
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Отправка сделки, в состав которой входит заказ.
		/// </summary>
		[Display(Name = "Отправка сделки")]
		public virtual ChainStoreDelayBitrixDeal Deal
		{
			get => _deal;
			set => SetField(ref _deal, value);
		}

		/// <summary>
		/// Номер заказа.
		/// </summary>
		[Display(Name = "Номер заказа")]
		public virtual int OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		/// <summary>
		/// Идентификатор точки доставки (null - самовывоз или заказ без точки).
		/// </summary>
		[Display(Name = "Точка доставки")]
		public virtual int? DeliveryPointId
		{
			get => _deliveryPointId;
			set => SetField(ref _deliveryPointId, value);
		}

		/// <summary>
		/// Признак самовывоза.
		/// </summary>
		[Display(Name = "Самовывоз")]
		public virtual bool IsSelfDelivery
		{
			get => _isSelfDelivery;
			set => SetField(ref _isSelfDelivery, value);
		}

		/// <summary>
		/// Расчётная дата платежа на момент сбора.
		/// </summary>
		[Display(Name = "Расчётная дата платежа")]
		public virtual DateTime EstimatedPaymentDate
		{
			get => _estimatedPaymentDate;
			set => SetField(ref _estimatedPaymentDate, value);
		}

		/// <summary>
		/// Сумма заказа на момент сбора.
		/// </summary>
		[Display(Name = "Сумма заказа")]
		public virtual decimal OrderSum
		{
			get => _orderSum;
			set => SetField(ref _orderSum, value);
		}

		/// <summary>
		/// Долг по заказу на момент сбора.
		/// </summary>
		[Display(Name = "Долг")]
		public virtual decimal Debt
		{
			get => _debt;
			set => SetField(ref _debt, value);
		}
	}
}
