using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;
using Vodovoz.Repositories.Sale;

namespace Vodovoz.ReportsParameters.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureForShipment : SingleUoWWidgetBase, IParametersWidget
	{
		public NomenclatureForShipment()
		{
			this.Build();
			ydatepicker.Date = DateTime.Today.AddDays(1);
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			lstGeoGrp.SetRenderTextFunc<GeographicGroup>(g => string.Format("{0}", g.Name));
			lstGeoGrp.ItemsList = GeographicGroupRepository.GeographicGroupsWithCoordinates(UoW);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по необходимым товарам для отгрузки";

		#endregion


		private ReportInfo GetReportInfo()
		{
			var repInfo = new ReportInfo {
				Identifier = "Store.GoodsToShipOnDate",
				Parameters = new Dictionary<string, object>
				{
					{ "date", ydatepicker.Date.ToString("yyyy-MM-dd") },
					{ "geo_group_id", (lstGeoGrp.SelectedItem as GeographicGroup)?.Id ?? 0 }
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

