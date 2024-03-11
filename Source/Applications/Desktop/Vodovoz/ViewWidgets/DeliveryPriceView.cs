using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.ViewWidgets
{
	[ToolboxItem(true)]
	public partial class DeliveryPriceView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private const int _maxScheduleCountOnLine = 4;

		private DeliveryPriceNode _deliveryPrice;
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
				drrvToday.ConfigureDeliveryRulesTreeView(DeliveryRulesToday);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesMonday
		{
			get => _deliveryRulesMonday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesMonday = value;
				drrvMonday.Title = "Понедельник";
				drrvMonday.ConfigureDeliveryRulesTreeView(DeliveryRulesMonday);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesTuesday
		{
			get => _deliveryRulesTuesday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesTuesday = value;
				drrvTuesday.Title = "Вторник";
				drrvTuesday.ConfigureDeliveryRulesTreeView(DeliveryRulesTuesday);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesWednesday
		{
			get => _deliveryRulesWednesday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesWednesday = value;
				drrvWednesday.Title = "Среда";
				drrvWednesday.ConfigureDeliveryRulesTreeView(DeliveryRulesWednesday);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesThursday
		{
			get => _deliveryRulesThursday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesThursday = value;
				drrvThursday.Title = "Четверг";
				drrvThursday.ConfigureDeliveryRulesTreeView(DeliveryRulesThursday);
			}
		}


		public IList<DeliveryRuleRow> DeliveryRulesFriday
		{
			get => _deliveryRulesFriday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesFriday = value;
				drrvFriday.Title = "Пятница";
				drrvFriday.ConfigureDeliveryRulesTreeView(DeliveryRulesFriday);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesSaturday
		{
			get => _deliveryRulesSaturday ?? new List<DeliveryRuleRow>();
			set
			{
				_deliveryRulesSaturday = value;
				drrvSaturday.Title = "Суббота";
				drrvSaturday.ConfigureDeliveryRulesTreeView(DeliveryRulesSaturday);
			}
		}

		public IList<DeliveryRuleRow> DeliveryRulesSunday
		{
			get => _deliverySundayRules ?? new List<DeliveryRuleRow>();
			set
			{
				_deliverySundayRules = value;
				drrvSunday.Title = "Воскресенье";
				drrvSunday.ConfigureDeliveryRulesTreeView(DeliveryRulesSunday);
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

		private void ShowResults(DeliveryPriceNode deliveryPriceNode)
		{
			yTxtWarehouses.Buffer.Text = deliveryPriceNode.GeographicGroups;
			GtkScrolledWindow.Visible = deliveryPriceNode.ByDistance;
			ytreeviewPrices.SetItemsSource(deliveryPriceNode.Prices);
			lblDistrict.LabelProp = deliveryPriceNode.DistrictName;
			wageTypeValueLabel.Text = deliveryPriceNode.WageDistrict + ",";

			if(District?.Id != null)
			{
				RefreshDistrictData();
			}
		}

		private void RefreshDistrictData()
		{
			#region DeliveryRules

			DeliveryRulesToday =
				ConvertToDeliveryRuleRows(District.TodayDistrictRuleItems.Any()
				? District.TodayDistrictRuleItems
				: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

			DeliveryRulesMonday = ConvertToDeliveryRuleRows(
				District.MondayDistrictRuleItems.Any()
				? District.MondayDistrictRuleItems
				: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

			DeliveryRulesTuesday = ConvertToDeliveryRuleRows(
				District.TuesdayDistrictRuleItems.Any()
				? District.TuesdayDistrictRuleItems
				: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

			DeliveryRulesWednesday =
				ConvertToDeliveryRuleRows(District.WednesdayDistrictRuleItems.Any()
				? District.WednesdayDistrictRuleItems
				: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

			DeliveryRulesThursday =
				ConvertToDeliveryRuleRows(District.ThursdayDistrictRuleItems.Any()
				? District.ThursdayDistrictRuleItems
				: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

			DeliveryRulesFriday =
				ConvertToDeliveryRuleRows(District.FridayDistrictRuleItems.Any()
				? District.FridayDistrictRuleItems
				: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

			DeliveryRulesSaturday =
				ConvertToDeliveryRuleRows(District.SaturdayDistrictRuleItems.Any()
				? District.SaturdayDistrictRuleItems
				: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

			DeliveryRulesSunday =
				ConvertToDeliveryRuleRows(District.SundayDistrictRuleItems.Any()
				? District.SundayDistrictRuleItems
				: District.CommonDistrictRuleItems.Cast<DistrictRuleItemBase>());

			#endregion DeliveryRules

			#region Sheduules

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

		private string GetSheduleRestrictionsFor(WeekDayName weekDayName)
		{
			var restrictions = District
				.GetAllDeliveryScheduleRestrictions()
				.Where(x => x.WeekDay == weekDayName)
				.OrderBy(x => x.DeliverySchedule.From)
				.ThenBy(x => x.DeliverySchedule.To);

			var result = new StringBuilder();

			int i = 1;
			int? lastItemId = restrictions.LastOrDefault()?.Id;

			foreach(var restriction in restrictions)
			{
				result.Append(restriction.DeliverySchedule.Name);
				result.Append(restriction.Id == lastItemId ? ";" : ", ");
				if(i == _maxScheduleCountOnLine && restriction.Id != lastItemId)
				{
					result.AppendLine();
					i = 0;
				}
				i++;
			}

			return result.ToString();
		}

		private IList<DeliveryRuleRow> ConvertToDeliveryRuleRows(IEnumerable<DistrictRuleItemBase> weekDayDistrictRuleItems)
		{
			var deliveryRuleRows = new List<DeliveryRuleRow>();

			if(!weekDayDistrictRuleItems.Any())
			{
				return deliveryRuleRows;
			}

			var volumes = DeliveryPriceRule.Volumes;

			var deliveryRuleHeader = new DeliveryRuleRow
			{
				Volune = "Цена",
				DynamicColumns = new List<string>()
			};

			foreach(var weekDayDistrictRuleItem in weekDayDistrictRuleItems)
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

				foreach(var weekDayDistrictRuleItem in weekDayDistrictRuleItems)
				{
					deliveryRuleRow.DynamicColumns.Add(weekDayDistrictRuleItem.DeliveryPriceRule.GetVolumeValue(volume));
				}

				deliveryRuleRow.FreeDeliveryBottlesCount = deliveryRuleRow.DynamicColumns.Last();

				deliveryRuleRows.Add(deliveryRuleRow);
			}

			return deliveryRuleRows;
		}
	}
}
