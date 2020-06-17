using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ReportsParameters
{
	public partial class OrdersByDistrictReport : SingleUoWWidgetBase, IParametersWidget
	{
		public OrdersByDistrictReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			refDistrict.SubjectType = typeof(District);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по районам";

		#endregion

		private ReportInfo GetReportInfo()
		{
			string ReportName;
			var parameters = new Dictionary<string, object> {
				{ "start_date", dateperiodpicker.StartDate },
				{ "end_date", dateperiodpicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) }
			};

			if(checkAllDistrict.Active) {
				ReportName = "Orders.OrdersByAllDistrict";
			} else {
				ReportName = "Orders.OrdersByDistrict";
				parameters.Add("id_district", ((District)refDistrict.Subject).Id);
			}

			return new ReportInfo {
				Identifier = ReportName,
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			string errorString = string.Empty;
			if(refDistrict.Subject == null && !checkAllDistrict.Active)
				errorString += "Не заполнен район\n";
			if(dateperiodpicker.StartDateOrNull == null)
				errorString += "Не заполнена дата\n";
			if(!string.IsNullOrWhiteSpace(errorString)) {
				MessageDialogHelper.RunErrorDialog(errorString);
				return;
			}
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		void CanRun()
		{
			buttonCreateReport.Sensitive = dateperiodpicker.EndDateOrNull.HasValue && dateperiodpicker.StartDateOrNull.HasValue;
		}

		protected void OnDateperiodpickerPeriodChanged(object sender, EventArgs e)
		{
			CanRun();
		}
	}
}
