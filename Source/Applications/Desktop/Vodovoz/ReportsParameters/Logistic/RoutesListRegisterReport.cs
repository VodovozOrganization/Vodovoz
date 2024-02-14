using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Reports.Logistic
{
	public partial class RoutesListRegisterReport : SingleUoWWidgetBase, IParametersWidget
	{
		GenericObservableList<GeoGroup> geographicGroups;

		public RoutesListRegisterReport()
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			geographicGroup.UoW = UoW;
			geographicGroup.Label = "Часть города:";
			geographicGroups = new GenericObservableList<GeoGroup>();
			geographicGroup.Items = geographicGroups;
			foreach(var gg in UoW.Session.QueryOver<GeoGroup>().List())
				geographicGroups.Add(gg);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Реестр маршрутных листов";

		#endregion

		private int[] GetResultIds(IEnumerable<int> ids)
		{
			return ids.Any() ? ids.ToArray() : new int[] { 0 };
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Bottles.RoutesListRegister",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull },
					{ "is_driver_master", chkMasters.Active ? 1 : 0 },
					{ "geographic_groups", GetResultIds(geographicGroups.Select(g => g.Id)) }
				}
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDateOrNull == null) {
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать дату");
				return;
			}
			OnUpdate(true);
		}
	}
}
