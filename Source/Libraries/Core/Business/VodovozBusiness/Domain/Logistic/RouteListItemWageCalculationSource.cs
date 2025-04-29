using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.Settings.Delivery;
using Vodovoz.Tools.Exceptions;

namespace Vodovoz.Domain.Logistic
{
	public class RouteListItemWageCalculationSource : IRouteListItemWageCalculationSource
	{
		private readonly RouteListItem _item;
		private readonly EmployeeCategory _employeeCategory;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;

		public RouteListItemWageCalculationSource(
			RouteListItem item,
			EmployeeCategory employeeCategory,
			IDeliveryRulesSettings deliveryRulesSettings)
		{
			_employeeCategory = employeeCategory;
			_item = item ?? throw new ArgumentNullException(nameof(item));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings)); ;
		}

		#region IRouteListItemWageCalculationSource implementation

		public int FullBottle19LCount => (int)_item.Order.OrderItems
			.Where(i => i.Nomenclature.Category == NomenclatureCategory.water && i.Nomenclature.TareVolume == TareVolume.Vol19L)
			.Sum(i => i.CurrentCount);

		public int EmptyBottle19LCount => _item.RouteListIsUnloaded() || _item.RouteList.Status == RouteListStatus.MileageCheck
			? _item.BottlesReturned : _item.Order.BottlesReturn ?? 0;

		public int Bottle6LCount => (int)_item.Order.OrderItems
			.Where(i => i.Nomenclature.Category == NomenclatureCategory.water && i.Nomenclature.TareVolume == TareVolume.Vol6L)
			.Sum(i => i.CurrentCount);

		public int Bottle600mlCount => (int)_item.Order.OrderItems
			.Where(i => i.Nomenclature.TareVolume == TareVolume.Vol600ml)
			.Sum(i => i.CurrentCount);

		public int Bottle1500mlCount => (int)_item.Order.OrderItems
			.Where(i => i.Nomenclature.TareVolume == TareVolume.Vol1500ml)
			.Sum(i => i.CurrentCount);

		public int Bottle500mlCount => (int)_item.Order.OrderItems
			.Where(i => i.Nomenclature.TareVolume == TareVolume.Vol500ml)
			.Sum(i => i.CurrentCount);

		public bool ContractCancelation => false;

		public CarTypeOfUse CarTypeOfUse => _item.RouteList.Car?.CarModel?.CarTypeOfUse
			?? throw new InvalidOperationException("Модель автомобиля в МЛ должна быть заполнена");

		public IEnumerable<IOrderItemWageCalculationSource> OrderItemsSource => _item.Order.OrderItems;

		public IEnumerable<IOrderDepositItemWageCalculationSource> OrderDepositItemsSource => _item.Order.OrderDepositItems;
		public bool IsDriverForeignDistrict
		{
			get
			{
				var driverDistricts = _item.RouteList.Driver.DriverDistrictPrioritySets
					.FirstOrDefault(x =>
						_item.RouteList.Date >= x.DateActivated && (x.DateDeactivated == null || _item.RouteList.Date <= x.DateDeactivated))
					?.DriverDistrictPriorities
					?.Select(x => x.District);

				return driverDistricts == null || !driverDistricts.Contains(_item.Order.DeliveryPoint.District);
			}
		}

		public bool HasFirstOrderForDeliveryPoint
		{
			get
			{
				var sameAddress = _item.RouteList.Addresses
					.Where(i => i.IsValidForWageCalculation())
					.Select(i => i.Order)
					.FirstOrDefault(o => o.DeliveryPoint?.Id == _item.Order.DeliveryPoint?.Id);

				if(sameAddress == null)
				{
					return false;
				}

				return sameAddress.Id == _item.Order.Id;
			}
		}

		public WageDistrict WageDistrictOfAddress => _item.Order.DeliveryPoint.District?.WageDistrict
			?? throw new DeliveryPointDistrictNotFoundException($"Точке доставки {_item.Order.DeliveryPoint.Id} не присвоен логистический или зарплатный район! (Id адреса: {_item.Id})");

		public bool WasVisitedByForwarder => _item.WithForwarder;

		public bool NeedTakeOrDeliverEquipment
		{
			get
			{
				bool result = _item.Order.OrderItems.Any(i => i.CurrentCount > 0 && i.Nomenclature.Category == NomenclatureCategory.equipment);
				result |= _item.Order.OrderEquipments.Any(i => i.CurrentCount > 0 && i.Nomenclature.Category == NomenclatureCategory.equipment);
				return result;
			}
		}

		public bool IsFastDelivery => _item.Order.IsFastDelivery;

		public WageRateTypes GetFastDeliveryWageRateType()
		{
			var hasFastDeliveryLate =
				(_item.Order.TimeDelivered ?? _item.StatusLastUpdate) - _item.CreationDate > _deliveryRulesSettings.MaxTimeForFastDelivery;

			return hasFastDeliveryLate ? WageRateTypes.FastDeliveryWithLate : WageRateTypes.FastDelivery;
		}

		#region Для старого расчета оплаты за оборудование

		/// <summary>
		/// Текущая зарплата которая записана в МЛ
		/// </summary>
		public decimal CurrentWage
		{
			get
			{
				switch(_employeeCategory)
				{
					case EmployeeCategory.driver:
						return _item.DriverWage;
					case EmployeeCategory.forwarder:
						return _item.ForwarderWage;
					case EmployeeCategory.office:
					default:
						throw new NotSupportedException();
				}
			}
		}

		#endregion Для старого расчета оплаты за оборудование

		public WageDistrictLevelRate WageCalculationMethodic
		{
			get
			{
				switch(_employeeCategory)
				{
					case EmployeeCategory.driver:
						return _item.DriverWageCalculationMethodic;
					case EmployeeCategory.forwarder:
						return _item.ForwarderWageCalculationMethodic;
					case EmployeeCategory.office:
					default:
						throw new NotSupportedException();
				}
			}
		}

		public decimal DriverWageSurcharge => _item.DriverWageSurcharge;

		public bool IsDelivered => _item.IsDelivered();
		public bool IsValidForWageCalculation => _item.IsValidForWageCalculation();

		public (TimeSpan, TimeSpan) DeliverySchedule => (_item.Order.DeliverySchedule.From, _item.Order.DeliverySchedule.To);

		public EmployeeCategory EmployeeCategory => _employeeCategory;

		#endregion IRouteListItemWageCalculationSource implementation
	}
}
