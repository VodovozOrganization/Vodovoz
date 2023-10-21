using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Domain.Logistic
{
	public class RouteListItemWageCalculationSource : IRouteListItemWageCalculationSource
	{
		private readonly RouteListItem item;
		private readonly EmployeeCategory employeeCategory;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;

		public RouteListItemWageCalculationSource(RouteListItem item, EmployeeCategory employeeCategory,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider)
		{
			this.employeeCategory = employeeCategory;
			this.item = item ?? throw new ArgumentNullException(nameof(item));
			_deliveryRulesParametersProvider = deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider)); ;
		}

		#region IRouteListItemWageCalculationSource implementation

		public int FullBottle19LCount => (int)item.Order.OrderItems
			.Where(i => i.Nomenclature.Category == NomenclatureCategory.water && i.Nomenclature.TareVolume == TareVolume.Vol19L)
			.Sum(i => i.CurrentCount);

		public int EmptyBottle19LCount => item.RouteListIsUnloaded() || item.RouteList.Status == RouteListStatus.MileageCheck
			? item.BottlesReturned : item.Order.BottlesReturn ?? 0;

		public int Bottle6LCount => (int)item.Order.OrderItems
			.Where(i => i.Nomenclature.Category == NomenclatureCategory.water && i.Nomenclature.TareVolume == TareVolume.Vol6L)
			.Sum(i => i.CurrentCount);

		public int Bottle600mlCount => (int)item.Order.OrderItems
			.Where(i => i.Nomenclature.TareVolume == TareVolume.Vol600ml)
			.Sum(i => i.CurrentCount);
		
		public int Bottle1500mlCount => (int)item.Order.OrderItems
			.Where(i => i.Nomenclature.TareVolume == TareVolume.Vol1500ml)
			.Sum(i => i.CurrentCount);
		
		public int Bottle500mlCount => (int)item.Order.OrderItems
			.Where(i => i.Nomenclature.TareVolume == TareVolume.Vol500ml)
			.Sum(i => i.CurrentCount);

		public bool ContractCancelation => false;

		public CarTypeOfUse CarTypeOfUse => item.RouteList.Car?.CarModel?.CarTypeOfUse
			?? throw new InvalidOperationException("Модель автомобиля в МЛ должна быть заполнена");

		public IEnumerable<IOrderItemWageCalculationSource> OrderItemsSource => item.Order.OrderItems;

		public IEnumerable<IOrderDepositItemWageCalculationSource> OrderDepositItemsSource => item.Order.OrderDepositItems;
		public bool IsDriverForeignDistrict
		{
			get
			{
				var driverDistricts = item.RouteList.Driver.DriverDistrictPrioritySets
					.FirstOrDefault(x =>
						item.RouteList.Date >= x.DateActivated && (x.DateDeactivated == null || item.RouteList.Date <= x.DateDeactivated))
					?.DriverDistrictPriorities
					?.Select(x => x.District);

				return driverDistricts == null || !driverDistricts.Contains(item.Order.DeliveryPoint.District);
			}
		}

		public bool HasFirstOrderForDeliveryPoint {
			get
			{
				var sameAddress = item.RouteList.Addresses
					.Where(i => i.IsValidForWageCalculation())
					.Select(i => i.Order)
					.FirstOrDefault(o => o.DeliveryPoint?.Id == item.Order.DeliveryPoint?.Id); 
				if(sameAddress == null) {
					return false;
				}

				return sameAddress.Id == item.Order.Id;
			}
		}

		public WageDistrict WageDistrictOfAddress => item.Order.DeliveryPoint.District?.WageDistrict
			?? throw new InvalidOperationException($"Точке доставки не присвоен логистический или зарплатный район! (Id адреса: {item.Id})");

		public bool WasVisitedByForwarder => item.WithForwarder;

		public bool NeedTakeOrDeliverEquipment {
			get {
				bool result = item.Order.OrderItems.Any(i => i.CurrentCount > 0 && i.Nomenclature.Category == NomenclatureCategory.equipment);
				result |= item.Order.OrderEquipments.Any(i => i.CurrentCount > 0 && i.Nomenclature.Category == NomenclatureCategory.equipment);
				return result;
			}
		}

		public bool IsFastDelivery => item.Order.IsFastDelivery;

		public WageRateTypes GetFastDeliveryWageRateType()
		{
			var hasFastDeliveryLate =
				(item.Order.TimeDelivered ?? item.StatusLastUpdate) - item.CreationDate > _deliveryRulesParametersProvider.MaxTimeForFastDelivery;

			return hasFastDeliveryLate ? WageRateTypes.FastDeliveryWithLate : WageRateTypes.FastDelivery;
		}

		#region Для старого расчета оплаты за оборудование

		/// <summary>
		/// Текущая зарплата которая записана в МЛ
		/// </summary>
		public decimal CurrentWage {
			get {
				switch(employeeCategory) {
					case EmployeeCategory.driver:
						return item.DriverWage;
					case EmployeeCategory.forwarder:
						return item.ForwarderWage;
					case EmployeeCategory.office:
					default:
						throw new NotSupportedException();
				}
			}
		}

		#endregion Для старого расчета оплаты за оборудование


		public WageDistrictLevelRate WageCalculationMethodic {
			get {
				switch(employeeCategory) {
					case EmployeeCategory.driver:
						return item.DriverWageCalculationMethodic;
					case EmployeeCategory.forwarder:
						return item.ForwarderWageCalculationMethodic;
					case EmployeeCategory.office:
					default:
						throw new NotSupportedException();
				}
			}
		}

		public decimal DriverWageSurcharge => item.DriverWageSurcharge;

		public bool IsDelivered => item.IsDelivered();
		public bool IsValidForWageCalculation => item.IsValidForWageCalculation();

		public (TimeSpan, TimeSpan) DeliverySchedule => (item.Order.DeliverySchedule.From, item.Order.DeliverySchedule.To);

		public EmployeeCategory EmployeeCategory => employeeCategory;

		#endregion IRouteListItemWageCalculationSource implementation
	}
}
