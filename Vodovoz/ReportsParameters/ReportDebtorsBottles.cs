using System;
using System.Collections.Generic;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Client;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReportDebtorsBottles : Gtk.Bin, IOrmDialog, IParametersWidget
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

		public object EntityObject {
			get {
				return null;
			}
		}

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
