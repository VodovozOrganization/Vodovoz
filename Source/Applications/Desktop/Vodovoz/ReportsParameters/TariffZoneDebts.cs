using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TariffZoneDebts : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public TariffZoneDebts(ReportFactory reportFactory)
		{
			this.Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			dateperiodpicker.StartDate = DateTime.Today.AddMonths(-1);
			dateperiodpicker.EndDate = DateTime.Today;

			yspeccomboboxTariffZone.SetRenderTextFunc<TariffZone>(x => x.Name);
			yspeccomboboxTariffZone.ItemsList = UoW.GetAll<TariffZone>().ToList();
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
		}


		#region IParametersWidget implementation

		public string Title => "Отчет по тарифным зонам";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			var selectedTariffZone = yspeccomboboxTariffZone.SelectedItem as TariffZone;
			if(selectedTariffZone == null) {
				MessageDialogHelper.RunWarningDialog("Необходимо выбрать тарифную зону");
				return;
			}
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{

			var parameters = new Dictionary<string, object>();

			parameters.Add("debt_from", yspinbuttonFrom.ValueAsInt);
			parameters.Add("debt_to", yspinbuttonTo.ValueAsInt);
			parameters.Add("date_from", dateperiodpicker.StartDate);
			parameters.Add("date_to", dateperiodpicker.EndDate);

			parameters.Add("tariff_zone_id", ((TariffZone)yspeccomboboxTariffZone.SelectedItem).Id);

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Client.TariffZoneDebts";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}
	}
}
