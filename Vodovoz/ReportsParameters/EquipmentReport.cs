using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EquipmentReport : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		GenericObservableList<GeographicGroup> geographicGroups;

		public EquipmentReport()
		{
			this.Build();
			ConfigureDlg();
			Destroyed += (sender, e) => UoW.Dispose();
		}

		void ConfigureDlg()
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			geograficGroup.UoW = UoW;
			geograficGroup.Label = "Часть города:";
			geographicGroups = new GenericObservableList<GeographicGroup>();
			geograficGroup.Items = geographicGroups;
			foreach(var gg in UoW.Session.QueryOver<GeographicGroup>().List())
				geographicGroups.Add(gg);
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по оборудованию";

		#endregion

		private int[] GetResultIds(IEnumerable<int> ids)
		{
			return ids.Any() ? ids.ToArray() : new int[] { 0 };
		}

		private ReportInfo GetReportInfo()
		{
			var repInfo = new ReportInfo {
				Identifier = "ServiceCenter.EquipmentReport",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDate },
					{ "end_date", dateperiodpicker.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59) },
					{ "geographic_groups", GetResultIds(geographicGroups.Select(g => g.Id)) }
				}
			};
			return repInfo;
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			string errorString = string.Empty;
			if(dateperiodpicker.StartDateOrNull == null)
				errorString += "Не заполнена дата\n";
			if(!string.IsNullOrWhiteSpace(errorString)) {
				MessageDialogHelper.RunErrorDialog(errorString);
				return;
			}
			OnUpdate(true);
		}
	}
}

