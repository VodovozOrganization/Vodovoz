using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Report;
using QSReport;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReportDebtorsBottles : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public IUnitOfWork UoW { get; private set; }

		public ReportDebtorsBottles()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
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
			
			return new ReportInfo {
				Identifier = "Client.DebtorsBottles",
				Parameters = parameters
			};
		}
 

	}
}
