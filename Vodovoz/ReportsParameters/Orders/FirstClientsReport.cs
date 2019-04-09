using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Store;
using Vodovoz.Repositories.Orders;
using System.Linq;
using NHibernate.Util;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ReportsParameters.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FirstClientsReport : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public IUnitOfWork UoW { get; private set; }

		public FirstClientsReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			var reasons = OrderRepository.GetDiscountReasons(UoW);
			yCpecCmbDiscountReason.ItemsList = reasons;
			yCpecCmbDiscountReason.SelectedItem = reasons.FirstOrDefault(r => r.Id == 16);
			datePeriodPicker.StartDate = datePeriodPicker.EndDate = DateTime.Today;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по не полностью погруженным МЛ";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var reportInfo = new ReportInfo {
				Identifier = "Orders.FirstClients",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", datePeriodPicker.StartDateOrNull.Value },
					{ "end_date", datePeriodPicker.EndDateOrNull.Value },
					{ "discount_id", (yCpecCmbDiscountReason.SelectedItem as DiscountReason)?.Id ?? 0}
				}
			};
			return reportInfo;
		}

		protected void OnDatePeriodPickerPeriodChanged(object sender, EventArgs e)
		{
			SetSensitivity();
		}

		private void SetSensitivity()
		{
			var datePeriodSelected = datePeriodPicker.EndDateOrNull.HasValue && datePeriodPicker.StartDateOrNull.HasValue;
			buttonRun.Sensitive = datePeriodSelected;
		}
	}
}
