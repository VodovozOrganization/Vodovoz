using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSProjectsLib;
using QSReport;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Reports.Logistic
{
	public partial class DeliveriesLateReport : SingleUoWWidgetBase, IParametersWidget
	{
		public DeliveriesLateReport ()
		{
			this.Build ();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ySpecCmbGeographicGroup.ItemsList = UoW.GetAll<GeoGroup>();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по опозданиям";

		#endregion

		private void OnUpdate (bool hide = false)
		{
			LoadReport?.Invoke (this, new LoadReportEventArgs (GetReportInfo (), hide));
		}

		private ReportInfo GetReportInfo ()
		{
			return new ReportInfo {
				Identifier = "Logistic.DeliveriesLate",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDate },
					{ "end_date", dateperiodpicker.EndDate.AddHours(3) },
					{ "is_driver_sort", ychkDriverSort.Active },
					{ "geographic_group_id", (ySpecCmbGeographicGroup.SelectedItem as GeoGroup)?.Id ?? 0 },
					{ "geographic_group_name", (ySpecCmbGeographicGroup.SelectedItem as GeoGroup)?.Name ?? "Все" },
					{ "exclude_truck_drivers_office_employees", ycheckExcludeTruckAndOfficeEmployees.Active },
					{ "order_select_mode", GetOrderSelectMode().ToString() },
					{ "interval_select_mode", GetIntervalSelectMode().ToString() },
				}
			};
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			if (dateperiodpicker.StartDateOrNull == null) {
				MessageDialogWorks.RunErrorDialog ("Необходимо выбрать дату");
				return;
			}
			OnUpdate (true);
		}

		private OrderSelectMode GetOrderSelectMode()
		{
			if (ycheckOnlyFastSelect.Active)
			{
				return OrderSelectMode.DeliveryInAnHour;
			}
			if (ycheckWithoutFastSelect.Active)
			{
				return OrderSelectMode.WithoutDeliveryInAnHour;
			}
			return OrderSelectMode.All;
		}

		private enum OrderSelectMode
		{
			All,
			DeliveryInAnHour,
			WithoutDeliveryInAnHour
		}

		private IntervalSelectMode GetIntervalSelectMode => ycheckIntervalFromCreateTime.Active ? IntervalSelectMode.Create : IntervalSelectMode.Transfer;
		
		private enum IntervalSelectMode
		{
			Create,
			Transfer
		}
	}
}
