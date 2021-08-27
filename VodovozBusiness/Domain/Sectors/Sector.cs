using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Domain.Sectors
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "районы",
		Nominative = "район")]
	[EntityPermission]
	[HistoryTrace]
	public class Sector : BusinessObjectBase<Sector>, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		
		private DateTime dateCreated;
		[Display(Name = "Время создания")]
		public virtual DateTime DateCreated {
			get => dateCreated;
			set => SetField(ref dateCreated, value);
		}

		private IList<SectorVersion> _sectorVersions = new List<SectorVersion>();

		public virtual IList<SectorVersion> SectorVersions
		{
			get => _sectorVersions;
			set => SetField(ref _sectorVersions, value);
		}

		private GenericObservableList<SectorVersion> _observableSectorVersions;

		public virtual GenericObservableList<SectorVersion> ObservableSectorVersions => _observableSectorVersions ??
		                                                                               (_observableSectorVersions = new GenericObservableList<SectorVersion>(SectorVersions));
		
		private IList<SectorDeliveryRuleVersion> _sectorDeliveryRuleVersions = new List<SectorDeliveryRuleVersion>();

		public virtual IList<SectorDeliveryRuleVersion> SectorDeliveryRuleVersions
		{
			get => _sectorDeliveryRuleVersions;
			set => SetField(ref _sectorDeliveryRuleVersions, value);
		}

		private GenericObservableList<SectorDeliveryRuleVersion> _observableSectorDeliveryRuleVersions;

		public virtual GenericObservableList<SectorDeliveryRuleVersion> ObservableSectorDeliveryRuleVersions =>
			_observableSectorDeliveryRuleVersions ??
			(_observableSectorDeliveryRuleVersions = new GenericObservableList<SectorDeliveryRuleVersion>(SectorDeliveryRuleVersions));
		
		private IList<SectorWeekDayScheduleVersion> _sectorWeekDaySchedulesVersions = new List<SectorWeekDayScheduleVersion>();

		public virtual IList<SectorWeekDayScheduleVersion> SectorWeekDaySchedulesVersions
		{
			get => _sectorWeekDaySchedulesVersions;
			set => SetField(ref _sectorWeekDaySchedulesVersions, value);
		}

		private GenericObservableList<SectorWeekDayScheduleVersion> _observableSectorWeekDayRulesVersions;

		public virtual GenericObservableList<SectorWeekDayScheduleVersion> ObservableSectorWeekDayScheduleVersions =>
			_observableSectorWeekDayRulesVersions ??
			(_observableSectorWeekDayRulesVersions = new GenericObservableList<SectorWeekDayScheduleVersion>(SectorWeekDaySchedulesVersions));
		
		private IList<SectorWeekDayDeliveryRuleVersion> _sectorWeekDayDeliveryRuleVersions = new List<SectorWeekDayDeliveryRuleVersion>();

		public virtual IList<SectorWeekDayDeliveryRuleVersion> SectorWeekDayDeliveryRuleVersions
		{
			get => _sectorWeekDayDeliveryRuleVersions;
			set => SetField(ref _sectorWeekDayDeliveryRuleVersions, value);
		}

		private GenericObservableList<SectorWeekDayDeliveryRuleVersion> _observableSectorWeekDayDeliveryRules;

		public virtual GenericObservableList<SectorWeekDayDeliveryRuleVersion> ObservableSectorWeekDayDeliveryRuleVersions =>
			_observableSectorWeekDayDeliveryRules ??
			(_observableSectorWeekDayDeliveryRules = new GenericObservableList<SectorWeekDayDeliveryRuleVersion>(SectorWeekDayDeliveryRuleVersions));
		
		private IList<DeliveryPointSectorVersion> _deliveryPointSectorVersions = new List<DeliveryPointSectorVersion>();

		public virtual IList<DeliveryPointSectorVersion> DeliveryPointSectorVersions
		{
			get => _deliveryPointSectorVersions;
			set => SetField(ref _deliveryPointSectorVersions, value);
		}

		private GenericObservableList<DeliveryPointSectorVersion> _observableDeliveryPointSectorVersions;

		public virtual GenericObservableList<DeliveryPointSectorVersion> ObservableDeliveryPointSectorVersions =>
			_observableDeliveryPointSectorVersions ??
			(_observableDeliveryPointSectorVersions = new GenericObservableList<DeliveryPointSectorVersion>(DeliveryPointSectorVersions));
		
		public virtual SectorVersion GetActiveSectorVersion(DateTime? activationTime = null)
		{
			if(activationTime.HasValue)
				return ObservableSectorVersions.SingleOrDefault(x =>
					x.Status == SectorsSetStatus.Active && x.StartDate <= activationTime &&
					(x.EndDate == null || x.EndDate <= activationTime.Value.Date.AddDays(1)));
			return ObservableSectorVersions.SingleOrDefault(x =>
					x.Status == SectorsSetStatus.Active && x.StartDate <= DateTime.Today.Date &&
					(x.EndDate == null || x.EndDate <= DateTime.Now.Date.AddDays(1)));
		}
		
		private SectorDeliveryRuleVersion _activeDeliveryRuleVersion;

		public virtual SectorDeliveryRuleVersion GetActiveDeliveryRuleVersion(DateTime? activationTime = null)
		{
			if(activationTime.HasValue)
				return ObservableSectorDeliveryRuleVersions.SingleOrDefault(x =>
					x.StartDate <= activationTime && (x.EndDate == null || x.EndDate <= activationTime?.Date.AddDays(1)));
			return ObservableSectorDeliveryRuleVersions.SingleOrDefault(x =>
				x.StartDate <= DateTime.Now.Date && (x.EndDate == null || x.EndDate <= DateTime.Now.Date.AddDays(1)));
		}
		
		private SectorWeekDayScheduleVersion _activeWeekDayScheduleVersion;

		public virtual SectorWeekDayScheduleVersion GetActiveWeekDayScheduleVersion(DateTime? activationTime = null)
		{
			if(activationTime.HasValue)
				return ObservableSectorWeekDayScheduleVersions.SingleOrDefault(x =>
					x.StartDate <= activationTime && (x.EndDate == null || x.EndDate <= activationTime?.Date.AddDays(1)));
			return ObservableSectorWeekDayScheduleVersions.SingleOrDefault(x =>
				x.StartDate <= DateTime.Now.Date && (x.EndDate == null || x.EndDate <= DateTime.Now.Date.AddDays(1)));
		}
		
		private SectorWeekDayDeliveryRuleVersion _activeWeekDayDeliveryRuleVersion;

		public virtual SectorWeekDayDeliveryRuleVersion GetActiveWeekDayDeliveryRuleVersion(DateTime? activationTime = null)
		{
			if(activationTime.HasValue)
				return ObservableSectorWeekDayDeliveryRuleVersions.SingleOrDefault(x =>
					x.StartDate <= activationTime && (x.EndDate == null || x.EndDate <= activationTime?.Date.AddDays(1)));
			return ObservableSectorWeekDayDeliveryRuleVersions.SingleOrDefault(x =>
				x.StartDate <= DateTime.Now.Date && (x.EndDate == null || x.EndDate <= DateTime.Now.Date.AddDays(1)));
		}
		
		private DeliveryPointSectorVersion _activeDeliveryPointVersion;

		public virtual DeliveryPointSectorVersion ActiveDeliveryPointVersion(DateTime? activationTime = null)
		{
			if(activationTime.HasValue)
				return ObservableDeliveryPointSectorVersions.SingleOrDefault(x =>
					x.StartDate <= activationTime && (x.EndDate == null || x.EndDate <= activationTime?.Date.AddDays(1)));
			return ObservableDeliveryPointSectorVersions.SingleOrDefault(x =>
				x.StartDate <= DateTime.Now.Date && (x.EndDate == null || x.EndDate <= DateTime.Now.Date.AddDays(1)));
		}
		#endregion


		/// <summary>
		///	Приоритет от максимального:
		///	1) Правила доставки на сегодня
		/// 2) Правила доставки на текущий день недели
		/// 3) Правила доставки района
		/// </summary>
		public virtual decimal GetDeliveryPrice(OrderStateKey orderStateKey, decimal eShopGoodsSum, DateTime? startDate)
		{
			if(orderStateKey.Order.DeliveryDate.HasValue)
			{
				if(orderStateKey.Order.DeliveryDate.Value.Date == DateTime.Today 
				   && (GetActiveWeekDayScheduleVersion(startDate).SectorSchedules.Any(x=>x.WeekDay == WeekDayName.Today) 
				       || GetActiveWeekDayDeliveryRuleVersion(startDate).WeekDayDistrictRules.Any(y=>y.WeekDay == WeekDayName.Today))) {
					var todayDeliveryRules = GetActiveWeekDayDeliveryRuleVersion(startDate).WeekDayDistrictRules.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();
					
					if (todayDeliveryRules.Any())
					{
						var todayMinEShopGoodsSum =
							todayDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);
						
						if(eShopGoodsSum < todayMinEShopGoodsSum || todayMinEShopGoodsSum == 0)
							return todayDeliveryRules.Max(x => x.Price);
					}
					return 0m;
				}
				var dayOfWeekRules = GetActiveWeekDayDeliveryRuleVersion(startDate).WeekDayDistrictRules.Where(x=> x.WeekDay == ConvertDayOfWeekToWeekDayName(orderStateKey.Order.DeliveryDate.Value.DayOfWeek));
				if(dayOfWeekRules.Any()) {
					var dayOfWeekDeliveryRules = dayOfWeekRules.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();
					
					if (dayOfWeekDeliveryRules.Any())
					{
						var dayOfWeekEShopGoodsSum =
							dayOfWeekDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);
						
						if(eShopGoodsSum < dayOfWeekEShopGoodsSum || dayOfWeekEShopGoodsSum == 0)
							return dayOfWeekDeliveryRules.Max(x => x.Price);
					}
					
					return 0m;
				}
			}
			var commonDeliveryRules = 
				GetActiveDeliveryRuleVersion(startDate).ObservableCommonDistrictRuleItems.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();
			
			if (commonDeliveryRules.Any())
			{
				var commonMinEShopGoodsSum = commonDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);
				
				if(eShopGoodsSum < commonMinEShopGoodsSum || commonMinEShopGoodsSum == 0)
					return commonDeliveryRules.Max(x => x.Price);
			}
					
			return 0m;
		}
		
		public static WeekDayName ConvertDayOfWeekToWeekDayName(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek) {
				case DayOfWeek.Monday: return WeekDayName.Monday;
				case DayOfWeek.Tuesday: return WeekDayName.Tuesday;
				case DayOfWeek.Wednesday: return WeekDayName.Wednesday;
				case DayOfWeek.Thursday: return WeekDayName.Thursday;
				case DayOfWeek.Friday: return WeekDayName.Friday;
				case DayOfWeek.Saturday: return WeekDayName.Saturday;
				case DayOfWeek.Sunday: return WeekDayName.Sunday;
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}
	
	public enum SectorsSetStatus
	{
		[Display(Name = "Черновик")]
		Draft,
		[Display(Name = "На активации")]
		OnActivation,
		[Display(Name = "Активна")]
		Active,
		[Display(Name = "Закрыта")]
		Closed
	}
	public class SectorsSetStatusStringType : NHibernate.Type.EnumStringType {
		public SectorsSetStatusStringType() : base(typeof(SectorsSetStatus)) { }
	}
}
