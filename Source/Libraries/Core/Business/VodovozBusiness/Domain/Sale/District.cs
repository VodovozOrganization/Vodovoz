using Gamma.Utilities;
using NetTopologySuite.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "районы",
		Nominative = "район")]
	[EntityPermission]
	[HistoryTrace]
	public class District : BusinessObjectBase<District>, IDomainObject, IValidatableObject, ICloneable
	{
		#region Fields
		string _districtName;
		private TariffZone _tariffZone;
		private Geometry _districtBorder;
		private int _minBottles;
		private decimal _waterPrice;
		private DistrictWaterPrice _priceType;
		private WageDistrict _wageDistrict;
		private GeoGroup _geographicGroup;
		private DistrictsSet _districtsSet;
		private District _copyOf;
		private IList<DistrictCopyItem> _districtCopyItems = new List<DistrictCopyItem>();
		private IList<CommonDistrictRuleItem> _commonDistrictRuleItems = new List<CommonDistrictRuleItem>();

		private IList<WeekDayDistrictRuleItem> _todayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		private GenericObservableList<WeekDayDistrictRuleItem> _observableTodayDistrictRuleItems;
		private IList<WeekDayDistrictRuleItem> _mondayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		private GenericObservableList<WeekDayDistrictRuleItem> _observableMondayDistrictRuleItems;
		private IList<WeekDayDistrictRuleItem> _tuesdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		private GenericObservableList<WeekDayDistrictRuleItem> _observableTuesdayDistrictRuleItems;
		private IList<WeekDayDistrictRuleItem> _wednesdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		private GenericObservableList<WeekDayDistrictRuleItem> _observableWednesdayDistrictRuleItems;
		private IList<WeekDayDistrictRuleItem> _thursdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		private GenericObservableList<WeekDayDistrictRuleItem> _observableThursdayDistrictRuleItems;
		private IList<WeekDayDistrictRuleItem> _fridayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		private GenericObservableList<WeekDayDistrictRuleItem> _observableFridayDistrictRuleItems;
		private IList<WeekDayDistrictRuleItem> _saturdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		private GenericObservableList<WeekDayDistrictRuleItem> _observableSaturdayDistrictRuleItems;
		private IList<WeekDayDistrictRuleItem> _sundayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		private GenericObservableList<WeekDayDistrictRuleItem> _observableSundayDistrictRuleItems;

		private IList<DeliveryScheduleRestriction> _todayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		private GenericObservableList<DeliveryScheduleRestriction> _observableTodayDeliveryScheduleRestrictions;
		private IList<DeliveryScheduleRestriction> _mondayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		private GenericObservableList<DeliveryScheduleRestriction> _observableMondayDeliveryScheduleRestrictions;
		private IList<DeliveryScheduleRestriction> _tuesdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		private GenericObservableList<DeliveryScheduleRestriction> _observableTuesdayDeliveryScheduleRestrictions;
		private IList<DeliveryScheduleRestriction> _wednesdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		private GenericObservableList<DeliveryScheduleRestriction> _observableWednesdayDeliveryScheduleRestrictions;
		private IList<DeliveryScheduleRestriction> _thursdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		private GenericObservableList<DeliveryScheduleRestriction> _observableThursdayDeliveryScheduleRestrictions;
		private IList<DeliveryScheduleRestriction> _fridayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		private GenericObservableList<DeliveryScheduleRestriction> _observableFridayDeliveryScheduleRestrictions;
		private IList<DeliveryScheduleRestriction> _saturdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		private GenericObservableList<DeliveryScheduleRestriction> _observableSaturdayDeliveryScheduleRestrictions;
		private IList<DeliveryScheduleRestriction> _sundayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		private GenericObservableList<DeliveryScheduleRestriction> _observableSundayDeliveryScheduleRestrictions;
		#endregion Fields

		#region Свойства
		public virtual int Id { get; set; }

		[Display(Name = "Название района")]
		public virtual string DistrictName
		{
			get => _districtName;
			set => SetField(ref _districtName, value);
		}

		[Display(Name = "Тарифная зоны")]
		public virtual TariffZone TariffZone
		{
			get => _tariffZone;
			set => SetField(ref _tariffZone, value);
		}

		[Display(Name = "Граница")]
		public virtual Geometry DistrictBorder
		{
			get => _districtBorder;
			set => SetField(ref _districtBorder, value);
		}

		[Display(Name = "Минимальное количество бутылей")]
		public virtual int MinBottles
		{
			get => _minBottles;
			set => SetField(ref _minBottles, value);
		}

		[Display(Name = "Цена на воду")]
		public virtual decimal WaterPrice
		{
			get => _waterPrice;
			set => SetField(ref _waterPrice, value);
		}

		[Display(Name = "Вид цены")]
		public virtual DistrictWaterPrice PriceType
		{
			get => _priceType;
			set
			{
				SetField(ref _priceType, value);
				if(WaterPrice != 0 && PriceType != DistrictWaterPrice.FixForDistrict)
					WaterPrice = 0;
			}
		}

		[Display(Name = "Группа района для расчёта ЗП")]
		public virtual WageDistrict WageDistrict
		{
			get => _wageDistrict;
			set => SetField(ref _wageDistrict, value);
		}

		[Display(Name = "Часть города")]
		public virtual GeoGroup GeographicGroup
		{
			get => _geographicGroup;
			set => SetField(ref _geographicGroup, value);
		}

		[Display(Name = "Версия районов")]
		public virtual DistrictsSet DistrictsSet
		{
			get => _districtsSet;
			set => SetField(ref _districtsSet, value);
		}

		[Display(Name = "Копия района")]
		public virtual District CopyOf
		{
			get => _copyOf;
			set => SetField(ref _copyOf, value);
		}

		[Display(Name = "Районы, в которые был скопирован данный район")]
		public virtual IList<DistrictCopyItem> DistrictCopyItems
		{
			get => _districtCopyItems;
			set => SetField(ref _districtCopyItems, value);
		}
		#endregion

		#region CommonDistrictRuleItems

		[Display(Name = "Правила и цены доставки района")]
		public virtual IList<CommonDistrictRuleItem> CommonDistrictRuleItems
		{
			get => _commonDistrictRuleItems;
			set => SetField(ref _commonDistrictRuleItems, value, () => CommonDistrictRuleItems);
		}

		private GenericObservableList<CommonDistrictRuleItem> _observableCommonDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CommonDistrictRuleItem> ObservableCommonDistrictRuleItems =>
			_observableCommonDistrictRuleItems ?? (_observableCommonDistrictRuleItems =
				new GenericObservableList<CommonDistrictRuleItem>(CommonDistrictRuleItems));

		#endregion

		#region WeekDayDistricRuleItems

		public virtual IList<WeekDayDistrictRuleItem> TodayDistrictRuleItems
		{
			get => _todayDistrictRuleItems;
			set => SetField(ref _todayDistrictRuleItems, value, () => TodayDistrictRuleItems);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableTodayDistrictRuleItems =>
			_observableTodayDistrictRuleItems ?? (_observableTodayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(TodayDistrictRuleItems));

		public virtual IList<WeekDayDistrictRuleItem> MondayDistrictRuleItems
		{
			get => _mondayDistrictRuleItems;
			set => SetField(ref _mondayDistrictRuleItems, value, () => MondayDistrictRuleItems);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableMondayDistrictRuleItems =>
			_observableMondayDistrictRuleItems ?? (_observableMondayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(MondayDistrictRuleItems));

		public virtual IList<WeekDayDistrictRuleItem> TuesdayDistrictRuleItems
		{
			get => _tuesdayDistrictRuleItems;
			set => SetField(ref _tuesdayDistrictRuleItems, value, () => TuesdayDistrictRuleItems);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableTuesdayDistrictRuleItems =>
			_observableTuesdayDistrictRuleItems ?? (_observableTuesdayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(TuesdayDistrictRuleItems));

		public virtual IList<WeekDayDistrictRuleItem> WednesdayDistrictRuleItems
		{
			get => _wednesdayDistrictRuleItems;
			set => SetField(ref _wednesdayDistrictRuleItems, value, () => WednesdayDistrictRuleItems);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableWednesdayDistrictRuleItems =>
			_observableWednesdayDistrictRuleItems ?? (_observableWednesdayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(WednesdayDistrictRuleItems));

		public virtual IList<WeekDayDistrictRuleItem> ThursdayDistrictRuleItems
		{
			get => _thursdayDistrictRuleItems;
			set => SetField(ref _thursdayDistrictRuleItems, value, () => ThursdayDistrictRuleItems);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableThursdayDistrictRuleItems =>
			_observableThursdayDistrictRuleItems ?? (_observableThursdayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(ThursdayDistrictRuleItems));

		public virtual IList<WeekDayDistrictRuleItem> FridayDistrictRuleItems
		{
			get => _fridayDistrictRuleItems;
			set => SetField(ref _fridayDistrictRuleItems, value, () => FridayDistrictRuleItems);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableFridayDistrictRuleItems =>
			_observableFridayDistrictRuleItems ?? (_observableFridayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(FridayDistrictRuleItems));

		public virtual IList<WeekDayDistrictRuleItem> SaturdayDistrictRuleItems
		{
			get => _saturdayDistrictRuleItems;
			set => SetField(ref _saturdayDistrictRuleItems, value, () => SaturdayDistrictRuleItems);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableSaturdayDistrictRuleItems =>
			_observableSaturdayDistrictRuleItems ?? (_observableSaturdayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(SaturdayDistrictRuleItems));

		public virtual IList<WeekDayDistrictRuleItem> SundayDistrictRuleItems
		{
			get => _sundayDistrictRuleItems;
			set => SetField(ref _sundayDistrictRuleItems, value, () => SundayDistrictRuleItems);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableSundayDistrictRuleItems =>
			_observableSundayDistrictRuleItems ?? (_observableSundayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(SundayDistrictRuleItems));

		#endregion

		#region DeliveryScheduleRestrictions

		public virtual IList<DeliveryScheduleRestriction> TodayDeliveryScheduleRestrictions
		{
			get => _todayDeliveryScheduleRestrictions;
			set => SetField(ref _todayDeliveryScheduleRestrictions, value, () => TodayDeliveryScheduleRestrictions);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableTodayDeliveryScheduleRestrictions =>
			_observableTodayDeliveryScheduleRestrictions ?? (_observableTodayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(TodayDeliveryScheduleRestrictions));

		public virtual IList<DeliveryScheduleRestriction> MondayDeliveryScheduleRestrictions
		{
			get => _mondayDeliveryScheduleRestrictions;
			set => SetField(ref _mondayDeliveryScheduleRestrictions, value, () => MondayDeliveryScheduleRestrictions);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableMondayDeliveryScheduleRestrictions =>
			_observableMondayDeliveryScheduleRestrictions ?? (_observableMondayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(MondayDeliveryScheduleRestrictions));

		public virtual IList<DeliveryScheduleRestriction> TuesdayDeliveryScheduleRestrictions
		{
			get => _tuesdayDeliveryScheduleRestrictions;
			set => SetField(ref _tuesdayDeliveryScheduleRestrictions, value, () => TuesdayDeliveryScheduleRestrictions);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableTuesdayDeliveryScheduleRestrictions =>
			_observableTuesdayDeliveryScheduleRestrictions ?? (_observableTuesdayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(TuesdayDeliveryScheduleRestrictions));

		public virtual IList<DeliveryScheduleRestriction> WednesdayDeliveryScheduleRestrictions
		{
			get => _wednesdayDeliveryScheduleRestrictions;
			set => SetField(ref _wednesdayDeliveryScheduleRestrictions, value, () => WednesdayDeliveryScheduleRestrictions);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableWednesdayDeliveryScheduleRestrictions =>
			_observableWednesdayDeliveryScheduleRestrictions ?? (_observableWednesdayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(WednesdayDeliveryScheduleRestrictions));

		public virtual IList<DeliveryScheduleRestriction> ThursdayDeliveryScheduleRestrictions
		{
			get => _thursdayDeliveryScheduleRestrictions;
			set => SetField(ref _thursdayDeliveryScheduleRestrictions, value, () => ThursdayDeliveryScheduleRestrictions);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableThursdayDeliveryScheduleRestrictions =>
			_observableThursdayDeliveryScheduleRestrictions ?? (_observableThursdayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(ThursdayDeliveryScheduleRestrictions));

		public virtual IList<DeliveryScheduleRestriction> FridayDeliveryScheduleRestrictions
		{
			get => _fridayDeliveryScheduleRestrictions;
			set => SetField(ref _fridayDeliveryScheduleRestrictions, value, () => FridayDeliveryScheduleRestrictions);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableFridayDeliveryScheduleRestrictions =>
			_observableFridayDeliveryScheduleRestrictions ?? (_observableFridayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(FridayDeliveryScheduleRestrictions));

		public virtual IList<DeliveryScheduleRestriction> SaturdayDeliveryScheduleRestrictions
		{
			get => _saturdayDeliveryScheduleRestrictions;
			set => SetField(ref _saturdayDeliveryScheduleRestrictions, value, () => SaturdayDeliveryScheduleRestrictions);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableSaturdayDeliveryScheduleRestrictions =>
			_observableSaturdayDeliveryScheduleRestrictions ?? (_observableSaturdayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(SaturdayDeliveryScheduleRestrictions));

		public virtual IList<DeliveryScheduleRestriction> SundayDeliveryScheduleRestrictions
		{
			get => _sundayDeliveryScheduleRestrictions;
			set => SetField(ref _sundayDeliveryScheduleRestrictions, value, () => SundayDeliveryScheduleRestrictions);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableSundayDeliveryScheduleRestrictions =>
			_observableSundayDeliveryScheduleRestrictions ?? (_observableSundayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(SundayDeliveryScheduleRestrictions));

		#endregion

		#region Функции

		public virtual string Title => DistrictName;

		public virtual bool HaveRestrictions => GetAllDeliveryScheduleRestrictions().Any();

		public virtual string GetSchedulesString(bool withMarkup = false)
		{
			var result = new StringBuilder();
			foreach(var deliveryScheduleRestriction in GetAllDeliveryScheduleRestrictions().GroupBy(x => x.WeekDay).OrderBy(x => (int)x.Key))
			{
				var weekName = deliveryScheduleRestriction.Key.GetEnumTitle();
				result.Append(withMarkup ? $"<u><b>{weekName}</b></u>" : weekName);
				var weekRules = GetWeekDayRuleItemCollectionByWeekDayName(deliveryScheduleRestriction.Key);
				if(weekRules.Any())
				{
					result.AppendLine("\nцена: " + weekRules.Select(x => x.Price).Min());
					result.AppendLine("минимум 19л: " + weekRules.Select(x => x.DeliveryPriceRule.Water19LCount).Min());
					result.AppendLine("минимум 6л: " + weekRules.Select(x => x.DeliveryPriceRule.Water6LCount).Min());
					result.AppendLine("минимум 1,5л: " + weekRules.Select(x => x.DeliveryPriceRule.Water1500mlCount).Min());
					result.AppendLine("минимум 0,5л: " + weekRules.Select(x => x.DeliveryPriceRule.Water500mlCount).Min());
				}
				else if(ObservableCommonDistrictRuleItems.Any())
				{
					result.AppendLine("\nцена: " + ObservableCommonDistrictRuleItems
						.Select(x => x.Price).Min());
					result.AppendLine("минимум 19л: " + ObservableCommonDistrictRuleItems
						.Select(x => x.DeliveryPriceRule.Water19LCount).Min());
					result.AppendLine("минимум 6л: " + ObservableCommonDistrictRuleItems
						.Select(x => x.DeliveryPriceRule.Water6LCount).Min());
					result.AppendLine("минимум 1,5л: " + ObservableCommonDistrictRuleItems
						.Select(x => x.DeliveryPriceRule.Water1500mlCount).Min());
					result.AppendLine("минимум 0,5л: " + ObservableCommonDistrictRuleItems
						.Select(x => x.DeliveryPriceRule.Water500mlCount).Min());
				}
				else
					result.AppendLine();

				if(deliveryScheduleRestriction.Key == WeekDayName.Today)
				{
					var groupedRestrictions = deliveryScheduleRestriction
						.Where(x => x.AcceptBefore != null)
						.GroupBy(x => x.AcceptBefore.Name)
						.OrderBy(x => x.Key);

					foreach(var group in groupedRestrictions)
					{
						result.Append(withMarkup ? $"<b>до {group.Key}:</b> " : $"до {group.Key}: ");

						int i = 1;
						int maxScheduleCountOnLine = 3;
						var restrictions = group.OrderBy(x => x.DeliverySchedule.From).ThenBy(x => x.DeliverySchedule.To).ToList();
						int lastItemId = restrictions.Last().Id;
						foreach(var restriction in restrictions)
						{
							result.Append(restriction.DeliverySchedule.Name);
							result.Append(restriction.Id == lastItemId ? ";" : ", ");
							if(i == maxScheduleCountOnLine && restriction.Id != lastItemId)
							{
								result.AppendLine();
								maxScheduleCountOnLine = 4;
								i = 0;
							}
							i++;
						}
						result.AppendLine();
					}
				}
				else
				{
					var restrictions = deliveryScheduleRestriction.OrderBy(x => x.DeliverySchedule.From).ThenBy(x => x.DeliverySchedule.To).ToList();
					int maxScheduleCountOnLine = 4;
					int i = 1;
					int lastItemId = restrictions.Last().Id;
					foreach(var restriction in restrictions)
					{
						result.Append(restriction.DeliverySchedule.Name);
						result.Append(restriction.Id == lastItemId ? ";" : ", ");
						if(i == maxScheduleCountOnLine && restriction.Id != lastItemId)
						{
							result.AppendLine();
							i = 0;
						}
						i++;
					}
					result.AppendLine();
				}
				result.AppendLine();
			}
			//Удаление лишних переносов строк
			result.Length -= 2;
			return result.ToString();
		}

		public virtual IEnumerable<DeliveryScheduleRestriction> GetAllDeliveryScheduleRestrictions()
		{
			return TodayDeliveryScheduleRestrictions
					.Concat(MondayDeliveryScheduleRestrictions)
					.Concat(TuesdayDeliveryScheduleRestrictions)
					.Concat(WednesdayDeliveryScheduleRestrictions)
					.Concat(ThursdayDeliveryScheduleRestrictions)
					.Concat(FridayDeliveryScheduleRestrictions)
					.Concat(SaturdayDeliveryScheduleRestrictions)
					.Concat(SundayDeliveryScheduleRestrictions);
		}

		public virtual IEnumerable<WeekDayDistrictRuleItem> GetAllWeekDayDistrictRuleItems()
		{
			return TodayDistrictRuleItems
					.Concat(MondayDistrictRuleItems)
					.Concat(TuesdayDistrictRuleItems)
					.Concat(WednesdayDistrictRuleItems)
					.Concat(ThursdayDistrictRuleItems)
					.Concat(FridayDistrictRuleItems)
					.Concat(SaturdayDistrictRuleItems)
					.Concat(SundayDistrictRuleItems);
		}

		public virtual GenericObservableList<WeekDayDistrictRuleItem> GetWeekDayRuleItemCollectionByWeekDayName(WeekDayName weekDayName)
		{
			switch(weekDayName)
			{
				case WeekDayName.Today: return ObservableTodayDistrictRuleItems;
				case WeekDayName.Monday: return ObservableMondayDistrictRuleItems;
				case WeekDayName.Tuesday: return ObservableTuesdayDistrictRuleItems;
				case WeekDayName.Wednesday: return ObservableWednesdayDistrictRuleItems;
				case WeekDayName.Thursday: return ObservableThursdayDistrictRuleItems;
				case WeekDayName.Friday: return ObservableFridayDistrictRuleItems;
				case WeekDayName.Saturday: return ObservableSaturdayDistrictRuleItems;
				case WeekDayName.Sunday: return ObservableSundayDistrictRuleItems;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		public virtual GenericObservableList<DeliveryScheduleRestriction> GetScheduleRestrictionCollectionByWeekDayName(WeekDayName weekDayName)
		{
			switch(weekDayName)
			{
				case WeekDayName.Today: return ObservableTodayDeliveryScheduleRestrictions;
				case WeekDayName.Monday: return ObservableMondayDeliveryScheduleRestrictions;
				case WeekDayName.Tuesday: return ObservableTuesdayDeliveryScheduleRestrictions;
				case WeekDayName.Wednesday: return ObservableWednesdayDeliveryScheduleRestrictions;
				case WeekDayName.Thursday: return ObservableThursdayDeliveryScheduleRestrictions;
				case WeekDayName.Friday: return ObservableFridayDeliveryScheduleRestrictions;
				case WeekDayName.Saturday: return ObservableSaturdayDeliveryScheduleRestrictions;
				case WeekDayName.Sunday: return ObservableSundayDeliveryScheduleRestrictions;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		private void ClearAllDeliveryScheduleRestrictions()
		{
			TodayDeliveryScheduleRestrictions.Clear();
			MondayDeliveryScheduleRestrictions.Clear();
			TuesdayDeliveryScheduleRestrictions.Clear();
			WednesdayDeliveryScheduleRestrictions.Clear();
			ThursdayDeliveryScheduleRestrictions.Clear();
			FridayDeliveryScheduleRestrictions.Clear();
			SaturdayDeliveryScheduleRestrictions.Clear();
			SundayDeliveryScheduleRestrictions.Clear();
		}

		public virtual void ReplaceDistrictDeliveryScheduleRestrictions(IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictions)
		{
			if(deliveryScheduleRestrictions == null)
			{
				throw new ArgumentException(
					 "Отсутствуют данные новых графиков доставки");
			}

			if(deliveryScheduleRestrictions.Any(s => s.District.Id != Id))
			{
				throw new ArgumentException(
					 "Id района в который добавляется график доставки должен совпадать с Id района в новом графике доставки");
			}

			ClearAllDeliveryScheduleRestrictions();

			foreach(var schedule in deliveryScheduleRestrictions)
			{
				switch(schedule.WeekDay)
				{
					case WeekDayName.Today:
						ObservableTodayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Monday:
						ObservableMondayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Tuesday:
						ObservableTuesdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Wednesday:
						ObservableWednesdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Thursday:
						ObservableThursdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Friday:
						ObservableFridayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Saturday:
						ObservableSaturdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Sunday:
						ObservableSundayDeliveryScheduleRestrictions.Add(schedule);
						break;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

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
				if(orderStateKey.Order.DeliveryDate.Value.Date == DateTime.Today && ObservableTodayDistrictRuleItems.Any())
				{
					var todayDeliveryRules = ObservableTodayDistrictRuleItems.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();

					if(todayDeliveryRules.Any())
					{
						var todayMinEShopGoodsSum =
							todayDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);

						if(eShopGoodsSum < todayMinEShopGoodsSum || todayMinEShopGoodsSum == 0)
							return todayDeliveryRules.Max(x => x.Price);
					}
					return 0m;
				}
				var dayOfWeekRules = GetWeekDayRuleItemCollectionByWeekDayName(ConvertDayOfWeekToWeekDayName(orderStateKey.Order.DeliveryDate.Value.DayOfWeek));
				if(dayOfWeekRules.Any())
				{
					var dayOfWeekDeliveryRules = dayOfWeekRules.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();

					if(dayOfWeekDeliveryRules.Any())
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
				CommonDistrictRuleItems.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();

			if(commonDeliveryRules.Any())
			{
				var commonMinEShopGoodsSum = commonDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);

				if(eShopGoodsSum < commonMinEShopGoodsSum || commonMinEShopGoodsSum == 0)
					return commonDeliveryRules.Max(x => x.Price);
			}

			return 0m;
		}

		private void InitializeAllCollections()
		{
			CommonDistrictRuleItems = new List<CommonDistrictRuleItem>();

			TodayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			MondayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			TuesdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			WednesdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			ThursdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			FridayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			SaturdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			SundayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();

			TodayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			MondayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			TuesdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			WednesdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			ThursdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			FridayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			SaturdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			SundayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		}

		/// <summary>
		/// Активная ли версия районов, связанная с этим районом
		/// </summary>
		public virtual bool IsActive => DistrictsSet?.Status == DistrictsSetStatus.Active;

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrWhiteSpace(DistrictName))
			{
				yield return new ValidationResult(
					"Необходимо заполнить имя района",
					new[] { this.GetPropertyName(o => o.DistrictName) }
				);
			}
			if(GeographicGroup == null)
			{
				yield return new ValidationResult(
					$"Для района \"{DistrictName}\" необходимо указать часть города, содержащую этот район доставки",
					new[] { this.GetPropertyName(o => o.GeographicGroup) }
				);
			}
			if(DistrictBorder == null)
			{
				yield return new ValidationResult(
					$"Для района \"{DistrictName}\" необходимо нарисовать границы на карте",
					new[] { this.GetPropertyName(o => o.DistrictBorder) }
				);
			}
			if(WageDistrict == null)
			{
				yield return new ValidationResult(
					$"Для района \"{DistrictName}\" необходимо выбрать зарплатную группу",
					new[] { this.GetPropertyName(o => o.WageDistrict) }
				);
			}
			if(ObservableCommonDistrictRuleItems.Any(i => i.Price <= 0))
			{
				yield return new ValidationResult(
					$"Для всех правил доставки для района \"{DistrictName}\" должны быть указаны цены",
					new[] { this.GetPropertyName(o => o.CommonDistrictRuleItems) }
				);
			}
			if(GetAllWeekDayDistrictRuleItems().Any(i => i.Price <= 0))
			{
				yield return new ValidationResult(
					$"Для всех особых правил доставки для района \"{DistrictName}\" должны быть указаны цены"
				);
			}
			if(ObservableTodayDeliveryScheduleRestrictions.Any(i => i.AcceptBefore == null))
			{
				yield return new ValidationResult(
					$"Для графиков доставки \"день в день\" для района \"{DistrictName}\" должно быть указано время приема до"
				);
			}
		}

		#endregion

		#region ICloneable implementation

		public virtual object Clone()
		{
			var newDistrict = new District
			{
				DistrictName = DistrictName,
				DistrictBorder = DistrictBorder?.Copy(),
				WageDistrict = WageDistrict,
				GeographicGroup = GeographicGroup,
				PriceType = PriceType,
				MinBottles = MinBottles,
				TariffZone = TariffZone,
				WaterPrice = WaterPrice
			};
			newDistrict.InitializeAllCollections();

			foreach(var commonRuleItem in CommonDistrictRuleItems)
			{
				var newCommonRuleItem = (CommonDistrictRuleItem)commonRuleItem.Clone();
				newCommonRuleItem.District = newDistrict;
				newDistrict.CommonDistrictRuleItems
					.Add(newCommonRuleItem);
			}
			foreach(var scheduleRestriction in GetAllDeliveryScheduleRestrictions())
			{
				var newScheduleRestriction = (DeliveryScheduleRestriction)scheduleRestriction.Clone();
				newScheduleRestriction.District = newDistrict;
				newDistrict.GetScheduleRestrictionCollectionByWeekDayName(scheduleRestriction.WeekDay)
					.Add(newScheduleRestriction);
			}
			foreach(var weekDayRule in GetAllWeekDayDistrictRuleItems())
			{
				var newWeekDayRule = (WeekDayDistrictRuleItem)weekDayRule.Clone();
				newWeekDayRule.District = newDistrict;
				newDistrict.GetWeekDayRuleItemCollectionByWeekDayName(weekDayRule.WeekDay)
					.Add(newWeekDayRule);
			}

			return newDistrict;
		}

		#endregion

		public static WeekDayName ConvertDayOfWeekToWeekDayName(DayOfWeek dayOfWeek)
		{
			switch(dayOfWeek)
			{
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

		public static District GetDistrictFromActiveDistrictsSetOrNull(District district)
		{
			while(true)
			{
				if(district?.DistrictsSet == null)
				{
					return null;
				}

				if(district.DistrictsSet.Status == DistrictsSetStatus.Active)
				{
					return district;
				}

				var ruleItems = district.CommonDistrictRuleItems.ToList();
				var copyItems = district.DistrictCopyItems.ToList();

				return null;
				//district = district.CopiedTo;
			}
		}
	}
}
