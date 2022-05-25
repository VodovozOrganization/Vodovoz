using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic.FastDelivery
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "история доступности экспресс-доставки",
		NominativePlural = "истории доступности экспресс-доставки")]

	public class FastDeliveryAvailabilityHistory : PropertyChangedBase, IDomainObject
	{
		private DateTime _verificationDate;
		private Order _order;
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private District _district;
		private bool _isValid;
		private Employee _logistician;
		private string _logisticianComment;
		private DateTime _logisticianCommentVersion;
		private DateTime? _logisticianReactionTime;
		private Double _fastDeliveryMaxDistanceKm;
		private Employee _author;
		private IList<FastDeliveryAvailabilityHistoryItem> _items;
		private IList<FastDeliveryOrderItemsHistory> _orderItemsHistoryItems;
		private IList<FastDeliveryNomenclatureDistributionHistory> _nomenclatureDistributionHistoryItems;
		private bool _isGetClosestByRoute;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Дата проверки")]
		public virtual DateTime VerificationDate
		{
			get => _verificationDate;
			set => SetField(ref _verificationDate, value);
		}

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		[Display(Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		[Display(Name = "Район")]
		public virtual District District
		{
			get => _district;
			set => SetField(ref _district, value);
		}

		[Display(Name = "Доступно для экспресс-доставки")]
		public virtual bool IsValid
		{
			get => _isValid;
			set => SetField(ref _isValid, value);
		}

		[Display(Name = "Логист")]
		public virtual Employee Logistician
		{
			get => _logistician;
			set => SetField(ref _logistician, value);
		}

		[Display(Name = "Комментарий логиста")]
		public virtual string LogisticianComment
		{
			get => _logisticianComment;
			set => SetField(ref _logisticianComment, value);
		}

		[Display(Name = "Дата и время комментария логиста")]
		public virtual DateTime LogisticianCommentVersion
		{
			get => _logisticianCommentVersion;
			set => SetField(ref _logisticianCommentVersion, value);
		}

		[Display(Name = "Время реакции логиста")]
		public virtual DateTime? LogisticianReactionTime
		{
			get => _logisticianReactionTime;
			set => SetField(ref _logisticianReactionTime, value);
		}

		[Display(Name = "Радиус охвата экспресс-доставки в км")]
		public virtual Double FastDeliveryMaxDistanceKm
		{
			get => _fastDeliveryMaxDistanceKm;
			set => SetField(ref _fastDeliveryMaxDistanceKm, value);
		}

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Расстояние по дорогам")]
		public virtual bool IsGetClosestByRoute
		{
			get => _isGetClosestByRoute;
			set => SetField(ref _isGetClosestByRoute, value);
		}

		#endregion

		//[Display(Name = "Строки истории доступности экспресс-доставки")]
		//public virtual IList<FastDeliveryAvailabilityHistoryItem> Items
		//{
		//	get => _items;
		//	set => SetField(ref _items, value);
		//}

		//[Display(Name = "Строки заказа итории экспресс-доставки")]
		//public virtual IList<FastDeliveryOrderItemsHistory> OrderItemsHistoryItems
		//{
		//	get => _orderItemsHistoryItems;
		//	set => SetField(ref _orderItemsHistoryItems, value);
		//}

		//[Display(Name = "Строки истории распределения номенклатур для экспресс-доставки")]
		//public virtual IList<FastDeliveryNomenclatureDistributionHistory> NomenclatureDistributionHistoryItems
		//{
		//	get => _nomenclatureDistributionHistoryItems;
		//	set => SetField(ref _nomenclatureDistributionHistoryItems, value);
		//}
	}
}
