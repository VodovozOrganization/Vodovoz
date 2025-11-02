using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic.FastDelivery
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "история доступности экспресс-доставки",
		NominativePlural = "истории доступности экспресс-доставки")]
	[EntityPermission]

	public class FastDeliveryAvailabilityHistory : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private DateTime _verificationDate;
		private Order _order;
		private Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private District _district;
		private Employee _logistician;
		private string _logisticianComment;
		private DateTime _logisticianCommentVersion;
		private Employee _author;
		private IList<FastDeliveryAvailabilityHistoryItem> _items = new List<FastDeliveryAvailabilityHistoryItem>();
		private IList<FastDeliveryOrderItemHistory> _orderItemsHistory;
		private IList<FastDeliveryNomenclatureDistributionHistory> _nomenclatureDistributionHistoryItems;
		private bool _isGetClosestByRoute;
		private double _maxDistanceToLatestTrackPointKm;
		private int _driverGoodWeightLiftPerHandInKg;
		private int _maxFastOrdersPerSpecificTime;
		private TimeSpan _maxTimeForFastDelivery;
		private TimeSpan _minTimeForNewFastDeliveryOrder;
		private TimeSpan _driverUnloadTime;
		private TimeSpan _specificTimeForMaxFastOrdersCount;
		private string _addressWithoutDeliveryPoint;

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

		[Display(Name = "Адрес без точки доставки (сайт)")]
		public virtual string AddressWithoutDeliveryPoint 
		{
			get => _addressWithoutDeliveryPoint; 
			set => SetField(ref _addressWithoutDeliveryPoint, value);
		}

		[Display(Name = "Район")]
		public virtual District District
		{
			get => _district;
			set => SetField(ref _district, value);
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

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Расстояние по дорогам?")]
		public virtual bool IsGetClosestByRoute
		{
			get => _isGetClosestByRoute;
			set => SetField(ref _isGetClosestByRoute, value);
		}

		[Display(Name = "Радиус охвата экспресс-доставки")]
		public virtual double MaxDistanceToLatestTrackPointKm
		{
			get => _maxDistanceToLatestTrackPointKm;
			set => SetField(ref _maxDistanceToLatestTrackPointKm, value);
		}

		[Display(Name = "Вес для одной руки")]
		public virtual int DriverGoodWeightLiftPerHandInKg
		{
			get => _driverGoodWeightLiftPerHandInKg;
			set => SetField(ref _driverGoodWeightLiftPerHandInKg, value);
		}

		[Display(Name = "Максимальное время для экспресс-доставки")]
		public virtual TimeSpan MaxTimeForFastDelivery
		{
			get => _maxTimeForFastDelivery;
			set => SetField(ref _maxTimeForFastDelivery, value);
		}

		[Display(Name = "Минимальное время экспресс-доставки")]
		public virtual TimeSpan MinTimeForNewFastDeliveryOrder
		{
			get => _minTimeForNewFastDeliveryOrder;
			set => SetField(ref _minTimeForNewFastDeliveryOrder, value);
		}

		[Display(Name = "Время разгрузки")]
		public virtual TimeSpan DriverUnloadTime
		{
			get => _driverUnloadTime;
			set => SetField(ref _driverUnloadTime, value);
		}

		[Display(Name = "Максимальное количество экспресс-доставок в определённое время")]
		public virtual int MaxFastOrdersPerSpecificTime
		{
			get => _maxFastOrdersPerSpecificTime;
			set => SetField(ref _maxFastOrdersPerSpecificTime, value);
		}

		[Display(Name = "Время для максимального количества экспресс-доставок")]
		public virtual TimeSpan SpecificTimeForMaxFastOrdersCount
		{
			get => _specificTimeForMaxFastOrdersCount;
			set => SetField(ref _specificTimeForMaxFastOrdersCount, value);
		}

		#endregion

		[Display(Name = "Строки истории доступности экспресс-доставки")]
		public virtual IList<FastDeliveryAvailabilityHistoryItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		[Display(Name = "Строки заказа истории экспресс-доставки")]
		public virtual IList<FastDeliveryOrderItemHistory> OrderItemsHistory
		{
			get => _orderItemsHistory;
			set => SetField(ref _orderItemsHistory, value);
		}

		[Display(Name = "Строки истории распределения номенклатур для экспресс-доставки")]
		public virtual IList<FastDeliveryNomenclatureDistributionHistory> NomenclatureDistributionHistoryItems
		{
			get => _nomenclatureDistributionHistoryItems;
			set => SetField(ref _nomenclatureDistributionHistoryItems, value);
		}

		public virtual IEnumerable<string> AdditionalInformation { get; set; }

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(LogisticianComment?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина комментария ({LogisticianComment.Length}/255).",
					new[] { nameof(LogisticianComment) });
			}
		}

		#endregion
	}
}
