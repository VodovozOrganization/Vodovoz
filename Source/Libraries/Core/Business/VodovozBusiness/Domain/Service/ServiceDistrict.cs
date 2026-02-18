using NetTopologySuite.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Extensions;

namespace VodovozBusiness.Domain.Service
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "сервисные районы",
		Nominative = "сервисный район")]
	[EntityPermission]
	[HistoryTrace]
	public class ServiceDistrict : BusinessObjectBase<ServiceDistrict>, IDomainObject, IValidatableObject, ICloneable
	{
		private string _serviceDistrictName;
		private Geometry _serviceDistrictBorder;
		private GeoGroup _geographicGroup;
		private ServiceDistrictsSet _districtsSet;
		private ServiceDistrict _copyOf;

		private IList<ServiceDistrictCopyItem> _districtCopyItems = new List<ServiceDistrictCopyItem>();

		private IObservableList<ServiceDeliveryScheduleRestriction> _allDeliveryScheduleRestrictions = new ObservableList<ServiceDeliveryScheduleRestriction>();
		private IObservableList<ServiceDistrictRule> _allDistrictRules = new ObservableList<ServiceDistrictRule>();

		public virtual int Id { get; set; }

		[Display(Name = "Название района")]
		public virtual string ServiceDistrictName
		{
			get => _serviceDistrictName;
			set => SetField(ref _serviceDistrictName, value);
		}

		[Display(Name = "Граница")]
		public virtual Geometry ServiceDistrictBorder
		{
			get => _serviceDistrictBorder;
			set => SetField(ref _serviceDistrictBorder, value);
		}

		[Display(Name = "Часть города")]
		public virtual GeoGroup GeographicGroup
		{
			get => _geographicGroup;
			set => SetField(ref _geographicGroup, value);
		}

		[Display(Name = "Версия сервисных районов")]
		public virtual ServiceDistrictsSet ServiceDistrictsSet
		{
			get => _districtsSet;
			set => SetField(ref _districtsSet, value);
		}

		[Display(Name = "Копия района")]
		public virtual ServiceDistrict CopyOf
		{
			get => _copyOf;
			set => SetField(ref _copyOf, value);
		}

		[Display(Name = "Районы, в которые был скопирован данный район")]
		public virtual IList<ServiceDistrictCopyItem> ServiceDistrictCopyItems
		{
			get => _districtCopyItems;
			set => SetField(ref _districtCopyItems, value);
		}

		[Display(Name = "Правила и цены доставки района")]
		public virtual IObservableList<ServiceDistrictRule> AllServiceDistrictRules
		{
			get => _allDistrictRules;
			set => SetField(ref _allDistrictRules, value);
		}

		private IEnumerable<ServiceDeliveryScheduleRestriction> GetDeliveryScheduleRestrictionsByDeliveryDate(DateTime? deliveryDate)
		{
			if(deliveryDate == null)
			{
				return new List<ServiceDeliveryScheduleRestriction>();
			}

			if(deliveryDate.Value == DateTime.Today)
			{
				return GetServiceScheduleRestrictionsByWeekDay(WeekDayName.Today);
			}

			return GetServiceScheduleRestrictionsByWeekDay(deliveryDate.Value.DayOfWeek.ConvertToWeekDayName());
		}

		public virtual IEnumerable<ServiceDistrictRule> GetWeekDayServiceDistrictRuleByDeliveryDate(DateTime? deliveryDate)
		{
			if(deliveryDate == null)
			{
				return new List<ServiceDistrictRule>();
			}

			var serviceDistrictRules = deliveryDate.Value == DateTime.Today
				? GetWeekDayRulesByWeekDayName(WeekDayName.Today)
				: GetWeekDayRulesByWeekDayName(deliveryDate.Value.DayOfWeek.ConvertToWeekDayName());

			return serviceDistrictRules;
		}


		public virtual IList<ServiceDeliveryScheduleRestriction> GetServiceScheduleRestrictionsByWeekDay(WeekDayName weekDayName)
		{
			return AllServiceDeliveryScheduleRestrictions.Where(x => x.WeekDay == weekDayName).ToList();
		}

		public virtual IObservableList<ServiceDeliveryScheduleRestriction> AllServiceDeliveryScheduleRestrictions
		{
			get => _allDeliveryScheduleRestrictions;
			set => SetField(ref _allDeliveryScheduleRestrictions, value);
		}

		public virtual string Title => ServiceDistrictName;

		public virtual IEnumerable<ServiceDeliveryScheduleRestriction> GetAvailableServiceDeliveryScheduleRestrictionsByDeliveryDate(
			DateTime? deliveryDate)
		{
			if(deliveryDate == null)
			{
				return new List<ServiceDeliveryScheduleRestriction>();
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

		public virtual IEnumerable<ServiceDeliveryScheduleRestriction> GetAllDeliveryScheduleRestrictions()
		{
			return AllServiceDeliveryScheduleRestrictions;
		}

		public virtual IList<WeekDayServiceDistrictRule> GetWeekDayRulesByWeekDayName(WeekDayName weekDayName) =>
			AllServiceDistrictRules.Where(x => x is WeekDayServiceDistrictRule)
				.Cast<WeekDayServiceDistrictRule>()
				.Where(x => x.WeekDay == weekDayName)
				.ToList();

		public virtual IList<CommonServiceDistrictRule> GetCommonServiceDistrictRules() =>
			AllServiceDistrictRules.Where(x => x is CommonServiceDistrictRule)
			.Cast<CommonServiceDistrictRule>()
			.ToList();

		public virtual void ReplaceServiceDistrictDeliveryScheduleRestrictions(IEnumerable<ServiceDeliveryScheduleRestriction> serviceDeliveryScheduleRestrictions)
		{
			if(serviceDeliveryScheduleRestrictions == null)
			{
				throw new ArgumentException(
					 "Отсутствуют данные новых графиков доставки");
			}

			if(serviceDeliveryScheduleRestrictions.Any(s => s.ServiceDistrict.Id != Id))
			{
				throw new ArgumentException(
					 "Id района в который добавляется график доставки должен совпадать с Id района в новом графике доставки");
			}

			AllServiceDeliveryScheduleRestrictions.Clear();

			foreach(var schedule in serviceDeliveryScheduleRestrictions)
			{
				AllServiceDeliveryScheduleRestrictions.Add(schedule);
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

				var serviceDeliveryScheduleRestrictions = GetAvailableServiceDeliveryScheduleRestrictionsByDeliveryDate(date);

				if(serviceDeliveryScheduleRestrictions.Count() > 0)
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

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(ServiceDistrictName))
			{
				yield return new ValidationResult(
					"Необходимо заполнить имя района",
					new[] { nameof(ServiceDistrictName) }
				);
			}

			if(GeographicGroup == null)
			{
				yield return new ValidationResult(
					$"Для района \"{ServiceDistrictName}\" необходимо указать часть города, содержащую этот район доставки",
					new[] { nameof(GeographicGroup) }
				);
			}
			if(ServiceDistrictBorder == null)
			{
				yield return new ValidationResult(
					$"Для района \"{ServiceDistrictName}\" необходимо нарисовать границы на карте",
					new[] { nameof(ServiceDistrictBorder) }
				);
			}

			if(GetServiceScheduleRestrictionsByWeekDay(WeekDayName.Today).Any(i => i.AcceptBefore == null))
			{
				yield return new ValidationResult(
					$"Для графиков доставки \"день в день\" для района \"{ServiceDistrictName}\" должно быть указано время приема до"
				);
			}
		}

		#endregion

		#region ICloneable implementation

		public virtual object Clone()
		{
			var newServiceDistrict = new ServiceDistrict
			{
				ServiceDistrictName = ServiceDistrictName,
				ServiceDistrictBorder = ServiceDistrictBorder?.Copy(),
				GeographicGroup = GeographicGroup,
				AllServiceDistrictRules = new ObservableList<ServiceDistrictRule>(),
				AllServiceDeliveryScheduleRestrictions = new ObservableList<ServiceDeliveryScheduleRestriction>()
			};


			foreach(var serviceDistrictRule in AllServiceDistrictRules)
			{
				var newServiceDistrictRuleItem = serviceDistrictRule.Clone();

				if(newServiceDistrictRuleItem is WeekDayServiceDistrictRule newWeekServiceDistrictRuleItem)
				{
					newWeekServiceDistrictRuleItem.ServiceDistrict = newServiceDistrict;
					newServiceDistrict.AllServiceDistrictRules.Add(newWeekServiceDistrictRuleItem);
					continue;
				}

				if(newServiceDistrictRuleItem is CommonServiceDistrictRule newCommonServiceDistrictRule)
				{
					newCommonServiceDistrictRule.ServiceDistrict = newServiceDistrict;
					newServiceDistrict.AllServiceDistrictRules.Add(newCommonServiceDistrictRule);
					continue;
				}
			}

			foreach(var scheduleRestriction in AllServiceDeliveryScheduleRestrictions)
			{
				var newScheduleRestriction = (ServiceDeliveryScheduleRestriction)scheduleRestriction.Clone();
				newScheduleRestriction.ServiceDistrict = newServiceDistrict;
				newServiceDistrict.AllServiceDeliveryScheduleRestrictions
					.Add(newScheduleRestriction);
			}

			return newServiceDistrict;
		}

		#endregion ICloneable implementation
	}
}
