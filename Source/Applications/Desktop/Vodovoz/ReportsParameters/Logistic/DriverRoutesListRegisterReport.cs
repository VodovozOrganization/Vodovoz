﻿using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Sale;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class DriverRoutesListRegisterReport : SingleUoWWidgetBase, IParametersWidget
	{
		GenericObservableList<GeoGroup> geographicGroups;
		private readonly ReportFactory _reportFactory;

		public DriverRoutesListRegisterReport(ReportFactory reportFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.Build();
			ConfigureDlg();

			buttonCreateReport.Clicked += OnButtonCreateReportClicked;
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

		public string Title => "Реестр маршрутных листов (По водителям)";

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
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{ "is_driver_master", chkMasters.Active ? 1 : 0 },
				{ "geographic_groups", GetResultIds(geographicGroups.Select(g => g.Id)) }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Logistic.DriverRoutesListRegister";
			reportInfo.Parameters = parameters;

			return reportInfo;
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
