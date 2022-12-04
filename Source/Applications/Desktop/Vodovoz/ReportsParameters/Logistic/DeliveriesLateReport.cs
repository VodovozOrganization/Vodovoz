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
		private readonly ReportFactory _reportFactory;

		public DeliveriesLateReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
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
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDate },
				{ "end_date", dateperiodpicker.EndDate.AddHours(3) },
				{ "is_driver_sort", ychkDriverSort.Active },
				{ "geographic_group_id", (ySpecCmbGeographicGroup.SelectedItem as GeoGroup)?.Id ?? 0 },
				{ "geographic_group_name", (ySpecCmbGeographicGroup.SelectedItem as GeoGroup)?.Name ?? "Все" },
				{ "exclude_truck_drivers_office_employees", ycheckExcludeTruckAndOfficeEmployees.Active },
				{ "select_mode", GetSelectMode().ToString() }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Logistic.DeliveriesLate";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			if (dateperiodpicker.StartDateOrNull == null) {
				MessageDialogWorks.RunErrorDialog ("Необходимо выбрать дату");
				return;
			}
			OnUpdate (true);
		}

		private SelectMode GetSelectMode()
		{
			if (ycheckOnlyFastSelect.Active)
			{
				return SelectMode.DeliveryInAnHour;
			}
			if (ycheckWithoutFastSelect.Active)
			{
				return SelectMode.WithoutDeliveryInAnHour;
			}
			return SelectMode.All;
		}

		private enum SelectMode
		{
			All,
			DeliveryInAnHour,
			WithoutDeliveryInAnHour
		}
	}
}
