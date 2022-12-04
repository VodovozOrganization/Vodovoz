using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class FuelConsumptionReport : SingleUoWWidgetBase, IParametersWidget
	{
		GenericObservableList<GeoGroup> geographicGroups;
		private readonly ReportFactory _reportFactory;

		public FuelConsumptionReport(ReportFactory reportFactory, bool orderById = false)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			geographicGroup.UoW = UoW;
			geographicGroup.Label = "Часть города:";
			geographicGroups = new GenericObservableList<GeoGroup>();
			geographicGroup.Items = geographicGroups;
			foreach(var gg in UoW.Session.QueryOver<GeoGroup>().List())
				geographicGroups.Add(gg);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по выдаче топлива по МЛ";

		#endregion

		private int[] GetResultIds(IEnumerable<int> ids)
		{
			return ids.Any() ? ids.ToArray() : new int[] { 0 };
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{ "geo_group_ids", GetResultIds(geographicGroups.Select(g => g.Id)) }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = chkDetailed.Active ? "Logistic.FuelConsumptionDetailedReport" : "Logistic.FuelConsumptionReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDateOrNull == null) {
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать дату");
				return;
			}
			OnUpdate(true);
		}
	}
}
