using Gamma.Utilities;
using NetTopologySuite.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
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

		private string _districtName;
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

		private GenericObservableList<WeekDayDistrictRuleItem> _todayDistrictRuleItems;
		private GenericObservableList<WeekDayDistrictRuleItem> _mondayDistrictRuleItems;
		private GenericObservableList<WeekDayDistrictRuleItem> _tuesdayDistrictRuleItems;
		private GenericObservableList<WeekDayDistrictRuleItem> _wednesdayDistrictRuleItems;
		private GenericObservableList<WeekDayDistrictRuleItem> _thursdayDistrictRuleItems;
		private GenericObservableList<WeekDayDistrictRuleItem> _fridayDistrictRuleItems;
		private GenericObservableList<WeekDayDistrictRuleItem> _saturdayDistrictRuleItems;
		private GenericObservableList<WeekDayDistrictRuleItem> _sundayDistrictRuleItems;

		private GenericObservableList<DeliveryScheduleRestriction> _todayDeliveryScheduleRestrictions;
		private GenericObservableList<DeliveryScheduleRestriction> _mondayDeliveryScheduleRestrictions;
		private GenericObservableList<DeliveryScheduleRestriction> _tuesdayDeliveryScheduleRestrictions;
		private GenericObservableList<DeliveryScheduleRestriction> _wednesdayDeliveryScheduleRestrictions;
		private GenericObservableList<DeliveryScheduleRestriction> _thursdayDeliveryScheduleRestrictions;
		private GenericObservableList<DeliveryScheduleRestriction> _fridayDeliveryScheduleRestrictions;
		private GenericObservableList<DeliveryScheduleRestriction> _saturdayDeliveryScheduleRestrictions;
		private GenericObservableList<DeliveryScheduleRestriction> _sundayDeliveryScheduleRestrictions;

		private IList<DeliveryScheduleRestriction> _allDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		private GenericObservableList<DeliveryScheduleRestriction> _observableAllDeliveryScheduleRestrictions;

		private GenericObservableList<CommonDistrictRuleItem> _commonDistrictRuleItems;

		private IList<DistrictRuleItemBase> _allDistrictRuleItems = new List<DistrictRuleItemBase>();
		private GenericObservableList<DistrictRuleItemBase> _observableAllDistrictRuleItems;

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
				{
					WaterPrice = 0;
				}
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

		#region AllDistrictRuleItems

		[Display(Name = "Правила и цены доставки района")]
		public virtual IList<DistrictRuleItemBase> AllDistrictRuleItems
		{
			get => _allDistrictRuleItems;
			set => SetField(ref _allDistrictRuleItems, value);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DistrictRuleItemBase> ObservableAllDistrictRuleItems =>
			_observableAllDistrictRuleItems ?? (_observableAllDistrictRuleItems =
				new GenericObservableList<DistrictRuleItemBase>(AllDistrictRuleItems));

		#endregion AllDistrictRuleItems

		#region CommonDistrictRuleItems

		public virtual GenericObservableList<CommonDistrictRuleItem> CommonDistrictRuleItems
		{
			get
			{
				if(_commonDistrictRuleItems is null)
				{
					_commonDistrictRuleItems =
						new GenericObservableList<CommonDistrictRuleItem>(AllDistrictRuleItems
							.Where(drib => drib is CommonDistrictRuleItem)
							.Cast<CommonDistrictRuleItem>()
							.ToList());

					_commonDistrictRuleItems.ElementAdded += OnObservableDistrictRuleItemsElementAdded;
					_commonDistrictRuleItems.ElementRemoved += OnObservableDistrictRuleItemsElementRemoved;
				}

				return _commonDistrictRuleItems;
			}
		}

		#endregion

		#region WeekDayDistricRuleItems

		public virtual GenericObservableList<WeekDayDistrictRuleItem> TodayDistrictRuleItems
		{
			get
			{
				if(_todayDistrictRuleItems is null)
				{
					_todayDistrictRuleItems =
						new GenericObservableList<WeekDayDistrictRuleItem>(AllDistrictRuleItems
							.Where(drib => drib is WeekDayDistrictRuleItem wddri && wddri.WeekDay == WeekDayName.Today)
							.Cast<WeekDayDistrictRuleItem>()
							.ToList());

					_todayDistrictRuleItems.ElementAdded += OnObservableDistrictRuleItemsElementAdded;
					_todayDistrictRuleItems.ElementRemoved += OnObservableDistrictRuleItemsElementRemoved;
				}

				return _todayDistrictRuleItems;
			}
		}

		public virtual GenericObservableList<WeekDayDistrictRuleItem> MondayDistrictRuleItems
		{
			get
			{
				if(_mondayDistrictRuleItems is null)
				{
					_mondayDistrictRuleItems = new GenericObservableList<WeekDayDistrictRuleItem>(AllDistrictRuleItems
						.Where(drib => drib is WeekDayDistrictRuleItem wddri && wddri.WeekDay == WeekDayName.Monday)
						.Cast<WeekDayDistrictRuleItem>()
						.ToList());

					_mondayDistrictRuleItems.ElementAdded += OnObservableDistrictRuleItemsElementAdded;
					_mondayDistrictRuleItems.ElementRemoved += OnObservableDistrictRuleItemsElementRemoved;
				}

				return _mondayDistrictRuleItems;
			}
		}


		public virtual GenericObservableList<WeekDayDistrictRuleItem> TuesdayDistrictRuleItems
		{
			get
			{
				if(_tuesdayDistrictRuleItems is null)
				{
					_tuesdayDistrictRuleItems = new GenericObservableList<WeekDayDistrictRuleItem>(AllDistrictRuleItems
						.Where(drib => drib is WeekDayDistrictRuleItem wddri && wddri.WeekDay == WeekDayName.Tuesday)
						.Cast<WeekDayDistrictRuleItem>()
						.ToList());

					_tuesdayDistrictRuleItems.ElementAdded += OnObservableDistrictRuleItemsElementAdded;
					_tuesdayDistrictRuleItems.ElementRemoved += OnObservableDistrictRuleItemsElementRemoved;
				}

				return _tuesdayDistrictRuleItems;
			}
		}

		public virtual GenericObservableList<WeekDayDistrictRuleItem> WednesdayDistrictRuleItems
		{
			get
			{
				if(_wednesdayDistrictRuleItems is null)
				{
					_wednesdayDistrictRuleItems = new GenericObservableList<WeekDayDistrictRuleItem>(AllDistrictRuleItems
						.Where(drib => drib is WeekDayDistrictRuleItem wddri && wddri.WeekDay == WeekDayName.Wednesday)
						.Cast<WeekDayDistrictRuleItem>()
						.ToList());

					_wednesdayDistrictRuleItems.ElementAdded += OnObservableDistrictRuleItemsElementAdded;
					_wednesdayDistrictRuleItems.ElementRemoved += OnObservableDistrictRuleItemsElementRemoved;
				}

				return _wednesdayDistrictRuleItems;
			}
		}

		public virtual GenericObservableList<WeekDayDistrictRuleItem> ThursdayDistrictRuleItems
		{
			get
			{
				if(_thursdayDistrictRuleItems is null)
				{
					_thursdayDistrictRuleItems = new GenericObservableList<WeekDayDistrictRuleItem>(AllDistrictRuleItems
						.Where(drib => drib is WeekDayDistrictRuleItem wddri && wddri.WeekDay == WeekDayName.Thursday)
						.Cast<WeekDayDistrictRuleItem>()
						.ToList());

					_thursdayDistrictRuleItems.ElementAdded += OnObservableDistrictRuleItemsElementAdded;
					_thursdayDistrictRuleItems.ElementRemoved += OnObservableDistrictRuleItemsElementRemoved;
				}

				return _thursdayDistrictRuleItems;
			}
		}

		public virtual GenericObservableList<WeekDayDistrictRuleItem> FridayDistrictRuleItems
		{
			get
			{
				if(_fridayDistrictRuleItems is null)
				{
					_fridayDistrictRuleItems = new GenericObservableList<WeekDayDistrictRuleItem>(AllDistrictRuleItems
						.Where(drib => drib is WeekDayDistrictRuleItem wddri && wddri.WeekDay == WeekDayName.Friday)
						.Cast<WeekDayDistrictRuleItem>()
						.ToList());

					_fridayDistrictRuleItems.ElementAdded += OnObservableDistrictRuleItemsElementAdded;
					_fridayDistrictRuleItems.ElementRemoved += OnObservableDistrictRuleItemsElementRemoved;
				}

				return _fridayDistrictRuleItems;
			}
		}

		public virtual GenericObservableList<WeekDayDistrictRuleItem> SaturdayDistrictRuleItems
		{
			get
			{
				if(_saturdayDistrictRuleItems is null)
				{
					_saturdayDistrictRuleItems = new GenericObservableList<WeekDayDistrictRuleItem>(AllDistrictRuleItems
						.Where(drib => drib is WeekDayDistrictRuleItem wddri && wddri.WeekDay == WeekDayName.Saturday)
						.Cast<WeekDayDistrictRuleItem>()
						.ToList());

					_saturdayDistrictRuleItems.ElementAdded += OnObservableDistrictRuleItemsElementAdded;
					_saturdayDistrictRuleItems.ElementRemoved += OnObservableDistrictRuleItemsElementRemoved;
				}

				return _saturdayDistrictRuleItems;
			}
		}

		public virtual GenericObservableList<WeekDayDistrictRuleItem> SundayDistrictRuleItems
		{
			get
			{
				if(_sundayDistrictRuleItems is null)
				{
					_sundayDistrictRuleItems = new GenericObservableList<WeekDayDistrictRuleItem>(AllDistrictRuleItems
						.Where(drib => drib is WeekDayDistrictRuleItem wddri && wddri.WeekDay == WeekDayName.Sunday)
						.Cast<WeekDayDistrictRuleItem>()
						.ToList());

					_sundayDistrictRuleItems.ElementAdded += OnObservableDistrictRuleItemsElementAdded;
					_sundayDistrictRuleItems.ElementRemoved += OnObservableDistrictRuleItemsElementRemoved;
				}

				return _sundayDistrictRuleItems;
			}
		}

		#endregion

		#region DeliveryScheduleRestrictions

		public virtual GenericObservableList<DeliveryScheduleRestriction> TodayDeliveryScheduleRestrictions
		{
			get
			{
				if(_todayDeliveryScheduleRestrictions is null)
				{
					_todayDeliveryScheduleRestrictions =
						new GenericObservableList<DeliveryScheduleRestriction>(AllDeliveryScheduleRestrictions.Where(drr => drr.WeekDay == WeekDayName.Today).ToList());

					_todayDeliveryScheduleRestrictions.ElementAdded += OnDeliveryScheduleRestrictionsElementAdded;
					_todayDeliveryScheduleRestrictions.ElementRemoved += OnDeliveryScheduleRestrictionsElementRemoved;
				}

				return _todayDeliveryScheduleRestrictions;
			}
		}

		public virtual GenericObservableList<DeliveryScheduleRestriction> MondayDeliveryScheduleRestrictions
		{
			get
			{
				if(_mondayDeliveryScheduleRestrictions is null)
				{
					_mondayDeliveryScheduleRestrictions =
						new GenericObservableList<DeliveryScheduleRestriction>(AllDeliveryScheduleRestrictions.Where(drr => drr.WeekDay == WeekDayName.Monday).ToList());

					_mondayDeliveryScheduleRestrictions.ElementAdded += OnDeliveryScheduleRestrictionsElementAdded;
					_mondayDeliveryScheduleRestrictions.ElementRemoved += OnDeliveryScheduleRestrictionsElementRemoved;
				}

				return _mondayDeliveryScheduleRestrictions;
			}
		}

		public virtual GenericObservableList<DeliveryScheduleRestriction> TuesdayDeliveryScheduleRestrictions
		{
			get
			{
				if(_tuesdayDeliveryScheduleRestrictions is null)
				{
					_tuesdayDeliveryScheduleRestrictions =
						new GenericObservableList<DeliveryScheduleRestriction>(AllDeliveryScheduleRestrictions.Where(drr => drr.WeekDay == WeekDayName.Tuesday).ToList());

					_tuesdayDeliveryScheduleRestrictions.ElementAdded += OnDeliveryScheduleRestrictionsElementAdded;
					_tuesdayDeliveryScheduleRestrictions.ElementRemoved += OnDeliveryScheduleRestrictionsElementRemoved;
				}

				return _tuesdayDeliveryScheduleRestrictions;
			}
		}

		public virtual GenericObservableList<DeliveryScheduleRestriction> WednesdayDeliveryScheduleRestrictions
		{
			get
			{
				if(_wednesdayDeliveryScheduleRestrictions is null)
				{
					_wednesdayDeliveryScheduleRestrictions =
						new GenericObservableList<DeliveryScheduleRestriction>(AllDeliveryScheduleRestrictions.Where(drr => drr.WeekDay == WeekDayName.Wednesday).ToList());

					_wednesdayDeliveryScheduleRestrictions.ElementAdded += OnDeliveryScheduleRestrictionsElementAdded;
					_wednesdayDeliveryScheduleRestrictions.ElementRemoved += OnDeliveryScheduleRestrictionsElementRemoved;
				}

				return _wednesdayDeliveryScheduleRestrictions;
			}
		}

		public virtual GenericObservableList<DeliveryScheduleRestriction> ThursdayDeliveryScheduleRestrictions
		{
			get
			{
				if(_thursdayDeliveryScheduleRestrictions is null)
				{
					_thursdayDeliveryScheduleRestrictions =
						new GenericObservableList<DeliveryScheduleRestriction>(AllDeliveryScheduleRestrictions.Where(drr => drr.WeekDay == WeekDayName.Thursday).ToList());

					_thursdayDeliveryScheduleRestrictions.ElementAdded += OnDeliveryScheduleRestrictionsElementAdded;
					_thursdayDeliveryScheduleRestrictions.ElementRemoved += OnDeliveryScheduleRestrictionsElementRemoved;
				}

				return _thursdayDeliveryScheduleRestrictions;
			}
		}

		public virtual GenericObservableList<DeliveryScheduleRestriction> FridayDeliveryScheduleRestrictions
		{
			get
			{
				if(_fridayDeliveryScheduleRestrictions is null)
				{
					_fridayDeliveryScheduleRestrictions =
						new GenericObservableList<DeliveryScheduleRestriction>(AllDeliveryScheduleRestrictions.Where(drr => drr.WeekDay == WeekDayName.Friday).ToList());

					_fridayDeliveryScheduleRestrictions.ElementAdded += OnDeliveryScheduleRestrictionsElementAdded;
					_fridayDeliveryScheduleRestrictions.ElementRemoved += OnDeliveryScheduleRestrictionsElementRemoved;
				}
				return _fridayDeliveryScheduleRestrictions;
			}
		}

		public virtual GenericObservableList<DeliveryScheduleRestriction> SaturdayDeliveryScheduleRestrictions
		{
			get
			{
				if(_saturdayDeliveryScheduleRestrictions is null)
				{
					_saturdayDeliveryScheduleRestrictions =
						new GenericObservableList<DeliveryScheduleRestriction>(AllDeliveryScheduleRestrictions.Where(drr => drr.WeekDay == WeekDayName.Saturday).ToList());

					_saturdayDeliveryScheduleRestrictions.ElementAdded += OnDeliveryScheduleRestrictionsElementAdded;
					_saturdayDeliveryScheduleRestrictions.ElementRemoved += OnDeliveryScheduleRestrictionsElementRemoved;
				}

				return _saturdayDeliveryScheduleRestrictions;
			}
		}

		public virtual GenericObservableList<DeliveryScheduleRestriction> SundayDeliveryScheduleRestrictions
		{
			get
			{
				if(_sundayDeliveryScheduleRestrictions is null)
				{
					_sundayDeliveryScheduleRestrictions =
						new GenericObservableList<DeliveryScheduleRestriction>(AllDeliveryScheduleRestrictions.Where(drr => drr.WeekDay == WeekDayName.Sunday).ToList());

					_sundayDeliveryScheduleRestrictions.ElementAdded += OnDeliveryScheduleRestrictionsElementAdded;
					_sundayDeliveryScheduleRestrictions.ElementRemoved += OnDeliveryScheduleRestrictionsElementRemoved;
				}

				return _sundayDeliveryScheduleRestrictions;
			}
		}

		public virtual IList<DeliveryScheduleRestriction> AllDeliveryScheduleRestrictions
		{
			get => _allDeliveryScheduleRestrictions;
			set => SetField(ref _allDeliveryScheduleRestrictions, value);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableAllDeliveryScheduleRestrictions =>
			_observableAllDeliveryScheduleRestrictions ?? (_observableAllDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(AllDeliveryScheduleRestrictions));

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
				else if(CommonDistrictRuleItems.Any())
				{
					result.AppendLine("\nцена: " + CommonDistrictRuleItems
						.Select(x => x.Price).Min());
					result.AppendLine("минимум 19л: " + CommonDistrictRuleItems
						.Select(x => x.DeliveryPriceRule.Water19LCount).Min());
					result.AppendLine("минимум 6л: " + CommonDistrictRuleItems
						.Select(x => x.DeliveryPriceRule.Water6LCount).Min());
					result.AppendLine("минимум 1,5л: " + CommonDistrictRuleItems
						.Select(x => x.DeliveryPriceRule.Water1500mlCount).Min());
					result.AppendLine("минимум 0,5л: " + CommonDistrictRuleItems
						.Select(x => x.DeliveryPriceRule.Water500mlCount).Min());
				}
				else
				{
					result.AppendLine();
				}

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

		public virtual IEnumerable<DeliveryScheduleRestriction> GetAvailableDeliveryScheduleRestrictionsByDeliveryDate(
			DateTime? deliveryDate)
		{
			if(deliveryDate == null)
			{
				return new List<DeliveryScheduleRestriction>();
			}

			var deliveryScheduleRestriction = GetDeliveryScheduleRestrictionsByDeliveryDate(deliveryDate);

			var isDeliveryDateToday = deliveryDate.Value == DateTime.Today;
			var isDeliveryDateTomorrow = deliveryDate.Value == DateTime.Today.AddDays(1);

			if(isDeliveryDateToday || isDeliveryDateTomorrow)
			{
				var nowTime = DateTime.Now.TimeOfDay;

				return deliveryScheduleRestriction.Where(r => r.AcceptBefore == null || r.AcceptBefore?.Time > nowTime);
			}

			return deliveryScheduleRestriction;
		}

		private IEnumerable<DeliveryScheduleRestriction> GetDeliveryScheduleRestrictionsByDeliveryDate(DateTime? deliveryDate)
		{
			if(deliveryDate == null)
			{
				return new List<DeliveryScheduleRestriction>();
			}

			if(deliveryDate.Value == DateTime.Today)
			{
				return TodayDeliveryScheduleRestrictions;
			}

			switch(deliveryDate.Value.DayOfWeek)
			{
				case DayOfWeek.Sunday:
					return SundayDeliveryScheduleRestrictions;
				case DayOfWeek.Monday:
					return MondayDeliveryScheduleRestrictions;
				case DayOfWeek.Tuesday:
					return TuesdayDeliveryScheduleRestrictions;
				case DayOfWeek.Wednesday:
					return WednesdayDeliveryScheduleRestrictions;
				case DayOfWeek.Thursday:
					return ThursdayDeliveryScheduleRestrictions;
				case DayOfWeek.Friday:
					return FridayDeliveryScheduleRestrictions;
				case DayOfWeek.Saturday:
					return SaturdayDeliveryScheduleRestrictions;
				default:
					return new List<DeliveryScheduleRestriction>();
			}
		}

		public virtual IEnumerable<DateTime> GetNearestDatesWhenDeliveryIsPossible(
			int datesCountInResult = 2,
			int maxSearchPeriodInDays = 30)
		{
			var nearestDates = new List<DateTime>();
			var startDate = DateTime.Today;

			for(int i = 0; i < maxSearchPeriodInDays; i++)
			{
				var date = startDate.AddDays(i);

				var deliveryScheduleRestrictions =
					GetAvailableDeliveryScheduleRestrictionsByDeliveryDate(date);

				if(deliveryScheduleRestrictions.Count() > 0)
				{
					nearestDates.Add(date);
				}

				if(nearestDates.Count == 2)
				{
					break;
				}
			}

			return nearestDates;
		}

		private void OnDeliveryScheduleRestrictionsElementAdded(object aList, int[] aIdx)
		{
			if(aList is GenericObservableList<DeliveryScheduleRestriction> gol
				&& gol[aIdx] is DeliveryScheduleRestriction newDsr
				&& !_allDeliveryScheduleRestrictions.Any(adsr => adsr.Id == newDsr.Id && newDsr.Id != 0))
			{
				_allDeliveryScheduleRestrictions.Add(newDsr);
			}
		}

		private void OnDeliveryScheduleRestrictionsElementRemoved(object aList, int[] aIdx, object aObject)
		{
			if(aObject is DeliveryScheduleRestriction deletedDsr
				&& _allDeliveryScheduleRestrictions.Any(adsr => adsr.Id == deletedDsr.Id))
			{
				_allDeliveryScheduleRestrictions.Remove(deletedDsr);
			}
		}

		private void OnObservableDistrictRuleItemsElementAdded(object aList, int[] aIdx)
		{
			if(aList is GenericObservableList<WeekDayDistrictRuleItem> wdril
				&& wdril[aIdx] is WeekDayDistrictRuleItem newWddri
				&& !_allDistrictRuleItems.Any(adsr => adsr.Id == newWddri.Id && newWddri.Id != 0))
			{
				_allDistrictRuleItems.Add(newWddri);
				return;
			}

			if(aList is GenericObservableList<CommonDistrictRuleItem> cdril
				&& cdril[aIdx] is CommonDistrictRuleItem newCdrib
				&& !_allDistrictRuleItems.Any(adsr => adsr.Id == newCdrib.Id && newCdrib.Id != 0))
			{
				_allDistrictRuleItems.Add(newCdrib);
				return;
			}
		}

		private void OnObservableDistrictRuleItemsElementRemoved(object aList, int[] aIdx, object aObject)
		{
			if(aObject is DistrictRuleItemBase deletedDrib
				&& _allDistrictRuleItems.Any(adri => adri.Id == deletedDrib.Id))
			{
				_allDistrictRuleItems.Remove(deletedDrib);
			}
		}

		public virtual IEnumerable<DeliveryScheduleRestriction> GetAllDeliveryScheduleRestrictions()
		{
			return AllDeliveryScheduleRestrictions;
		}

		public virtual IEnumerable<WeekDayDistrictRuleItem> GetAllWeekDayDistrictRuleItems()
		{
			return AllDistrictRuleItems.Where(adri => adri is WeekDayDistrictRuleItem).Cast<WeekDayDistrictRuleItem>();
		}

		public virtual GenericObservableList<WeekDayDistrictRuleItem> GetWeekDayRuleItemCollectionByWeekDayName(WeekDayName weekDayName)
		{
			switch(weekDayName)
			{
				case WeekDayName.Today:
					return TodayDistrictRuleItems;
				case WeekDayName.Monday:
					return MondayDistrictRuleItems;
				case WeekDayName.Tuesday:
					return TuesdayDistrictRuleItems;
				case WeekDayName.Wednesday:
					return WednesdayDistrictRuleItems;
				case WeekDayName.Thursday:
					return ThursdayDistrictRuleItems;
				case WeekDayName.Friday:
					return FridayDistrictRuleItems;
				case WeekDayName.Saturday:
					return SaturdayDistrictRuleItems;
				case WeekDayName.Sunday:
					return SundayDistrictRuleItems;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public virtual GenericObservableList<DeliveryScheduleRestriction> GetScheduleRestrictionCollectionByWeekDayName(WeekDayName weekDayName)
		{
			switch(weekDayName)
			{
				case WeekDayName.Today:
					return TodayDeliveryScheduleRestrictions;
				case WeekDayName.Monday:
					return MondayDeliveryScheduleRestrictions;
				case WeekDayName.Tuesday:
					return TuesdayDeliveryScheduleRestrictions;
				case WeekDayName.Wednesday:
					return WednesdayDeliveryScheduleRestrictions;
				case WeekDayName.Thursday:
					return ThursdayDeliveryScheduleRestrictions;
				case WeekDayName.Friday:
					return FridayDeliveryScheduleRestrictions;
				case WeekDayName.Saturday:
					return SaturdayDeliveryScheduleRestrictions;
				case WeekDayName.Sunday:
					return SundayDeliveryScheduleRestrictions;
				default:
					throw new ArgumentOutOfRangeException();
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
			AllDeliveryScheduleRestrictions.Clear();
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
						TodayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Monday:
						MondayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Tuesday:
						TuesdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Wednesday:
						WednesdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Thursday:
						ThursdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Friday:
						FridayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Saturday:
						SaturdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Sunday:
						SundayDeliveryScheduleRestrictions.Add(schedule);
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
		public virtual decimal GetDeliveryPrice(ComparerDeliveryPrice comparerDeliveryPrice, decimal eShopGoodsSum)
		{
			if(comparerDeliveryPrice.DeliveryDate.HasValue)
			{
				if(comparerDeliveryPrice.DeliveryDate.Value.Date == DateTime.Today && TodayDistrictRuleItems.Any())
				{
					var todayDeliveryRules =
						TodayDistrictRuleItems.Where(x => comparerDeliveryPrice.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();

					if(todayDeliveryRules.Any())
					{
						var todayMinEShopGoodsSum =
							todayDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);

						if(eShopGoodsSum < todayMinEShopGoodsSum || todayMinEShopGoodsSum == 0)
						{
							return todayDeliveryRules.Max(x => x.Price);
						}
					}

					return 0m;
				}
				
				var dayOfWeekRules =
					GetWeekDayRuleItemCollectionByWeekDayName(
						ConvertDayOfWeekToWeekDayName(comparerDeliveryPrice.DeliveryDate.Value.DayOfWeek));
				
				if(dayOfWeekRules.Any())
				{
					var dayOfWeekDeliveryRules = 
						dayOfWeekRules.Where(x => comparerDeliveryPrice.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();

					if(dayOfWeekDeliveryRules.Any())
					{
						var dayOfWeekEShopGoodsSum =
							dayOfWeekDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);

						if(eShopGoodsSum < dayOfWeekEShopGoodsSum || dayOfWeekEShopGoodsSum == 0)
						{
							return dayOfWeekDeliveryRules.Max(x => x.Price);
						}
					}

					return 0m;
				}
			}

			var commonDeliveryRules =
				CommonDistrictRuleItems.Where(x => comparerDeliveryPrice.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();

			if(commonDeliveryRules.Any())
			{
				var commonMinEShopGoodsSum = commonDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);

				if(eShopGoodsSum < commonMinEShopGoodsSum || commonMinEShopGoodsSum == 0)
				{
					return commonDeliveryRules.Max(x => x.Price);
				}
			}

			return 0m;
		}

		/// <summary>
		/// Активная ли версия районов, связанная с этим районом
		/// </summary>
		public virtual bool IsActive => DistrictsSet?.Status == DistrictsSetStatus.Active;

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(DistrictName))
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
			if(CommonDistrictRuleItems.Any(i => i.Price <= 0))
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
			if(TodayDeliveryScheduleRestrictions.Any(i => i.AcceptBefore == null))
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
				WaterPrice = WaterPrice,
				AllDistrictRuleItems = new List<DistrictRuleItemBase>(),
				AllDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>()
			};

			foreach(var districtRuleItem in AllDistrictRuleItems)
			{
				var newDistrictRuleItem = districtRuleItem.Clone();

				if(newDistrictRuleItem is WeekDayDistrictRuleItem newWeekDistrictRuleItem)
				{
					newWeekDistrictRuleItem.District = newDistrict;
					newDistrict.AllDistrictRuleItems.Add(newWeekDistrictRuleItem);
					continue;
				}

				if(newDistrictRuleItem is CommonDistrictRuleItem newCommonDistrictRuleItem)
				{
					newCommonDistrictRuleItem.District = newDistrict;
					newDistrict.AllDistrictRuleItems.Add(newCommonDistrictRuleItem);
					continue;
				}
			}

			foreach(var scheduleRestriction in AllDeliveryScheduleRestrictions)
			{
				var newScheduleRestriction = (DeliveryScheduleRestriction)scheduleRestriction.Clone();
				newScheduleRestriction.District = newDistrict;
				newDistrict.AllDeliveryScheduleRestrictions
					.Add(newScheduleRestriction);
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

		public static District GetDistrictFromActiveDistrictsSetOrNull(IUnitOfWork uow, District district)
		{
			int exceptNodeId = 0;

			while(district != null && district.CopyOf != null)
			{
				if(district.IsActive)
				{
					return district;
				}

				var activeCopiedDistrict =
					GetAllCopiedDistrictsByRoot(uow, district.Id, exceptNodeId)
					.Where(d => d.IsActive)
					.FirstOrDefault();

				if(activeCopiedDistrict != null)
				{
					return activeCopiedDistrict;
				}

				exceptNodeId = district.Id;
				district = district.CopyOf;
			}

			return null;
		}

		private static IEnumerable<District> GetAllCopiedDistrictsByRoot(IUnitOfWork uow, int rootNodeId, int exceptNodeId = 0)
		{
			foreach(var childGroup in GetDistrictsByParentId(uow, rootNodeId).Where(d => d.Id != exceptNodeId))
			{
				yield return childGroup;

				foreach(var nextLevelChildGroup in GetAllCopiedDistrictsByRoot(uow, childGroup.Id))
				{
					yield return nextLevelChildGroup;
				}
			}
		}

		private static IQueryable<District> GetDistrictsByParentId(IUnitOfWork uow, int rootId) =>
			uow.GetAll<District>().Where(g => g.CopyOf.Id == rootId);
	}
}
