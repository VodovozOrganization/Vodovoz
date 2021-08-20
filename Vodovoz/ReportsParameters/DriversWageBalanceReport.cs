using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSProjectsLib;
using QSReport;
using Vodovoz.Domain.Employees;
using QS.Dialog.GtkUI;

namespace Vodovoz.Reports
{
	public partial class DriversWageBalanceReport : SingleUoWWidgetBase, IParametersWidget
	{
		private class DriverNode
		{
			public int 	  Id 		 { get; set; }
			public string Name 		 { get; set; }
			public string LastName 	 { get; set; }
			public string Patronymic { get; set; }
			public string FullName => LastName + " " + Name + (String.IsNullOrWhiteSpace(Patronymic) ? "" : (" " + Patronymic));
			public bool   IsSelected { get; set; } = false;
			public EmployeeCategory Category { get; set; }
			public DateTime FirstWorkDay { get; set; }
		}

		//Если это сопровождающий(forwarder) закрашивает серым то закрасить серым
		IColumnsConfig columnsConfig = ColumnsConfigFactory.Create<DriverNode> ()
			.AddColumn("Код").AddNumericRenderer(d => d.Id)
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
			                 .Where(
				                 () => employeeAlias.Status != EmployeeStatus.IsFired
				                 && (employeeAlias.Category == EmployeeCategory.driver || employeeAlias.Category == EmployeeCategory.forwarder)
				                 && !employeeAlias.VisitingMaster
				                 && employeeAlias.Status != EmployeeStatus.OnCalculation
				                )
			                 .SelectList(list => list
			                             .Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
			                             .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.Name)
			                             .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LastName)
			                             .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
			                             .Select(() => employeeAlias.Category).WithAlias (() => resultAlias.Category)
			                             .Select(() => employeeAlias.FirstWorkDay).WithAlias(() => resultAlias.FirstWorkDay))
			                 .OrderBy(e => e.LastName).Asc.ThenBy(x => x.Name).Asc
			                 .TransformUsing(Transformers.AliasToBean<DriverNode>())
			                 .List<DriverNode>();
		}

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			//Сохранение значений во временную структуру
			var oldSelected = new Dictionary<int, bool>();
			foreach(var item in driversList) {
				if(item.IsSelected) {
					oldSelected.Add(item.Id, item.IsSelected);
				}
			}

			FillDrivers(); // обновление значений

			//Возврат значений 
			foreach(var item in oldSelected) {
				for(int i = 0; i < driversList.Count; i++) {
					if(driversList[i].Id == item.Key) {
						driversList[i].IsSelected = item.Value;
					}
				}
			}


			ytreeviewDrivers.SetItemsSource(driversList); // Обновление списка

			if(driversList.Where(d => d.IsSelected).Select(d => d.Id).Count() <= 0)
			{
				MessageDialogWorks.RunErrorDialog("Необходимо выбрать хотя бы одного водителя");
				return;
			}

			OnUpdate(true); // Загрузка листа отчета


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
		//кнопка Проставить ЗП 
		protected void OnButtonSelectWageClicked(object sender, EventArgs e)
		{
			FillDrivers();
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

