using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReportDebtorsBottles : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public ReportDebtorsBottles(ReportFactory reportFactory)
		{
			this.Build();
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
		}

		#region IParametersWidget implementation

		public string Title {
			get {
				return "Отчет по должникам тары";
			}
		}

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
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();
			if(radiobuttonAllShow.Active){
				parameters.Clear();
				parameters.Add("allshow", "1");
				parameters.Add("withresidue", "-1");
			}
				
			if(radiobuttonNotManualEntered.Active){
				parameters.Clear();
				parameters.Add("allshow", "0");
				parameters.Add("withresidue", "0");
			}
				
			if(radiobuttonOnlyManualEntered.Active){
				parameters.Clear();
				parameters.Add("allshow", "0");
				parameters.Add("withresidue", "1");
			}

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Client.DebtorsBottles";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}
 

	}
}
