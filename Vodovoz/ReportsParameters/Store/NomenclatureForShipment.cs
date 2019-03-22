using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ReportsParameters.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureForShipment : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public NomenclatureForShipment()
		{
			this.Build();
			ydatepicker.Date = DateTime.Now.Date.AddDays(1);
			ConfigureDlg();
			Destroyed += (sender, e) => UoW.Dispose();
		}

		void ConfigureDlg()
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по необходимым товарам для отгрузки";

		#endregion


		private ReportInfo GetReportInfo()
		{
			var repInfo = new ReportInfo {
				Identifier = "Store.NomenclatureForShipment",
				Parameters = new Dictionary<string, object>
				{
					{ "date", ydatepicker.Date.Date },
					{ "hideNorth", !checkbuttonN.Active },
					{ "hideSouth", !checkbuttonS.Active }
				}
			};
			return repInfo;
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e) => OnUpdate(true);
	}
}

