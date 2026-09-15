using Gamma.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Comparers;
using Vodovoz.Tools.Logistic;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.ViewWidgets
{
	[ToolboxItem(true)]
	public partial class DeliveryPriceView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private const int _maxScheduleCountOnLine = 4;

		private DeliveryPriceNode _deliveryPrice;
		private ServiceDistrict _serviceDistrict;
		private IList<DeliveryRuleRow> _deliveryRulesMonday;
		private IList<DeliveryRuleRow> _deliveryRulesTuesday;
		private IList<DeliveryRuleRow> _deliveryRulesWednesday;
		private IList<DeliveryRuleRow> _deliveryRulesThursday;
		private IList<DeliveryRuleRow> _deliveryRulesFriday;
		private IList<DeliveryRuleRow> _deliveryRulesSaturday;
		private IList<DeliveryRuleRow> _deliverySundayRules;
		private IList<DeliveryRuleRow> _deliveryRulesToday;
		private District _district;

		public DeliveryPriceView()
		{
			Build();
			//Отображается только у точек доставки без района
			ytreeviewPrices.CreateFluentColumnsConfig<DeliveryPriceRow>()
				.AddColumn("Количество").AddNumericRenderer(x => x.Amount)
				.AddColumn("Цена за бутыль").AddTextRenderer(x => x.Price)
				.Finish();
		}

		public District District
		{
			get => _district;
			set => _district = value;
		}

		public ServiceDistrict ServiceDistrict
		{
			get => _serviceDistrict;
			set => _serviceDistrict = value;
		}

		public DeliveryPriceNode DeliveryPrice
		{
			get
			{
				if(_deliveryPrice == null)
				{
					_deliveryPrice = new DeliveryPriceNode();
				}
				return _deliveryPrice;
			}
			set
			{
				_deliveryPrice = value;
				ShowResults(DeliveryPrice);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesToday
		{
			get => _deliveryRulesToday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesToday = value;
				drrvToday.Title = "Сегодня";
				drrvToday.ConfigureDeliveryRulesTreeView(DeliveryRulesToday, TypeOfAddress == OrderAddressType.Service);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesMonday
		{
			get => _deliveryRulesMonday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesMonday = value;
				drrvMonday.Title = "Понедельник";
				drrvMonday.ConfigureDeliveryRulesTreeView(DeliveryRulesMonday, TypeOfAddress == OrderAddressType.Service);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesTuesday
		{
			get => _deliveryRulesTuesday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesTuesday = value;
				drrvTuesday.Title = "Вторник";
				drrvTuesday.ConfigureDeliveryRulesTreeView(DeliveryRulesTuesday, TypeOfAddress == OrderAddressType.Service);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesWednesday
		{
			get => _deliveryRulesWednesday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesWednesday = value;
				drrvWednesday.Title = "Среда";
				drrvWednesday.ConfigureDeliveryRulesTreeView(DeliveryRulesWednesday, TypeOfAddress == OrderAddressType.Service);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesThursday
		{
			get => _deliveryRulesThursday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesThursday = value;
				drrvThursday.Title = "Четверг";
				drrvThursday.ConfigureDeliveryRulesTreeView(DeliveryRulesThursday, TypeOfAddress == OrderAddressType.Service);
			}
		}


		public IList<DeliveryRuleRow> DeliveryRulesFriday
		{
			get => _deliveryRulesFriday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesFriday = value;
				drrvFriday.Title = "Пятница";
				drrvFriday.ConfigureDeliveryRulesTreeView(DeliveryRulesFriday, TypeOfAddress == OrderAddressType.Service);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesSaturday
		{
			get => _deliveryRulesSaturday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesSaturday = value;
				drrvSaturday.Title = "Суббота";
				drrvSaturday.ConfigureDeliveryRulesTreeView(DeliveryRulesSaturday, TypeOfAddress == OrderAddressType.Service);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesSunday
		{
			get => _deliverySundayRules ?? new List<DeliveryRuleRow>();
			set
			{
				_deliverySundayRules = value;
				drrvSunday.Title = "Воскресенье";
				drrvSunday.ConfigureDeliveryRulesTreeView(DeliveryRulesSunday, TypeOfAddress == OrderAddressType.Service);
			}
		}

		public string ScheduleRestrictionsToday
		{
			get => drrvToday.Schedule;
			set => drrvToday.Schedule = value;
		}

		public string ScheduleRestrictionsMonday
		{
			get => drrvMonday.Schedule;
			set => drrvMonday.Schedule = value;
		}

		public string ScheduleRestrictionsTuesday
		{
			get => drrvTuesday.Schedule;
			set => drrvTuesday.Schedule = value;
		}

		public string ScheduleRestrictionsWednesday
		{
			get => drrvWednesday.Schedule;
			set => drrvWednesday.Schedule = value;
		}

		public string ScheduleRestrictionsThursday
		{
			get => drrvThursday.Schedule;
			set => drrvThursday.Schedule = value;
		}

		public string ScheduleRestrictionsFriday
		{
			get => drrvFriday.Schedule;
			set => drrvFriday.Schedule = value;
		}

		public string ScheduleRestrictionsSaturday
		{
			get => drrvSaturday.Schedule;
			set => drrvSaturday.Schedule = value;
		}

		public string ScheduleRestrictionsSunday
		{
			get => drrvSunday.Schedule;
			set => drrvSunday.Schedule = value;
		}
		public OrderAddressType? TypeOfAddress { get; internal set; }

		private void ShowResults(DeliveryPriceNode deliveryPriceNode)
		{
			yTxtWarehouses.Buffer.Text = deliveryPriceNode.GeographicGroups;
			GtkScrolledWindow.Visible = deliveryPriceNode.ByDistance;
			ytreeviewPrices.SetItemsSource(deliveryPriceNode.Prices);
			lblDistrict.LabelProp = deliveryPriceNode.DistrictName;
			if(deliveryPriceNode.WageDistrict != null)
			{
				wageTypeValueLabel.Text = deliveryPriceNode.WageDistrict + ",";
			}
			else
			{
				wageTypeValueLabel.Text = "Часть города: ";
			}

			if(TypeOfAddress == OrderAddressType.Service)
			{
				RefreshServiceDistrictData();
			}
			else if(District?.Id != null)
			{
				RefreshDistrictData();

			}
			else
			{
				HideDistrictsData();
			}
		}

		private void RefreshDistrictData()
		{
			#region DeliveryRules

			if(District
				.GetAllDeliveryScheduleRestrictions()
				.Where(x => x.WeekDay == WeekDayName.Today).Any())
			{
				DeliveryRulesToday = ConvertToDeliveryRuleRows(
					District.TodayDistrictRuleItems.Any()
					? District.TodayDistrictRuleItems
					: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

				drrvToday.Visible = true;
			}
			else
			{
				drrvToday.Visible = false;
			}

			if(District
				.GetAllDeliveryScheduleRestrictions()
				.Where(x => x.WeekDay == WeekDayName.Monday).Any())
			{
				DeliveryRulesMonday = ConvertToDeliveryRuleRows(
					District.MondayDistrictRuleItems.Any()
					? District.MondayDistrictRuleItems
					: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

				drrvMonday.Visible = true;
			}
			else
			{
				drrvMonday.Visible = false;
			}

			if(District
				.GetAllDeliveryScheduleRestrictions()
				.Where(x => x.WeekDay == WeekDayName.Tuesday).Any())
			{
				DeliveryRulesTuesday = ConvertToDeliveryRuleRows(
					District.TuesdayDistrictRuleItems.Any()
					? District.TuesdayDistrictRuleItems
					: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

				drrvTuesday.Visible = true;
			}
			else
			{
				drrvTuesday.Visible = false;
			}

			if(District
				.GetAllDeliveryScheduleRestrictions()
				.Where(x => x.WeekDay == WeekDayName.Wednesday).Any())
			{
				DeliveryRulesWednesday = ConvertToDeliveryRuleRows(
					District.WednesdayDistrictRuleItems.Any()
					? District.WednesdayDistrictRuleItems
					: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

				drrvWednesday.Visible = true;
			}
			else
			{
				drrvWednesday.Visible = false;
			}

			if(District
				.GetAllDeliveryScheduleRestrictions()
				.Where(x => x.WeekDay == WeekDayName.Thursday).Any())
			{
				DeliveryRulesThursday = ConvertToDeliveryRuleRows(
					District.ThursdayDistrictRuleItems.Any()
					? District.ThursdayDistrictRuleItems
					: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

				drrvThursday.Visible = true;
			}
			else
			{
				drrvThursday.Visible = false;
			}

			if(District
				.GetAllDeliveryScheduleRestrictions()
				.Where(x => x.WeekDay == WeekDayName.Friday).Any())
			{
				DeliveryRulesFriday = ConvertToDeliveryRuleRows(
					District.FridayDistrictRuleItems.Any()
					? District.FridayDistrictRuleItems
					: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

				drrvFriday.Visible = true;
			}
			else
			{
				drrvFriday.Visible = false;
			}

			if(District
				.GetAllDeliveryScheduleRestrictions()
				.Where(x => x.WeekDay == WeekDayName.Saturday).Any())
			{
				DeliveryRulesSaturday = ConvertToDeliveryRuleRows(
					District.SaturdayDistrictRuleItems.Any()
					? District.SaturdayDistrictRuleItems
					: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

				drrvSaturday.Visible = true;
			}
			else
			{
				drrvSaturday.Visible = false;
			}

			if(District
				.GetAllDeliveryScheduleRestrictions()
				.Where(x => x.WeekDay == WeekDayName.Sunday).Any())
			{
				DeliveryRulesSunday = ConvertToDeliveryRuleRows(
					District.SundayDistrictRuleItems.Any()
					? District.SundayDistrictRuleItems
					: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

				drrvSunday.Visible = true;
			}
			else
			{
				drrvSunday.Visible = false;
			}

			#endregion DeliveryRules

			#region Shedules

			ScheduleRestrictionsToday = GetSheduleRestrictionsFor(WeekDayName.Today);
			ScheduleRestrictionsMonday = GetSheduleRestrictionsFor(WeekDayName.Monday);
			ScheduleRestrictionsTuesday = GetSheduleRestrictionsFor(WeekDayName.Tuesday);
			ScheduleRestrictionsWednesday = GetSheduleRestrictionsFor(WeekDayName.Wednesday);
			ScheduleRestrictionsThursday = GetSheduleRestrictionsFor(WeekDayName.Thursday);
			ScheduleRestrictionsFriday = GetSheduleRestrictionsFor(WeekDayName.Friday);
			ScheduleRestrictionsSaturday = GetSheduleRestrictionsFor(WeekDayName.Saturday);
			ScheduleRestrictionsSunday = GetSheduleRestrictionsFor(WeekDayName.Sunday);

			#endregion Shedules
		}

		private void RefreshServiceDistrictData()
		{
			#region DeliveryRules

			foreach(var weekDay in Enum.GetValues(typeof(WeekDayName)).Cast<WeekDayName>())
			{
				if(ServiceDistrict?.GetWeekDayRulesByWeekDayName(weekDay) is IList<WeekDayServiceDistrictRule> dayServiceDistrictRule)
				{
					var dayRule = ConvertToServiceDeliveryRuleRows(dayServiceDistrictRule.Any()
						? dayServiceDistrictRule
						: ServiceDistrict?.GetCommonServiceDistrictRules() as IEnumerable<ServiceDistrictRule>);

					var isDayVisible = dayRule.Any() && GetServiceSheduleRestrictionsForDay(weekDay).Any(); //Удали меня

					switch(weekDay)
					{
						case WeekDayName.Today:
							DeliveryRulesToday = dayRule;
							drrvToday.Visible = isDayVisible;
							break;
						case WeekDayName.Monday:
							DeliveryRulesMonday = dayRule;
							drrvMonday.Visible = isDayVisible;
							break;
						case WeekDayName.Tuesday:
							DeliveryRulesTuesday = dayRule;
							drrvTuesday.Visible = isDayVisible;
							break;
						case WeekDayName.Wednesday:
							DeliveryRulesWednesday = dayRule;
							drrvWednesday.Visible = isDayVisible;
							break;
						case WeekDayName.Thursday:
							DeliveryRulesThursday = dayRule;
							drrvThursday.Visible = isDayVisible;
							break;
						case WeekDayName.Friday:
							DeliveryRulesFriday = dayRule;
							drrvFriday.Visible = isDayVisible;
							break;
						case WeekDayName.Saturday:
							DeliveryRulesSaturday = dayRule;
							drrvSaturday.Visible = isDayVisible;
							break;
						case WeekDayName.Sunday:
							DeliveryRulesSunday = dayRule;
							drrvSunday.Visible = isDayVisible;
							break;
					}
				}
			}

			#endregion DeliveryRules

			#region Sheduules

			ScheduleRestrictionsToday = GetServiceSheduleRestrictionsForDay(WeekDayName.Today);
			ScheduleRestrictionsMonday = GetServiceSheduleRestrictionsForDay(WeekDayName.Monday);
			ScheduleRestrictionsTuesday = GetServiceSheduleRestrictionsForDay(WeekDayName.Tuesday);
			ScheduleRestrictionsWednesday = GetServiceSheduleRestrictionsForDay(WeekDayName.Wednesday);
			ScheduleRestrictionsThursday = GetServiceSheduleRestrictionsForDay(WeekDayName.Thursday);
			ScheduleRestrictionsFriday = GetServiceSheduleRestrictionsForDay(WeekDayName.Friday);
			ScheduleRestrictionsSaturday = GetServiceSheduleRestrictionsForDay(WeekDayName.Saturday);
			ScheduleRestrictionsSunday = GetServiceSheduleRestrictionsForDay(WeekDayName.Sunday);

			#endregion Shedules			
		}

		private string GetSheduleRestrictionsFor(WeekDayName weekDayName, bool isForServiceDistrict = false)
			{

			var restrictions = District.GetAllDeliveryScheduleRestrictions()
				.Where(x => x.WeekDay == weekDayName)
				.OrderBy(x => x.DeliverySchedule.From)
				.ThenBy(x => x.DeliverySchedule.To);

			var result = new StringBuilder();

			int i = 1;

			if(weekDayName == WeekDayName.Today)
			{
				var groupedTodayRestrictions = restrictions
					.GroupBy(x => x.AcceptBefore?.Name)
					.OrderBy(x => x.Key, new StringOrNullAfterComparer());

				foreach(var group in groupedTodayRestrictions)
				{
					if(!string.IsNullOrWhiteSpace(group.Key))
					{
						result.Append($"<b>до {group.Key}:</b> ");
					}
					else
					{
						result.Append($"<b>Без ограничений:</b> ");
					}

					i = 1;
					int maxScheduleCountOnLine = 3;
					var restrictionsInGroup = group.ToList();
					int lastItemOnDayId = restrictionsInGroup.Last().Id;
					foreach(var restriction in restrictionsInGroup)
					{
						result.Append(restriction.DeliverySchedule.Name);
						result.Append(restriction.Id == lastItemOnDayId ? ";" : ", ");
						if(i == maxScheduleCountOnLine && restriction.Id != lastItemOnDayId)
						{
							result.AppendLine();
							maxScheduleCountOnLine = _maxScheduleCountOnLine;
							i = 0;
						}
						i++;
					}
					result.AppendLine();
				}

				return result.ToString();
			}

			var groupedRestrictions = restrictions
				.GroupBy(x => x.AcceptBefore?.Name)
				.OrderBy(x => x.Key, new StringOrNullAfterComparer());

			foreach(var group in groupedRestrictions)
			{
				if(!string.IsNullOrWhiteSpace(group.Key))
				{
					result.Append($"<b>до {group.Key} (предыдущего дня):</b> ");
				}
				else
				{
					result.Append($"<b>Без ограничений:</b> ");
				}

				i = 1;
				int maxScheduleCountOnLine = 3;
				var restrictionsInGroup = group.ToList();
				int lastItemOnDayId = restrictionsInGroup.Last().Id;
				foreach(var restriction in restrictionsInGroup)
				{
					result.Append(restriction.DeliverySchedule.Name);
					result.Append(restriction.Id == lastItemOnDayId ? ";" : ", ");
					if(i == maxScheduleCountOnLine && restriction.Id != lastItemOnDayId)
					{
						result.AppendLine();
						maxScheduleCountOnLine = _maxScheduleCountOnLine;
						i = 0;
					}
					i++;
				}
				result.AppendLine();
			}

			return result.ToString();
		}

		private string GetServiceSheduleRestrictionsForDay(WeekDayName weekDayName)
		{
			if(ServiceDistrict is null)
			{
				return string.Empty;
			}

			var restrictions = ServiceDistrict.GetServiceScheduleRestrictionsByWeekDay(weekDayName);

			var result = new StringBuilder();

			int i = 1;

			if(weekDayName == WeekDayName.Today)
			{
				var groupedTodayRestrictions = restrictions
					.GroupBy(x => x.AcceptBefore?.Name)
					.OrderBy(x => x.Key, new StringOrNullAfterComparer());

				foreach(var group in groupedTodayRestrictions)
				{
					if(!string.IsNullOrWhiteSpace(group.Key))
					{
						result.Append($"<b>до {group.Key}:</b> ");
					}
					else
					{
						result.Append($"<b>Без ограничений:</b> ");
					}

					int maxScheduleCountOnLine = 3;
					var restrictionsInGroup = group.ToList();
					int lastItemOnDayId = restrictionsInGroup.Last().Id;
					foreach(var restriction in restrictionsInGroup)
					{
						result.Append(restriction.DeliverySchedule.Name);
						result.Append(restriction.Id == lastItemOnDayId ? ";" : ", ");
						if(i == maxScheduleCountOnLine && restriction.Id != lastItemOnDayId)
						{
							result.AppendLine();
							maxScheduleCountOnLine = _maxScheduleCountOnLine;
							i = 0;
						}
						i++;
					}
					result.AppendLine();
				}

				return result.ToString();
			}

			var groupedRestrictions = restrictions
				.GroupBy(x => x.AcceptBefore?.Name)
				.OrderBy(x => x.Key, new StringOrNullAfterComparer());

			foreach(var group in groupedRestrictions)
			{
				if(!string.IsNullOrWhiteSpace(group.Key))
				{
					result.Append($"<b>до {group.Key} (предыдущего дня):</b> ");
				}
				else
				{
					result.Append($"<b>Без ограничений:</b> ");
				}

				int maxScheduleCountOnLine = 3;
				var restrictionsInGroup = group.ToList();
				int lastItemOnDayId = restrictionsInGroup.Last().Id;
				foreach(var restriction in restrictionsInGroup)
				{
					result.Append(restriction.DeliverySchedule.Name);
					result.Append(restriction.Id == lastItemOnDayId ? ";" : ", ");
					if(i == maxScheduleCountOnLine && restriction.Id != lastItemOnDayId)
					{
						result.AppendLine();
						maxScheduleCountOnLine = _maxScheduleCountOnLine;
						i = 0;
					}
					i++;
				}
				result.AppendLine();
			}

			return result.ToString();
		}

		private IList<DeliveryRuleRow> ConvertToDeliveryRuleRows(IEnumerable<DistrictRuleItemBase> weekDayDistrictRuleItems)
		{
			var sortedByBottlesCountRules = weekDayDistrictRuleItems.OrderBy(x => x.DeliveryPriceRule.Water19LCount).ToList();

			var deliveryRuleRows = new List<DeliveryRuleRow>();

			if(!weekDayDistrictRuleItems.Any())
			{
				return deliveryRuleRows;
			}

			var volumes = DeliveryPriceRule.Volumes;

			var deliveryRuleHeader = new DeliveryRuleRow
			{
				Volune = "Цена\nдоставки",
				DynamicColumns = new List<string>()
			};

			foreach(var weekDayDistrictRuleItem in sortedByBottlesCountRules)
			{
				deliveryRuleHeader.DynamicColumns.Add(weekDayDistrictRuleItem.Price.ToString());
			}

			deliveryRuleRows.Add(deliveryRuleHeader);

			foreach(var volume in volumes)
			{
				var deliveryRuleRow = new DeliveryRuleRow
				{
					Volune = volume,
					DynamicColumns = new List<string>()
				};

				foreach(var weekDayDistrictRuleItem in sortedByBottlesCountRules)
				{
					deliveryRuleRow.DynamicColumns.Add(weekDayDistrictRuleItem.DeliveryPriceRule.GetVolumeValue(volume));
				}

				deliveryRuleRow.FreeDeliveryBottlesCount = deliveryRuleRow.DynamicColumns.Last();

				deliveryRuleRows.Add(deliveryRuleRow);
			}

			return deliveryRuleRows;
		}

		private IList<DeliveryRuleRow> ConvertToServiceDeliveryRuleRows(IEnumerable<ServiceDistrictRule> weekDayDistrictRuleItems)
		{
			var sortedByBottlesCountRules = weekDayDistrictRuleItems;

			var deliveryRuleRows = new List<DeliveryRuleRow>();

			if(!weekDayDistrictRuleItems.Any())
			{
				return deliveryRuleRows;
			}

			var volumes = DeliveryPriceRule.Volumes;

			var deliveryRuleHeader = new DeliveryRuleRow
			{
				DynamicColumns = new List<string>()
			};

			deliveryRuleHeader.DynamicColumns.Add("Тип сервисной доставки");
			deliveryRuleHeader.DynamicColumns.Add("Цена");

			deliveryRuleRows.Add(deliveryRuleHeader);

			foreach(var weekDayDistrictRuleItem in weekDayDistrictRuleItems)
			{
				var deliveryRuleRow = new DeliveryRuleRow
				{
					DynamicColumns = new List<string>
					{
						weekDayDistrictRuleItem.ServiceType.GetEnumTitle(),
						weekDayDistrictRuleItem.Price.ToString()
					}
				};

				deliveryRuleRows.Add(deliveryRuleRow);
			}

			return deliveryRuleRows;
		}

		private void HideDistrictsData()
		{
			drrvToday.Visible = false;
			drrvMonday.Visible = false;
			drrvTuesday.Visible = false;
			drrvWednesday.Visible = false;
			drrvThursday.Visible = false;
			drrvFriday.Visible = false;
			drrvSaturday.Visible = false;
			drrvSunday.Visible = false;
		}
	}
}
