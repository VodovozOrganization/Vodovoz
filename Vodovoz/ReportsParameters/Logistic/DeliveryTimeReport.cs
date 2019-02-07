using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class DeliveryTimeReport : Gtk.Bin, IParametersWidget
	{
		GenericObservableList<GeographicGroup> geographicGroups;
		IUnitOfWork uow;

		public DeliveryTimeReport ()
		{
			this.Build();
			ConfigureDlg();
			Destroyed += (sender, e) => uow.Dispose();
		}

		void ConfigureDlg()
		{
			uow = UnitOfWorkFactory.CreateWithoutRoot();
			geograficGroup.UoW = uow;
			geograficGroup.Label = "Часть города:";
			geographicGroups = new GenericObservableList<GeographicGroup>();
			geograficGroup.Items = geographicGroups;
			foreach(var gg in uow.Session.QueryOver<GeographicGroup>().List())
				geographicGroups.Add(gg);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет 'Время доставки'";

		#endregion

		private int[] GetResultIds(IEnumerable<int> ids)
		{
			return ids.Any() ? ids.ToArray() : new int[] { 0 };
		}

		void OnUpdate (bool hide = false)
		{
			if (LoadReport != null) {
				LoadReport (this, new LoadReportEventArgs (GetReportInfo (), hide));
			}
		}

		private ReportInfo GetReportInfo ()
		{
			return new ReportInfo {
				Identifier = "Logistic.DeliveryTime",
				Parameters = new Dictionary<string, object>
				{
					{ "beforeTime", ytimeDelivery.Text },
					{ "geographic_groups", GetResultIds(geographicGroups.Select(g => g.Id)) }
				}
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e) => OnUpdate(true);

		protected void OnYtimeDeliveryChanged (object sender, EventArgs e)
		{
			buttonCreateReport.Sensitive = ytimeDelivery.Time != default (TimeSpan);
		}
	}
}
