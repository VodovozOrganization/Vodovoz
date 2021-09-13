using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using MoreLinq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderStatisticByWeekReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly GenericObservableList<GeographicGroupNode> _geographicGroupNodes;

        public OrderStatisticByWeekReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			dateperiodpicker.StartDate = new DateTime(DateTime.Today.Year, 1, 1);
			dateperiodpicker.EndDate = DateTime.Today;

            new List<string>() {
                "План",
                "Факт"
            }.ForEach(comboboxReportMode.AppendText);

            comboboxReportMode.Active = 0;

			_geographicGroupNodes = new GenericObservableList<GeographicGroupNode>(
				UoW.GetAll<GeographicGroup>().Select(gg => new GeographicGroupNode(gg)).ToList());
			ytreeviewGeographicGroup.ColumnsConfig = FluentColumnsConfig<GeographicGroupNode>
				.Create()
					.AddColumn("Выбрать").AddToggleRenderer(ggn => ggn.Selected).Editing()
					.AddColumn("Район города").AddTextRenderer(ggn => ggn.ToString())
				.Finish();

			ytreeviewGeographicGroup.ItemsDataSource = _geographicGroupNodes;
			ytreeviewGeographicGroup.HeadersVisible = false;
        }

		#region IParametersWidget implementation

		public string Title => "Статистика заказов по дням недели";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		public object EntityObject => null;

		void OnUpdate(bool hide = false) => 
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

		protected void OnButtonRunClicked(object sender, EventArgs e) => OnUpdate(true);

		private ReportInfo GetReportInfo()
		{
			var selectedGeoGroupsIds = _geographicGroupNodes.Where(ggn => ggn.Selected).Select(ggn => ggn.GeographicGroup.Id).ToArray();
			return new ReportInfo {
				Identifier = "Logistic.OrderStatisticByWeek",
                Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDate },
					{ "end_date", dateperiodpicker.EndDate.AddDays(1).AddTicks(-1) },
                    { "report_mode", comboboxReportMode.Active },
					{ "geographic_group_id", selectedGeoGroupsIds },
					{ "selected_filters", GetSelectedFilters() }
				}
			};
		}

		private string GetSelectedFilters()
		{
			var result = "Фильтры: части города -";
			_geographicGroupNodes.Where(ggn => ggn.Selected).Select(ggn => ggn.ToString()).ForEach(ggName => result += $" {ggName},");
			result += " тип значений -";
			switch(comboboxReportMode.Active)
			{
				case 0:
					result += " план.";
					break;
				case 1:
					result += " факт.";
					break;
			}

			return result;
		}

		protected void OnDateperiodpickerPeriodChanged(object sender, EventArgs e) => 
			buttonRun.Sensitive = dateperiodpicker.StartDateOrNull.HasValue && dateperiodpicker.EndDateOrNull.HasValue;
	}
}
