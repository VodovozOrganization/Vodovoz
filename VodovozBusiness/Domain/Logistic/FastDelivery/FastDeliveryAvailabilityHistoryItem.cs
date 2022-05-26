using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic.FastDelivery
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "строка истории доступности экспресс-доставки",
		NominativePlural = "строки истории доступности экспресс-доставки")]
	public class FastDeliveryAvailabilityHistoryItem : PropertyChangedBase, IDomainObject
	{
		private RouteList _routeList;
		private Employee _driver;
		private bool _isGoodsEnough;
		private int _unclosedFastDeliveries;
		private TimeSpan _remainingTimeForShipmentNewOrder;
		private TimeSpan _lastCoordinateTime;
		private decimal _distanceByLineToClient;
		private decimal _distanceByRoadToClient;
		private int _id;
		private bool _isValidDistanceByLineToClient;
		private bool _isValidDistanceByRoadToClient;
		private bool _isValidLastCoordinateTime;
		private bool _isValidRemainingTimeForShipmentNewOrder;
		private bool _isValidIsGoodsEnough;
		private bool _isValidUnclosedFastDeliveries;
		private FastDeliveryAvailabilityHistory _fastDeliveryAvailabilityHistory;
		private bool _isValidToFastDelivery;

		#region Свойства

		public virtual int Id
		{
			get => _id;
			set => _id = value;
		}

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		[Display(Name = "Достаточно остатков на борту")]
		public virtual bool IsGoodsEnough
		{
			get => _isGoodsEnough;
			set => SetField(ref _isGoodsEnough, value);
		}

		[Display(Name = "Незакрытые экспресс-доставки")]
		public virtual int UnclosedFastDeliveries
		{
			get => _unclosedFastDeliveries;
			set => SetField(ref _unclosedFastDeliveries, value);
		}

		[Display(Name = "Остаток времени на отгрузку нового заказа")]
		public virtual TimeSpan RemainingTimeForShipmentNewOrder
		{
			get => _remainingTimeForShipmentNewOrder;
			set => SetField(ref _remainingTimeForShipmentNewOrder, value);
		}

		[Display(Name = "Время получения последних коодинат")]
		public virtual TimeSpan LastCoordinateTime
		{
			get => _lastCoordinateTime;
			set => SetField(ref _lastCoordinateTime, value);
		}

		[Display(Name = "Расстояние до клиента по прямой")]
		public virtual decimal DistanceByLineToClient
		{
			get => _distanceByLineToClient;
			set => SetField(ref _distanceByLineToClient, value);
		}

		[Display(Name = "Расстояние до клиента по дорогам")]
		public virtual decimal DistanceByRoadToClient
		{
			get => _distanceByRoadToClient;
			set => SetField(ref _distanceByRoadToClient, value);
		}

		[Display(Name = "Расстояние до клиента по прямой подходит")]
		public virtual bool IsValidDistanceByLineToClient
		{
			get => _isValidDistanceByLineToClient;
			set => SetField(ref _isValidDistanceByLineToClient, value);
		}

		[Display(Name = "Расстояние до клиента по дорогам подходит")]
		public virtual bool IsValidDistanceByRoadToClient
		{
			get => _isValidDistanceByRoadToClient;
			set => SetField(ref _isValidDistanceByRoadToClient, value);
		}

		[Display(Name = "Время получения последних коодинат не слишком старое")]
		public virtual bool IsValidLastCoordinateTime
		{
			get => _isValidLastCoordinateTime;
			set => SetField(ref _isValidLastCoordinateTime, value);
		}

		[Display(Name = "Достаточно времени на отгрузку нового заказа")]
		public virtual bool IsValidRemainingTimeForShipmentNewOrder
		{
			get => _isValidRemainingTimeForShipmentNewOrder;
			set => SetField(ref _isValidRemainingTimeForShipmentNewOrder, value);
		}

		[Display(Name = "Достаточно остатков на борту")]
		public virtual bool IsValidIsGoodsEnough
		{
			get => _isValidIsGoodsEnough;
			set => SetField(ref _isValidIsGoodsEnough, value);
		}

		[Display(Name = "Незакрытых экспресс-доставки немного")]
		public virtual bool IsValidUnclosedFastDeliveries
		{
			get => _isValidUnclosedFastDeliveries;
			set => SetField(ref _isValidUnclosedFastDeliveries, value);
		}

		[Display(Name = "История доступности экспресс-доставки")]
		public virtual FastDeliveryAvailabilityHistory FastDeliveryAvailabilityHistory
		{
			get => _fastDeliveryAvailabilityHistory;
			set => SetField(ref _fastDeliveryAvailabilityHistory, value);
		}

		[Display(Name = "МЛ подходит для экспресс-доставки")]
		public virtual bool IsValidToFastDelivery
		{
			get => _isValidToFastDelivery;
			set => SetField(ref _isValidToFastDelivery, value);
		}

		#endregion
	}
}
