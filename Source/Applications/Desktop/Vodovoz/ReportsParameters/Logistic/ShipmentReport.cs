using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Store;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using Vodovoz.Tools.Store;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ShipmentReport : SingleUoWWidgetBase, IParametersWidget
	{
		public ShipmentReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ydatepicker.Date = DateTime.Now.Date;
			referenceWarehouse.ItemsQuery = StoreDocumentHelper.GetNotArchiveWarehousesQuery();
			ButtonSensitivity();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчёт по отгрузке автомобилей";

		#endregion

		ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Logistic.ShipmentReport",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", ydatepicker.Date },
					{ "department", GetDepartment()}
				}
			};
		}

		string GetDepartment()
		{
			if(radioAll.Active)
			{
				return "-1";
			}

			if(radioCash.Active)
			{
				return "Касса";
			}
				
			return (referenceWarehouse.Subject as Warehouse).Name;
		}

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		void ButtonSensitivity()
		{
			referenceWarehouse.Sensitive = radioWarehouse.Active;
			buttonCreateReport.Sensitive = !radioWarehouse.Active || (radioWarehouse.Active && referenceWarehouse.Subject != null);
		}

		protected void OnRadioAllToggled(object sender, EventArgs e)
		{
			ButtonSensitivity();
		}

		protected void OnRadioCashToggled(object sender, EventArgs e)
		{
			ButtonSensitivity();
		}

		protected void OnRadioWarehouseToggled(object sender, EventArgs e)
		{
			ButtonSensitivity();
		}

		protected void OnReferenceWarehouseChanged(object sender, EventArgs e)
		{
			ButtonSensitivity();
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}
	}
}
