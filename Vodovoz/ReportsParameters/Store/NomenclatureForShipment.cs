using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Sale;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.Repositories.Sale;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ReportsParameters.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureForShipment : SingleUoWWidgetBase, IParametersWidget
	{
		SelectableParametersReportFilter filter;

		public NomenclatureForShipment()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			filter = new SelectableParametersReportFilter(UoW);
			ydatepicker.Date = DateTime.Today.AddDays(1);
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			lstGeoGrp.SetRenderTextFunc<GeographicGroup>(g => string.Format("{0}", g.Name));
			lstGeoGrp.ItemsList = GeographicGroupRepository.GeographicGroupsWithCoordinates(UoW);

			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			filter.CreateParameterSet(
				"Группы товаров",
				"product_group",
				new RecursiveParametersFactory<ProductGroup>(UoW,
				(filters) => {
					var query = UoW.Session.QueryOver<ProductGroup>();
					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							query.Where(f());
						}
					}
					return query.List();
				},
				x => x.Name,
				x => x.Childs)
			);

			var viewModel = new SelectableParameterReportFilterViewModel(filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по необходимым товарам для отгрузки";

		#endregion


		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object> 
			{ 
					{ "date", ydatepicker.Date.ToString("yyyy-MM-dd") },
					{ "geo_group_id", (lstGeoGrp.SelectedItem as GeographicGroup)?.Id ?? 0 },
					{ "creation_date", DateTime.Now}
			};
			foreach(var item in filter.GetParameters()) {
				parameters.Add(item.Key, item.Value);

			}

			var repInfo = new ReportInfo {
				Identifier = "Store.GoodsToShipOnDate",
				Parameters = parameters
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

