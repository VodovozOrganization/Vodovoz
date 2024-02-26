using System;
using System.Collections.Generic;
using System.Linq;
using DateTimeHelpers;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QS.Widgets;
using QSReport;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class RouteListsOnClosingReport : SingleUoWWidgetBase, IParametersWidget
	{
		public RouteListsOnClosingReport()
		{
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();

			Configure();
		}

		private void Configure()
		{
			ycheckTodayRouteLists.Active = true;
			nullCheckVisitingMasters.RenderMode = RenderMode.Icon;
			ySpecCmbGeographicGroup.ItemsList = UoW.GetAll<GeoGroup>();

			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.SelectAll();

			enumcheckCarOwnType.EnumType = typeof(CarOwnType);
			enumcheckCarOwnType.SelectAll();

			var now = DateTime.Now;
			dateEnd.Date = now.FirstDayOfMonth();
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по незакрытым МЛ";

		private ReportInfo GetReportInfo()
		{
			var carTypesOfUse = enumcheckCarTypeOfUse.SelectedValues.ToArray();
			var carOwnTypes = enumcheckCarOwnType.SelectedValues.ToArray();

			return new ReportInfo
			{
				Identifier = "Logistic.RouteListOnClosing",
				Parameters = new Dictionary<string, object>
				{
					{ "geographic_group_id", (ySpecCmbGeographicGroup.SelectedItem as GeoGroup)?.Id ?? 0 },
					{ "car_types_of_use", carTypesOfUse.Any() ? carTypesOfUse : new[] { (object)0 } },
					{ "car_own_types", carOwnTypes.Any() ? carOwnTypes : new[] { (object)0 } },
					{ "show_today_route_lists", ycheckTodayRouteLists.Active },
					{ "include_visiting_masters", nullCheckVisitingMasters.Active },
					{ "end_date", dateEnd.DateOrNull.Value }
				}
			};
		}

		private void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}
	}
}
