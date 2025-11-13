using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Domain
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "константы рентабельности",
		Nominative = "константы рентабельности",
		Prepositional = "константах рентабельности",
		PrepositionalPlural = "константах рентабельности")]
	[HistoryTrace]
	public class ProfitabilityConstants : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _threeYearsInMonths = 36;
		private DateTime _calculatedMonth;
		
		private int _administrativeExpenses;
		private int _administrativeTotalShipped;
		private decimal _administrativeExpensesPerKg;
		private IList<ProductGroup> _administrativeProductGroupsFilter = new List<ProductGroup>();
		private IList<Warehouse> _administrativeWarehousesFilter = new List<Warehouse>();
		private GenericObservableList<ProductGroup> _observableAdministrativeProductGroupsFilter;
		private GenericObservableList<Warehouse> _observableAdministrativeWarehousesFilter;

		private int _warehouseExpenses;
		private int _warehousesTotalShipped;
		private decimal _warehouseExpensesPerKg;
		private IList<ProductGroup> _warehouseExpensesProductGroupsFilter = new List<ProductGroup>();
		private IList<Warehouse> _warehouseExpensesWarehousesFilter = new List<Warehouse>();
		private GenericObservableList<ProductGroup> _observableWarehouseExpensesProductGroupsFilter;
		private GenericObservableList<Warehouse> _observableWarehouseExpensesWarehousesFilter;

		private int _decreaseGazelleCostFor3Year;
		private int _decreaseLargusCostFor3Year;
		private int _decreaseMinivanCostFor3Year;
		private int _decreaseTruckCostFor3Year;
		private int _gazelleAverageMileage;
		private int _largusAverageMileage;
		private int _minivanAverageMileage;
		private int _truckAverageMileage;
		private decimal _gazelleAmortisationPerKm;
		private decimal _largusAmortisationPerKm;
		private decimal _minivanAmortisationPerKm;
		private decimal _truckAmortisationPerKm;

		private int _operatingExpensesAllGazelles;
		private int _operatingExpensesAllLarguses;
		private int _operatingExpensesAllMinivans;
		private int _operatingExpensesAllTrucks;
		private int _averageMileageAllGazelles;
		private int _averageMileageAllLarguses;
		private int _averageMileageAllMinivans;
		private int _averageMileageAllTrucks;
		private decimal _gazelleRepairCostPerKm;
		private decimal _largusRepairCostPerKm;
		private decimal _minivanRepairCostPerKm;
		private decimal _truckRepairCostPerKm;
		private IList<CarEventType> _repairCostCarEventTypeTypesFilter = new List<CarEventType>();
		private GenericObservableList<CarEventType> _observableRepairCostCarEventTypesFilter;

		private DateTime? _calculationSaved;
		private Employee _calculationAuthor;

		public virtual int Id { get; set; }

		[Display(Name = "Расчетный период")]
		public virtual DateTime CalculatedMonth
		{
			get => _calculatedMonth;
			set => SetField(ref _calculatedMonth, value);
		}

		#region Административные расходы

		[Display(Name = "Административные расходы за расчетный период")]
		public virtual int AdministrativeExpenses
		{
			get => _administrativeExpenses;
			set => SetField(ref _administrativeExpenses, value);
		}

		[Display(Name = "Отгружено всего ОХР")]
		public virtual int AdministrativeTotalShipped
		{
			get => _administrativeTotalShipped;
			set => SetField(ref _administrativeTotalShipped, value);
		}

		[Display(Name = "ОХР на кг")]
		public virtual decimal AdministrativeExpensesPerKg
		{
			get => _administrativeExpensesPerKg;
			set => SetField(ref _administrativeExpensesPerKg, value);
		}

		[Display(Name = "Список фильтров по группам товаров для ОХР")]
		public virtual IList<ProductGroup> AdministrativeProductGroupsFilter
		{
			get => _administrativeProductGroupsFilter;
			set => SetField(ref _administrativeProductGroupsFilter, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ProductGroup> ObservableAdministrativeProductGroupsFilter =>
			_observableAdministrativeProductGroupsFilter
				?? (_observableAdministrativeProductGroupsFilter = new GenericObservableList<ProductGroup>(AdministrativeProductGroupsFilter));
		
		[Display(Name = "Список фильтров по складам для ОХР")]
		public virtual IList<Warehouse> AdministrativeWarehousesFilter
		{
			get => _administrativeWarehousesFilter;
			set => SetField(ref _administrativeWarehousesFilter, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Warehouse> ObservableAdministrativeWarehousesFilter =>
			_observableAdministrativeWarehousesFilter
				?? (_observableAdministrativeWarehousesFilter = new GenericObservableList<Warehouse>(AdministrativeWarehousesFilter));

		#endregion

		#region Складские расходы

		[Display(Name = "Складские расходы за расчетный период")]
		public virtual int WarehouseExpenses
		{
			get => _warehouseExpenses;
			set => SetField(ref _warehouseExpenses, value);
		}

		[Display(Name = "Отгружено всего по складам")]
		public virtual int WarehousesTotalShipped
		{
			get => _warehousesTotalShipped;
			set => SetField(ref _warehousesTotalShipped, value);
		}

		[Display(Name = "Складские расходы на кг")]
		public virtual decimal WarehouseExpensesPerKg
		{
			get => _warehouseExpensesPerKg;
			set => SetField(ref _warehouseExpensesPerKg, value);
		}

		[Display(Name = "Список фильтров по группам товаров для складских расходов")]
		public virtual IList<ProductGroup> WarehouseExpensesProductGroupsFilter
		{
			get => _warehouseExpensesProductGroupsFilter;
			set => SetField(ref _warehouseExpensesProductGroupsFilter, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ProductGroup> ObservableWarehouseExpensesProductGroupsFilter =>
			_observableWarehouseExpensesProductGroupsFilter
				?? (_observableWarehouseExpensesProductGroupsFilter = new GenericObservableList<ProductGroup>(WarehouseExpensesProductGroupsFilter));

		[Display(Name = "Список фильтров по складам для складских расходов")]
		public virtual IList<Warehouse> WarehouseExpensesWarehousesFilter
		{
			get => _warehouseExpensesWarehousesFilter;
			set => SetField(ref _warehouseExpensesWarehousesFilter, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Warehouse> ObservableWarehouseExpensesWarehousesFilter =>
			_observableWarehouseExpensesWarehousesFilter
				?? (_observableWarehouseExpensesWarehousesFilter = new GenericObservableList<Warehouse>(WarehouseExpensesWarehousesFilter));

		#endregion

		#region Амортизация авто

		[Display(Name = "Падение стоимости газели компании за три года")]
		public virtual int DecreaseGazelleCostFor3Year
		{
			get => _decreaseGazelleCostFor3Year;
			set => SetField(ref _decreaseGazelleCostFor3Year, value);
		}

		[Display(Name = "Падение стоимости ларгуса компании за три года")]
		public virtual int DecreaseLargusCostFor3Year
		{
			get => _decreaseLargusCostFor3Year;
			set => SetField(ref _decreaseLargusCostFor3Year, value);
		}

		[Display(Name = "Падение стоимости Transit Mini компании за три года")]
		public virtual int DecreaseMinivanCostFor3Year
		{
			get => _decreaseMinivanCostFor3Year;
			set => SetField(ref _decreaseMinivanCostFor3Year, value);
		}

		[Display(Name = "Падение стоимости фуры компании за три года")]
		public virtual int DecreaseTruckCostFor3Year
		{
			get => _decreaseTruckCostFor3Year;
			set => SetField(ref _decreaseTruckCostFor3Year, value);
		}

		[Display(Name = "Средний пробег одной газели за расчетный период")]
		public virtual int GazelleAverageMileage
		{
			get => _gazelleAverageMileage;
			set => SetField(ref _gazelleAverageMileage, value);
		}

		[Display(Name = "Средний пробег одного ларгуса за расчетный период")]
		public virtual int LargusAverageMileage
		{
			get => _largusAverageMileage;
			set => SetField(ref _largusAverageMileage, value);
		}

		[Display(Name = "Средний пробег Transit Mini за расчетный период")]
		public virtual int MinivanAverageMileage
		{
			get => _minivanAverageMileage;
			set => SetField(ref _minivanAverageMileage, value);
		}

		[Display(Name = "Средний пробег одной фуры за расчетный период")]
		public virtual int TruckAverageMileage
		{
			get => _truckAverageMileage;
			set => SetField(ref _truckAverageMileage, value);
		}

		[Display(Name = "Амортизация газели на км")]
		public virtual decimal GazelleAmortisationPerKm
		{
			get => _gazelleAmortisationPerKm;
			set => SetField(ref _gazelleAmortisationPerKm, value);
		}

		[Display(Name = "Амортизация ларгуса на км")]
		public virtual decimal LargusAmortisationPerKm
		{
			get => _largusAmortisationPerKm;
			set => SetField(ref _largusAmortisationPerKm, value);
		}

		[Display(Name = "Амортизация Transit Mini на км")]
		public virtual decimal MinivanAmortisationPerKm
		{
			get => _minivanAmortisationPerKm;
			set => SetField(ref _minivanAmortisationPerKm, value);
		}

		[Display(Name = "Амортизация фуры на км")]
		public virtual decimal TruckAmortisationPerKm
		{
			get => _truckAmortisationPerKm;
			set => SetField(ref _truckAmortisationPerKm, value);
		}

		#endregion

		#region Стоимость ремонта авто

		[Display(Name = "Затраты при эксплуатации всех газелей за расчетный период")]
		public virtual int OperatingExpensesAllGazelles
		{
			get => _operatingExpensesAllGazelles;
			set => SetField(ref _operatingExpensesAllGazelles, value);
		}

		[Display(Name = "Затраты при эксплуатации всех ларгусов за расчетный период")]
		public virtual int OperatingExpensesAllLarguses
		{
			get => _operatingExpensesAllLarguses;
			set => SetField(ref _operatingExpensesAllLarguses, value);
		}

		[Display(Name = "Затраты при эксплуатации всех Transit Mini за расчетный период")]
		public virtual int OperatingExpensesAllMinivans
		{
			get => _operatingExpensesAllMinivans;
			set => SetField(ref _operatingExpensesAllMinivans, value);
		}

		[Display(Name = "Затраты при эксплуатации всех фур за расчетный период")]
		public virtual int OperatingExpensesAllTrucks
		{
			get => _operatingExpensesAllTrucks;
			set => SetField(ref _operatingExpensesAllTrucks, value);
		}

		[Display(Name = "Средний пробег всех газелей за расчетный период")]
		public virtual int AverageMileageAllGazelles
		{
			get => _averageMileageAllGazelles;
			set => SetField(ref _averageMileageAllGazelles, value);
		}

		[Display(Name = "Средний пробег всех ларгусов за расчетный период")]
		public virtual int AverageMileageAllLarguses
		{
			get => _averageMileageAllLarguses;
			set => SetField(ref _averageMileageAllLarguses, value);
		}

		[Display(Name = "Средний пробег всех Transit Mini за расчетный период")]
		public virtual int AverageMileageAllMinivans
		{
			get => _averageMileageAllMinivans;
			set => SetField(ref _averageMileageAllMinivans, value);
		}

		[Display(Name = "Средний пробег всех фур за расчетный период")]
		public virtual int AverageMileageAllTrucks
		{
			get => _averageMileageAllTrucks;
			set => SetField(ref _averageMileageAllTrucks, value);
		}

		[Display(Name = "Стоимость ремонта газели на км")]
		public virtual decimal GazelleRepairCostPerKm
		{
			get => _gazelleRepairCostPerKm;
			set => SetField(ref _gazelleRepairCostPerKm, value);
		}

		[Display(Name = "Стоимость ремонта ларгуса на км")]
		public virtual decimal LargusRepairCostPerKm
		{
			get => _largusRepairCostPerKm;
			set => SetField(ref _largusRepairCostPerKm, value);
		}

		[Display(Name = "Стоимость ремонта Transit Mini на км")]
		public virtual decimal MinivanRepairCostPerKm
		{
			get => _minivanRepairCostPerKm;
			set => SetField(ref _minivanRepairCostPerKm, value);
		}

		[Display(Name = "Стоимость ремонта фуры на км")]
		public virtual decimal TruckRepairCostPerKm
		{
			get => _truckRepairCostPerKm;
			set => SetField(ref _truckRepairCostPerKm, value);
		}

		[Display(Name = "Список фильтров по событиям ТС для стоимости ремонта")]
		public virtual IList<CarEventType> RepairCostCarEventTypesFilter
		{
			get => _repairCostCarEventTypeTypesFilter;
			set => SetField(ref _repairCostCarEventTypeTypesFilter, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CarEventType> ObservableRepairCostCarEventTypesFilter =>
			_observableRepairCostCarEventTypesFilter
				?? (_observableRepairCostCarEventTypesFilter = new GenericObservableList<CarEventType>(RepairCostCarEventTypesFilter));

		#endregion

		public virtual DateTime? CalculationSaved
		{
			get => _calculationSaved;
			set
			{
				if(SetField(ref _calculationSaved, value))
				{
					OnPropertyChanged(nameof(CalculationDateAndAuthor));
				}
			}
		}

		public virtual Employee CalculationAuthor
		{
			get => _calculationAuthor;
			set
			{
				if(SetField(ref _calculationAuthor, value))
				{
					OnPropertyChanged(nameof(CalculationDateAndAuthor));
				}
			}
		}

		public virtual string CalculationDateAndAuthor => $"{CalculationSaved:g} {CalculationAuthor?.GetPersonNameWithInitials()}";

		private static IEnumerable<CarOwnType> CarOwnTypes => new[] { CarOwnType.Company };

		public virtual void UpdateAdministartiveExpensesFilters(IEnumerable<int> admProductGroupsIds, IEnumerable<int> admWarehousesIds)
		{
			UpdateAdministrativeProductGroupsFilters(admProductGroupsIds);
			UpdateAdministrativeWarehousesFilters(admWarehousesIds);
		}
		
		public virtual void UpdateWarehouseExpensesFilters(IEnumerable<int> warProductGroupsIds, IEnumerable<int> warWarehousesIds)
		{
			UpdateWarehouseProductGroupsFilters(warProductGroupsIds);
			UpdateWarehouseWarehousesFilters(warWarehousesIds);
		}
		
		public virtual void UpdateCarEventTypesFilter(IEnumerable<int> carEventTypesIds)
		{
			RepairCostCarEventTypesFilter.Clear();

			foreach(var carEventTypesId in carEventTypesIds)
			{
				var carEventType = new CarEventType
				{
					Id = carEventTypesId
				};
				RepairCostCarEventTypesFilter.Add(carEventType);
			}
		}
		
		public virtual void CalculateAdministrativeExpensesPerKg(
			IUnitOfWork uow,
			IWarehouseRepository warehouseRepository,
			IEnumerable<int> productGroupsIds,
			IEnumerable<int> warehousesIds)
		{
			AdministrativeTotalShipped = warehouseRepository.GetTotalShippedKgByWarehousesAndProductGroups(
				uow,
				CalculatedMonth,
				CalculatedMonth.AddMonths(1),
				productGroupsIds,
				warehousesIds);
			
			AdministrativeExpensesPerKg = AdministrativeTotalShipped != default(int)
				? Math.Round((decimal)AdministrativeExpenses / AdministrativeTotalShipped, 2)
				: 0;
		}
		
		public virtual void CalculateWarehouseExpensesPerKg(
			IUnitOfWork uow,
			IWarehouseRepository warehouseRepository,
			IEnumerable<int> productGroupsIds,
			IEnumerable<int> warehousesIds)
		{
			WarehousesTotalShipped = warehouseRepository.GetTotalShippedKgByWarehousesAndProductGroups(
				uow,
				CalculatedMonth,
				CalculatedMonth.AddMonths(1),
				productGroupsIds,
				warehousesIds);
			WarehouseExpensesPerKg = WarehousesTotalShipped != default(int)
				? Math.Round((decimal)WarehouseExpenses / WarehousesTotalShipped, 2)
				: 0;
		}

		public virtual void CalculateAverageMileageForCarsByTypeOfUse(
			IUnitOfWork uow,
			IProfitabilityConstantsRepository profitabilityConstantsRepository)
		{
			var result = profitabilityConstantsRepository.GetAverageMileageCarsByTypeOfUse(uow, CalculatedMonth);

			foreach(var resultItem in result)
			{
				switch(resultItem.CarTypeOfUse)
				{
					case CarTypeOfUse.GAZelle:
						AverageMileageAllGazelles = (int)resultItem.Distance;
						GazelleAverageMileage = AverageMileageAllGazelles / resultItem.CountCars;
						continue;
					case CarTypeOfUse.Largus:
						AverageMileageAllLarguses = (int)resultItem.Distance;
						LargusAverageMileage = AverageMileageAllLarguses / resultItem.CountCars;
						continue;
					case CarTypeOfUse.Minivan:
						AverageMileageAllMinivans = (int)resultItem.Distance;
						MinivanAverageMileage = AverageMileageAllMinivans / resultItem.CountCars;
						continue;
					case CarTypeOfUse.Truck:
						AverageMileageAllTrucks = (int)resultItem.Distance;
						TruckAverageMileage = AverageMileageAllTrucks / resultItem.CountCars;
						continue;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public virtual void CalculateAmortisation()
		{
			GazelleAmortisationPerKm = GazelleAverageMileage != default
				? Math.Round((decimal)DecreaseGazelleCostFor3Year / _threeYearsInMonths / GazelleAverageMileage, 2)
				: 0;
			LargusAmortisationPerKm = LargusAverageMileage != default
				? Math.Round((decimal)DecreaseLargusCostFor3Year / _threeYearsInMonths / LargusAverageMileage, 2)
				: 0;
			MinivanAmortisationPerKm = MinivanAverageMileage != default
				? Math.Round((decimal)DecreaseMinivanCostFor3Year / _threeYearsInMonths / MinivanAverageMileage, 2)
				: 0;
			TruckAmortisationPerKm = TruckAverageMileage != default
				? Math.Round((decimal)DecreaseTruckCostFor3Year / _threeYearsInMonths / TruckAverageMileage, 2)
				: 0;
		}

		public virtual void CalculateOperatingExpenses(
			IUnitOfWork uow,
			ICarRepository carRepository,
			IEnumerable<int> carEventTypesIds)
		{
			CalculateGazellesExpenses(uow, carRepository, carEventTypesIds);
			CalculateLargusesExpenses(uow, carRepository, carEventTypesIds);
			CalculateMinivansExpenses(uow, carRepository, carEventTypesIds);
			CalculateTrucksExpenses(uow, carRepository, carEventTypesIds);
		}

		private void CalculateGazellesExpenses(IUnitOfWork uow, ICarRepository carRepository, IEnumerable<int> carEventTypesIds)
		{
			var companyGazellesCarEvents =
				carRepository.GetCarEventsForCostCarExploitation(
					uow,
					CalculatedMonth,
					CalculatedMonth.AddMonths(1).AddMilliseconds(-1),
					null,
					carEventTypesIds,
					new[] { CarTypeOfUse.GAZelle },
					CarOwnTypes);

			CalculateExpensesFromCarEvents(companyGazellesCarEvents, out var gazelleRepairCost, out var gazelleFines);
			OperatingExpensesAllGazelles = (int)(gazelleRepairCost - gazelleFines);
		}

		private void CalculateLargusesExpenses(IUnitOfWork uow, ICarRepository carRepository, IEnumerable<int> carEventTypesIds)
		{
			var companyLargusesCarEvents =
				carRepository.GetCarEventsForCostCarExploitation(
					uow,
					CalculatedMonth,
					CalculatedMonth.AddMonths(1).AddMilliseconds(-1),
					null,
					carEventTypesIds,
					new[] { CarTypeOfUse.Largus },
					CarOwnTypes);
			
			CalculateExpensesFromCarEvents(companyLargusesCarEvents, out var largusRepairCost, out var largusFines);
			OperatingExpensesAllLarguses = (int)(largusRepairCost - largusFines);
		}

		private void CalculateMinivansExpenses(IUnitOfWork uow, ICarRepository carRepository, IEnumerable<int> carEventTypesIds)
		{
			var companyMinivansCarEvents =
				carRepository.GetCarEventsForCostCarExploitation(
					uow,
					CalculatedMonth,
					CalculatedMonth.AddMonths(1).AddMilliseconds(-1),
					null,
					carEventTypesIds,
					new[] { CarTypeOfUse.Minivan },
					CarOwnTypes);
			
			CalculateExpensesFromCarEvents(companyMinivansCarEvents, out var minivanRepairCost, out var minivanFines);
			OperatingExpensesAllMinivans = (int)(minivanRepairCost - minivanFines);
		}
		
		private void CalculateTrucksExpenses(IUnitOfWork uow, ICarRepository carRepository, IEnumerable<int> carEventTypesIds)
		{
			var companyTrucksCarEvents =
				carRepository.GetCarEventsForCostCarExploitation(
					uow,
					CalculatedMonth,
					CalculatedMonth.AddMonths(1).AddMilliseconds(-1),
					null,
					carEventTypesIds,
					new[] { CarTypeOfUse.Truck },
					CarOwnTypes);

			CalculateExpensesFromCarEvents(companyTrucksCarEvents, out var truckRepairCost, out var truckFines);
			OperatingExpensesAllTrucks = (int)(truckRepairCost - truckFines);
		}

		public virtual void CalculateRepairCost()
		{
			GazelleRepairCostPerKm = AverageMileageAllGazelles != default
				? Math.Round((decimal)OperatingExpensesAllGazelles / AverageMileageAllGazelles, 2)
				: 0;
			LargusRepairCostPerKm = AverageMileageAllLarguses != default
				? Math.Round((decimal)OperatingExpensesAllLarguses / AverageMileageAllLarguses, 2)
				: 0;
			MinivanRepairCostPerKm = AverageMileageAllMinivans != default
				? Math.Round((decimal)OperatingExpensesAllMinivans / AverageMileageAllMinivans, 2)
				: 0;
			TruckRepairCostPerKm = AverageMileageAllTrucks != default
				? Math.Round((decimal)OperatingExpensesAllTrucks / AverageMileageAllTrucks, 2)
				: 0;
		}
		
		private void CalculateExpensesFromCarEvents(IEnumerable<CarEvent> companyCarEvents, out decimal carRepairCost, out decimal carFines)
		{
			carRepairCost = 0m;
			carFines = 0m;
			
			foreach(var companyCarEvent in companyCarEvents)
			{
				carRepairCost += companyCarEvent.RepairAndPartsSummaryCost;
				carFines += companyCarEvent.Fines.Sum(x => x.TotalMoney);
			}
		}

		private void UpdateAdministrativeProductGroupsFilters(IEnumerable<int> admProductGroupsIds)
		{
			AdministrativeProductGroupsFilter.Clear();

			foreach(var productGroupId in admProductGroupsIds)
			{
				var productGroup = new ProductGroup
				{
					Id = productGroupId
				};
				AdministrativeProductGroupsFilter.Add(productGroup);
			}
		}
		
		private void UpdateAdministrativeWarehousesFilters(IEnumerable<int> admWarehousesIds)
		{
			AdministrativeWarehousesFilter.Clear();
			
			foreach(var warehouseId in admWarehousesIds)
			{
				var productGroup = new Warehouse
				{
					Id = warehouseId
				};
				AdministrativeWarehousesFilter.Add(productGroup);
			}
		}
		
		private void UpdateWarehouseProductGroupsFilters(IEnumerable<int> warProductGroupsIds)
		{
			WarehouseExpensesProductGroupsFilter.Clear();

			foreach(var productGroupId in warProductGroupsIds)
			{
				var productGroup = new ProductGroup
				{
					Id = productGroupId
				};
				WarehouseExpensesProductGroupsFilter.Add(productGroup);
			}
		}
		
		private void UpdateWarehouseWarehousesFilters(IEnumerable<int> warWarehousesIds)
		{
			WarehouseExpensesWarehousesFilter.Clear();
			
			foreach(var warehouseId in warWarehousesIds)
			{
				var productGroup = new Warehouse
				{
					Id = warehouseId
				};
				WarehouseExpensesWarehousesFilter.Add(productGroup);
			}
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(AdministrativeExpenses == default)
			{
				var administrativeExpensesName = this.GetPropertyInfo(pc => pc.AdministrativeExpenses)
					.GetCustomAttribute<DisplayAttribute>(true).Name;
				yield return new ValidationResult(
					$"Необходимо заполнить { administrativeExpensesName }",
					new[] { nameof(AdministrativeExpenses) });
			}
			
			if(WarehouseExpenses == default)
			{
				var warehouseExpensesName = this.GetPropertyInfo(pc => pc.WarehouseExpenses)
					.GetCustomAttribute<DisplayAttribute>(true).Name;
				yield return new ValidationResult(
					$"Необходимо заполнить { warehouseExpensesName }",
					new[] { nameof(WarehouseExpenses) });
			}
			
			if(DecreaseGazelleCostFor3Year == default)
			{
				var decreaseGazelleCostFor3YearName = this.GetPropertyInfo(pc => pc.DecreaseGazelleCostFor3Year)
					.GetCustomAttribute<DisplayAttribute>(true).Name;
				yield return new ValidationResult(
					$"Необходимо заполнить { decreaseGazelleCostFor3YearName }",
					new[] { nameof(DecreaseGazelleCostFor3Year) });
			}
			
			if(DecreaseLargusCostFor3Year == default)
			{
				var decreaseLargusCostFor3YearName = this.GetPropertyInfo(pc => pc.DecreaseLargusCostFor3Year)
					.GetCustomAttribute<DisplayAttribute>(true).Name;
				yield return new ValidationResult(
					$"Необходимо заполнить { decreaseLargusCostFor3YearName }",
					new[] { nameof(DecreaseLargusCostFor3Year) });
			}
			
			if(DecreaseMinivanCostFor3Year == default)
			{
				var decreaseMinivanCostFor3YearName = this.GetPropertyInfo(pc => pc.DecreaseMinivanCostFor3Year)
					.GetCustomAttribute<DisplayAttribute>(true).Name;
				yield return new ValidationResult(
					$"Необходимо заполнить { decreaseMinivanCostFor3YearName }",
					new[] { nameof(DecreaseMinivanCostFor3Year) });
			}
			
			if(DecreaseTruckCostFor3Year == default)
			{
				var decreaseTruckCostFor3YearName = this.GetPropertyInfo(pc => pc.DecreaseTruckCostFor3Year)
					.GetCustomAttribute<DisplayAttribute>(true).Name;
				yield return new ValidationResult(
					$"Необходимо заполнить { decreaseTruckCostFor3YearName }",
					new[] { nameof(DecreaseTruckCostFor3Year) });
			}
		}
	}
}
