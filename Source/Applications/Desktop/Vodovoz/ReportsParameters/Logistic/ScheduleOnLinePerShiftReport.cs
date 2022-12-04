using QS.Dialog.GtkUI;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class ScheduleOnLinePerShiftReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public ScheduleOnLinePerShiftReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			geographicGroup.UoW = UoW;
			geographicGroup.Label = "Часть города:";
			geographicGroup.Items = new GenericObservableList<GeoGroup>(UoW.GetAll<GeoGroup>().ToList());

			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.AddEnumToHideList(CarTypeOfUse.Loader);
			enumcheckCarTypeOfUse.SelectAll();

			enumcheckCarOwnType.EnumType = typeof(CarOwnType);
			enumcheckCarOwnType.SelectAll();

			buttonCreateReport.Clicked += OnButtonCreateReportClicked;
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public string Title => "График выхода на линию за смену";

		private ReportInfo GetReportInfo()
		{
			var geoGroupsIds = geographicGroup.Items.Select(g => g.Id).ToArray();
			var carTypesOfUse = enumcheckCarTypeOfUse.SelectedValues.ToArray();
			var carOwnTypes = enumcheckCarOwnType.SelectedValues.ToArray();

			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{ "geo_group_ids", geoGroupsIds.Any() ? geoGroupsIds : new[] { 0 } },
				{ "car_types_of_use", carTypesOfUse.Any() ? carTypesOfUse : new[] { (object)0 } },
				{ "car_own_types", carOwnTypes.Any() ? carOwnTypes : new[] { (object)0 } }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Logistic.ScheduleOnLinePerShiftReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		private void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDateOrNull == null)
			{
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать дату");
				return;
			}
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}
	}
}
