using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate.Transform;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Reports
{
	public partial class DriversWageBalanceReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		private class DriverNode
		{
			public int 	  Id 		 { get; set; }
			public string Name 		 { get; set; }
			public string LastName 	 { get; set; }
			public string FullName 	 { get {return LastName + " " + Name;} }
			public bool   IsSelected { get; set; } = false;
			public EmployeeCategory Category { get; set; }
			public DateTime FirstWorkDay { get; set; }
		}

		IColumnsConfig columnsConfig = ColumnsConfigFactory.Create<DriverNode> ()
			.AddColumn("Имя").AddTextRenderer(d => d.FullName)
			.AddColumn("Выбрать").AddToggleRenderer(d => d.IsSelected)
		    .RowCells().AddSetter<CellRenderer>((c, n) => c.CellBackground = n.Category == EmployeeCategory.forwarder ? "Light Gray" : "white")
			.Finish();

		IList<DriverNode> driversList = new List<DriverNode>();

		public DriversWageBalanceReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureWgt();
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		public object EntityObject { get {return null;} }

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get	{
				return "Отчет по балансу водителей";
			}
		}

		#endregion

		private void ConfigureWgt()
		{
			FillDrivers();

			ytreeviewDrivers.ColumnsConfig = columnsConfig;
			ytreeviewDrivers.SetItemsSource(driversList);

			ydateBalanceBefore.Date = DateTime.Today;
		}

		void OnUpdate(bool hide = false)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		private ReportInfo GetReportInfo()
		{			
			return new ReportInfo
			{
				Identifier = "Employees.DriversWageBalance",
				Parameters = new Dictionary<string, object>
				{ 
					{ "date", ydateBalanceBefore.Date.Date.AddDays(1).AddTicks(-1) },
					{ "drivers", driversList.Where(d => d.IsSelected).Select(d => d.Id) }
				}
			};
		}

		private void FillDrivers()
		{
			DriverNode resultAlias = null;
			Employee employeeAlias = null;

			driversList = UoW.Session.QueryOver<Employee>(() => employeeAlias)
			                 .Where(() => !employeeAlias.IsFired && (employeeAlias.Category == EmployeeCategory.driver 
			                                                         || employeeAlias.Category == EmployeeCategory.forwarder) )
				.SelectList(list => list
					.Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LastName)
			        .Select(() => employeeAlias.Category).WithAlias (() => resultAlias.Category)
			        .Select(() => employeeAlias.FirstWorkDay).WithAlias(() => resultAlias.FirstWorkDay))
				.OrderBy(e => e.LastName).Asc.ThenBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<DriverNode>())
				.List<DriverNode>();
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			if (driversList.Where(d => d.IsSelected).Select(d => d.Id).Count() <= 0)
			{
				MessageDialogWorks.RunErrorDialog("Необходимо выбрать хотя бы одного водителя");
				return;
			}

			OnUpdate(true);
		}

		protected void OnButtonSelectAllClicked (object sender, EventArgs e)
		{
			foreach (var item in driversList)
				item.IsSelected = true;
			ytreeviewDrivers.SetItemsSource(driversList);
		}

		protected void OnButtonUnselectAllClicked (object sender, EventArgs e)
		{
			foreach (var item in driversList)
				item.IsSelected = false;
			ytreeviewDrivers.SetItemsSource(driversList);
		}

		protected void OnButtonSelectWageClicked(object sender, EventArgs e)
		{
			foreach(var item in driversList)
				item.IsSelected = false;

			var driversListFiltered = driversList.Where(x => CheckDate(x.FirstWorkDay, ydateDateSolary.Date) == true).ToList();
			foreach(var item in driversListFiltered)
				item.IsSelected = true;
			 
			ytreeviewDrivers.SetItemsSource(driversList);				
		}
	 
		public bool CheckDate(DateTime firsWorkDay, DateTime currentDay)
		{
			if(currentDay.Subtract(firsWorkDay).Days > 14)
			{
				var monday = currentDay.AddDays(1 - ((int)currentDay.DayOfWeek == 0 ? 7 : (int)currentDay.DayOfWeek));
				var daydiff = (monday - firsWorkDay).Days;  
				return (daydiff - 1) % 14 < 7;
			}
			 
			return false;
		}
	}
}

