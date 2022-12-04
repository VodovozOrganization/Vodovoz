using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	public partial class OrdersByDistrictReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public OrdersByDistrictReport(ReportFactory reportFactory)
		{
			this.Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();

			entryDistrict.SetEntityAutocompleteSelectorFactory(new EntityAutocompleteSelectorFactory<DistrictJournalViewModel>(typeof(District), () => {
				var filter = new DistrictJournalFilterViewModel { Status = DistrictsSetStatus.Active };
				return new DistrictJournalViewModel(filter, ServicesConfig.UnitOfWorkFactory, ServicesConfig.CommonServices) {
					EnableDeleteButton = false,
					EnableAddButton = false,
					EnableEditButton = false
				};
			}));
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
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
				parameters.Add("id_district", ((District)entryDistrict.Subject).Id);
			}

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = ReportName;
			reportInfo.UseUserVariables = true;
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			string errorString = string.Empty;
			if(entryDistrict.Subject == null && !checkAllDistrict.Active)
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
