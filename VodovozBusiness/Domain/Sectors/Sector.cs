using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
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
	public class Sector : BusinessObjectBase<Sector>, IDomainObject, IValidatableObject, ICloneable
	{
		#region Свойства
		public virtual int Id { get; set; }

		string _sectorName;
		[Display(Name = "Название района")]
		public virtual string SectorName {
			get => _sectorName;
			set => SetField(ref _sectorName, value);
		}
		private DateTime dateCreated;
		[Display(Name = "Время создания")]
		public virtual DateTime DateCreated {
			get => dateCreated;
			set => SetField(ref dateCreated, value);
		}

		private List<SectorVersion> _sectorVersions;

		public virtual List<SectorVersion> SectorVersions
		{
			get => _sectorVersions;
			set => SetField(ref _sectorVersions, value);
		}

		private GenericObservableList<SectorVersion> _observableSectorVersions;

		public virtual GenericObservableList<SectorVersion> ObservableSectorVersions => _observableSectorVersions ??
		                                                                               (_observableSectorVersions = new GenericObservableList<SectorVersion>(SectorVersions));
		
		private List<SectorDeliveryRuleVersion> _sectorDeliveryRuleVersions;

		public virtual List<SectorDeliveryRuleVersion> SectorDeliveryRuleVersions
		{
			get => _sectorDeliveryRuleVersions;
			set => SetField(ref _sectorDeliveryRuleVersions, value);
		}

		private GenericObservableList<SectorDeliveryRuleVersion> _observableSectorDeliveryRuleVersions;

		public virtual GenericObservableList<SectorDeliveryRuleVersion> ObservableSectorDeliveryRuleVersions =>
			_observableSectorDeliveryRuleVersions ??
			(_observableSectorDeliveryRuleVersions = new GenericObservableList<SectorDeliveryRuleVersion>(SectorDeliveryRuleVersions));
		
		private List<SectorWeekDayRulesVersion> _sectorWeekDayRulesVersions;

		public virtual List<SectorWeekDayRulesVersion> SectorWeekDayRulesVersions
		{
			get => _sectorWeekDayRulesVersions;
			set => SetField(ref _sectorWeekDayRulesVersions, value);
		}

		private GenericObservableList<SectorWeekDayRulesVersion> _observableSectorWeekDayRulesVersions;

		public virtual GenericObservableList<SectorWeekDayRulesVersion> ObservableSectorWeekDayRulesVersions =>
			_observableSectorWeekDayRulesVersions ??
			(_observableSectorWeekDayRulesVersions = new GenericObservableList<SectorWeekDayRulesVersion>(SectorWeekDayRulesVersions));
		
		private List<DeliveryPointSectorVersion> _deliveryPointSectorVersions;

		public virtual List<DeliveryPointSectorVersion> DeliveryPointSectorVersions
		{
			get => _deliveryPointSectorVersions;
			set => SetField(ref _deliveryPointSectorVersions, value);
		}

		private GenericObservableList<DeliveryPointSectorVersion> _observableDeliveryPointSectorVersions;

		public virtual GenericObservableList<DeliveryPointSectorVersion> ObservableDeliveryPointSectorVersions =>
			_observableDeliveryPointSectorVersions ??
			(_observableDeliveryPointSectorVersions = new GenericObservableList<DeliveryPointSectorVersion>(DeliveryPointSectorVersions));

		private SectorVersion _activeSectorVersion;

		public virtual SectorVersion ActiveSectorVersion
		{
			get => _activeSectorVersion;
			set
			{
				var active = ObservableSectorVersions.Single(x => x.Status == SectorsSetStatus.Active);
				if(active != null)
				{
					_activeSectorVersion = active;
				}
				else
				{
					_activeSectorVersion = null;
				}
			}
		}
		
		private SectorDeliveryRuleVersion _activeDeliveryRuleVersion;

		public virtual SectorDeliveryRuleVersion ActiveDeliveryRuleVersion
		{
			get => _activeDeliveryRuleVersion;
			set
			{
				var active = ObservableSectorDeliveryRuleVersions.Single(x => x.Status == SectorsSetStatus.Active);
				if(active != null)
				{
					_activeDeliveryRuleVersion = active;
				}
				else
				{
					_activeDeliveryRuleVersion = null;
				}
			}
		}
		
		private SectorWeekDayRulesVersion _activeWeekDayRulesVersion;

		public virtual SectorWeekDayRulesVersion ActiveWeekDayRulesVersion
		{
			get => _activeWeekDayRulesVersion;
			set
			{
				var active = ObservableSectorWeekDayRulesVersions.Single(x => x.Status == SectorsSetStatus.Active);
				if(active != null)
				{
					_activeWeekDayRulesVersion = active;
				}
				else
				{
					_activeWeekDayRulesVersion = null;
				}
			}
		}
		
		private DeliveryPointSectorVersion _activeDeliveryPointVersion;

		public virtual DeliveryPointSectorVersion ActiveDeliveryPointVersion
		{
			get => _activeDeliveryPointVersion;
			set
			{
				var active = ObservableDeliveryPointSectorVersions.Single(x => x.Status == SectorsSetStatus.Active);
				if(active != null)
				{
					_activeDeliveryPointVersion = active;
				}
				else
				{
					_activeDeliveryPointVersion = null;
				}
			}
		}
		#endregion

		#region IValidatableObject implementation
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrWhiteSpace(SectorName)) {
				yield return new ValidationResult(
					"Необходимо заполнить имя района",
					new[] { this.GetPropertyName(o => o.SectorName) }
				);
			}
		}
		
		#endregion

		#region ICloneable implementation

		public virtual object Clone()
		{
			return new Sector {
				SectorName = SectorName
			};
		}
		#endregion
		
		
		/// <summary>
		///	Приоритет от максимального:
		///	1) Правила доставки на сегодня
		/// 2) Правила доставки на текущий день недели
		/// 3) Правила доставки района
		/// </summary>
		public virtual decimal GetDeliveryPrice(OrderStateKey orderStateKey, decimal eShopGoodsSum)
		{
			if(orderStateKey.Order.DeliveryDate.HasValue)
			{
				if(orderStateKey.Order.DeliveryDate.Value.Date == DateTime.Today 
				   && (ActiveWeekDayRulesVersion.SectorSchedules.Any(x=>x.DeliveryWeekDay == WeekDayName.Today) 
				       || ActiveWeekDayRulesVersion.SectorDeliveryRules.Any(y=>y.DeliveryWeekDay == WeekDayName.Today))) {
					var todayDeliveryRules = ActiveWeekDayRulesVersion.SectorDeliveryRules.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();
					
					if (todayDeliveryRules.Any())
					{
						var todayMinEShopGoodsSum =
							todayDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);
						
						if(eShopGoodsSum < todayMinEShopGoodsSum || todayMinEShopGoodsSum == 0)
							return todayDeliveryRules.Max(x => x.Price);
					}
					return 0m;
				}
				var dayOfWeekRules = ActiveWeekDayRulesVersion.SectorDeliveryRules.Where(x=> x.DeliveryWeekDay == ConvertDayOfWeekToWeekDayName(orderStateKey.Order.DeliveryDate.Value.DayOfWeek));
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
				ActiveDeliveryRuleVersion.ObservableCommonDistrictRuleItems.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();
			
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
