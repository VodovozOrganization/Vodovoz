using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ScheduleOnLinePerShiftReport : SingleUoWWidgetBase, IParametersWidget
	{
		GenericObservableList<GeographicGroup> geographicGroups;
		
		public ScheduleOnLinePerShiftReport()
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			geographicGroup.UoW = UoW;
			geographicGroup.Label = "Часть города:";
			geographicGroups = new GenericObservableList<GeographicGroup>();
			geographicGroup.Items = geographicGroups;
			foreach(var gg in UoW.Session.QueryOver<GeographicGroup>().List())
				geographicGroups.Add(gg);

			yEnumCmbTransport.ItemsEnum = typeof(CarTypeOfUse);
			
			buttonCreateReport.Clicked += OnButtonCreateReportClicked;
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public string Title => "Отчет по выдаче топлива по МЛ";
		
		private int[] GetResultIds(IEnumerable<int> ids)
		{
			return ids.Any() ? ids.ToArray() : new int[] { 0 };
		}
		
		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Logistic.ScheduleOnLinePerShiftReport" , 
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull },
					{ "geo_group_ids", GetResultIds(geographicGroups.Select(g => g.Id)) },
					{ "transport_type", yEnumCmbTransport.SelectedItemOrNull?.ToString() ?? ""},
					{ "is_raskat", ycheckRaskat.Active? 1 : 0 }
				}
			};
		}
		
		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
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
