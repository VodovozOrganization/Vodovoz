using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class ScheduleOnLinePerShiftReport : SingleUoWWidgetBase, IParametersWidget
	{
		public ScheduleOnLinePerShiftReport()
		{
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

			return new ReportInfo
			{
				Identifier = "Logistic.ScheduleOnLinePerShiftReport",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull },
					{ "geo_group_ids", geoGroupsIds.Any() ? geoGroupsIds : new[] { 0 } },
					{ "car_types_of_use", carTypesOfUse.Any() ? carTypesOfUse : new[] { (object)0 } },
					{ "car_own_types", carOwnTypes.Any() ? carOwnTypes : new[] { (object)0 } }
				}
			};
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
