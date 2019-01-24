using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Tdi;
using Vodovoz;
using Vodovoz.Domain.Employees;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.ViewModel;

namespace Dialogs.Employees
{
	public partial class EmployeeWorkChartDlg : QS.Dialog.Gtk.TdiTabBase, ITdiDialog
	{
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private List<EmployeeWorkChart> loadedCharts = new List<EmployeeWorkChart>();
		private List<EmployeeWorkChart> newCharts = new List<EmployeeWorkChart>();
		private List<EmployeeWorkChart> chartsToDelete = new List<EmployeeWorkChart>();
		private List<Tuple<int, DateTime>> cleared = new List<Tuple<int, DateTime>>();
		private Employee previousEmployee = new Employee();
		private string employeeName;
		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		#region Свойства

		public override string TabName {
			get {
				return String.Format("График работы сотрудника {0}", employeeName);
			}
			protected set {
				throw new InvalidOperationException("Установка протеворечит логике работы.");
			}
		}

		public bool HasChanges { get { return uow.HasChanges; } }

		#endregion

		public EmployeeWorkChartDlg()
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			DateTime now = DateTime.Now;

			var filter = new EmployeeFilter(uow);
			yentryEmployee.RepresentationModel = new EmployeesVM(filter);
			yentryEmployee.Changed += YentryEmployee_Changed;

			yenumcomboMonth.ItemsEnum = typeof(Months);
			yenumcomboMonth.SelectedItem = (Months)now.Month;
			yenumcomboMonth.EnumItemSelected += YenumcomboMonth_EnumItemSelected;

			yspinYear.Value = (double)now.Year;
			yspinYear.ValueChanged += YspinYear_ValueChanged;

			workcharttable.Date = now;
		}

		void YspinYear_ValueChanged(object sender, EventArgs e)
		{
			SetTableDate();
		}

		void YentryEmployee_Changed(object sender, EventArgs e)
		{
			ChangeTableData();
		}

		void YenumcomboMonth_EnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			ChangeTableData();
		}

		private void SetTableDate()
		{
			workcharttable.Date = new DateTime(yspinYear.ValueAsInt, (int)yenumcomboMonth.SelectedItem, 1);
			workcharttable.QueueDraw();
		}

		private void ChangeTableData()
		{
			Employee emp = yentryEmployee.Subject as Employee;

			if(emp == null)
				return;

			employeeName = emp.ShortName;
			OnTabNameChanged();

			int month = (int)yenumcomboMonth.SelectedItem;
			int year = yspinYear.ValueAsInt;

			logger.Debug(string.Format("Изменена дата на {0}.{1}", month, year));
			IList<EmployeeWorkChart> charts = null;

			var exist = newCharts.FirstOrDefault(e => e.Date.Month == month && e.Date.Year == year
				&& e.Employee == emp);

			if(exist == null) {

				exist = loadedCharts.FirstOrDefault(e => e.Date.Month == month && e.Date.Year == year
					&& e.Employee.Id == emp.Id);
				if(exist == null) {

					logger.Debug("Загрузка данных из БД");

					charts = EmployeeRepository.GetWorkChartForEmployeeByDate(
						uow, emp, new DateTime(year, month, 1));

					foreach(var item in charts)
						if(!loadedCharts.Contains(item))
							loadedCharts.Add(item);
				} else {
					var tuple = cleared.FirstOrDefault(c => c.Item2.Month == month && c.Item2.Year == year
								&& c.Item1 == emp.Id);
					if(tuple == null) {
						logger.Debug("Получение данных из кеша");

						charts = loadedCharts.Where(e => e.Date.Month == month && e.Date.Year == year
							&& e.Employee == emp).ToList();
					}
				}
			} else {
				logger.Debug("Получение измененных пользователем данных");

				charts = newCharts.Where(e => e.Date.Month == month && e.Date.Year == year
					&& e.Employee.Id == emp.Id).ToList();
			}

			var chartsFromTable = workcharttable.GetWorkChart();

			if(chartsFromTable.Count == 0) {
				if(loadedCharts.FirstOrDefault(c => c.Date.Month == workcharttable.Date.Month
					  && c.Date.Year == workcharttable.Date.Year
					  && c.Employee.Id == previousEmployee.Id) != null) {
					cleared.Add(new Tuple<int, DateTime>(previousEmployee.Id,
							new DateTime(workcharttable.Date.Year, workcharttable.Date.Month, 1)));
				}
			}

			SetEmployeeForCharts(chartsFromTable, previousEmployee);
			DeleteItemsByDate(newCharts, workcharttable.Date.Month, workcharttable.Date.Year, emp);
			newCharts.AddRange(chartsFromTable);

			logger.Debug("Передача данных в таблицу");
			workcharttable.SetWorkChart(charts);
			SetTableDate();
			previousEmployee = emp;
		}

		public bool Save()
		{
			logger.Debug("Сохранение...");
			var toSave = GetItemsForSave(loadedCharts, newCharts);
			foreach(var item in toSave) {
				uow.Save(item);
			}
			foreach(var item in chartsToDelete) {
				uow.Delete(item);
			}
			uow.Commit();
			ClearData();
			logger.Debug("Сохранение завершено");
			return true;
		}

		public void SaveAndClose()
		{
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			Employee employee = yentryEmployee.Subject as Employee;
			if(employee == null)
				return;

			var chartsFromTable = workcharttable.GetWorkChart();
			SetEmployeeForCharts(chartsFromTable, previousEmployee);
			DeleteItemsByDate(newCharts, workcharttable.Date.Month, workcharttable.Date.Year, employee);
			newCharts.AddRange(chartsFromTable);

			Save();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			uow.Session.Clear();
			ClearData();
		}

		private void DeleteItemsByDate(List<EmployeeWorkChart> list, int month, int year, Employee employee)
		{
			var temp = list.FirstOrDefault(i => i.Date.Month == month && i.Date.Year == year
				&& i.Employee == employee);
			while(temp != null) {
				list.Remove(temp);
				temp = list.FirstOrDefault(i => i.Date.Month == month && i.Date.Year == year
					&& i.Employee == employee);
			}

		}

		private void SetEmployeeForCharts(IList<EmployeeWorkChart> list, Employee employee)
		{
			foreach(var item in list) {
				item.Employee = employee;
			}
		}

		private IList<EmployeeWorkChart> GetItemsForSave(IList<EmployeeWorkChart> loadedList, IList<EmployeeWorkChart> newList)
		{
			if(loadedList.Count == 0)
				return newList;

			if(newList.Count == 0) {
				chartsToDelete.AddRange(loadedList);
				return newList;
			}

			for(int i = 0; i < loadedList.Count; i++) {
				if(i >= newList.Count)
					chartsToDelete.Add(loadedList[i]);
				else {
					newList[i].Id = loadedList[i].Id;
					uow.Session.Evict(loadedList[i]);
				}
			}

			return newList;
		}

		private void ClearData()
		{
			logger.Debug("Сброс данных");

			loadedCharts.Clear();
			newCharts.Clear();
			chartsToDelete.Clear();
			cleared.Clear();
			previousEmployee = new Employee();
			workcharttable.Reset();
		}

		public enum Months
		{
			[Display(Name = "Январь")] 	 Jan = 1,
			[Display(Name = "Февраль")]  Feb,
			[Display(Name = "Март")] 	 Mar,
			[Display(Name = "Апрель")] 	 Apr,
			[Display(Name = "Май")] 	 May,
			[Display(Name = "Июнь")] 	 Jun,
			[Display(Name = "Июль")] 	 Jul,
			[Display(Name = "Август")] 	 Aug,
			[Display(Name = "Сентябрь")] Sep,
			[Display(Name = "Октябрь")]  Oct,
			[Display(Name = "Ноябрь")] 	 Nov,
			[Display(Name = "Декабрь")]  Dec,
		}
	}
}

