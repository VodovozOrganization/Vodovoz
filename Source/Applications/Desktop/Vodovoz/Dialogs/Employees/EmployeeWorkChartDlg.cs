using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Tdi;
using Vodovoz;
using Vodovoz.CommonEnums;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModel;

namespace Dialogs.Employees
{
	public partial class EmployeeWorkChartDlg : QS.Dialog.Gtk.TdiTabBase, ITdiDialog
	{
		private readonly ILogger<EmployeeWorkChartDlg> _logger;
		private readonly IEmployeeRepository _employeeRepository;
			
		private IUnitOfWork uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
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

		public virtual bool HasCustomCancellationConfirmationDialog { get; private set; }

		public virtual Func<int> CustomCancellationConfirmationDialogFunc { get; private set; }

		#endregion

		public EmployeeWorkChartDlg(
			ILogger<EmployeeWorkChartDlg> logger,
			IEmployeeJournalFactory employeeJournalFactory,
			IEmployeeRepository employeeRepository)
		{
			if(employeeJournalFactory == null)
			{
				throw new ArgumentNullException(nameof(employeeJournalFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

			Build();
			ConfigureDlg(employeeJournalFactory);
		}

		private void ConfigureDlg(IEmployeeJournalFactory employeeJournalFactory)
		{
			DateTime now = DateTime.Now;
			
			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeJournalFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEmployee.Changed += YentryEmployee_Changed;

			yenumcomboMonth.ItemsEnum = typeof(Month);
			yenumcomboMonth.SelectedItem = (Month)now.Month;
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
			if(!(evmeEmployee.Subject is Employee emp))
			{
				return;
			}

			employeeName = emp.ShortName;
			OnTabNameChanged();

			int month = (int)yenumcomboMonth.SelectedItem;
			int year = yspinYear.ValueAsInt;

			_logger.LogDebug("Изменена дата на {Month}.{Year}", month, year);
			IList<EmployeeWorkChart> charts = null;

			var exist = newCharts.FirstOrDefault(e => e.Date.Month == month && e.Date.Year == year
				&& e.Employee == emp);

			if(exist == null) {

				exist = loadedCharts.FirstOrDefault(e => e.Date.Month == month && e.Date.Year == year
					&& e.Employee.Id == emp.Id);
				if(exist == null) {

					_logger.LogDebug("Загрузка данных из БД");

					charts = _employeeRepository.GetWorkChartForEmployeeByDate(
						uow, emp, new DateTime(year, month, 1));

					foreach(var item in charts)
						if(!loadedCharts.Contains(item))
							loadedCharts.Add(item);
				} else {
					var tuple = cleared.FirstOrDefault(c => c.Item2.Month == month && c.Item2.Year == year
								&& c.Item1 == emp.Id);
					if(tuple == null) {
						_logger.LogDebug("Получение данных из кеша");

						charts = loadedCharts.Where(e => e.Date.Month == month && e.Date.Year == year
							&& e.Employee == emp).ToList();
					}
				}
			} else {
				_logger.LogDebug("Получение измененных пользователем данных");

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

			_logger.LogDebug("Передача данных в таблицу");
			workcharttable.SetWorkChart(charts);
			SetTableDate();
			previousEmployee = emp;
		}

		public bool Save()
		{
			_logger.LogDebug("Сохранение...");
			var toSave = GetItemsForSave(loadedCharts, newCharts);
			foreach(var item in toSave) {
				uow.Save(item);
			}
			foreach(var item in chartsToDelete) {
				uow.Delete(item);
			}
			uow.Commit();
			ClearData();
			_logger.LogDebug("Сохранение завершено");
			return true;
		}

		public void SaveAndClose()
		{
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			if(!(evmeEmployee.Subject is Employee employee))
			{
				return;
			}

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
			_logger.LogDebug("Сброс данных");

			loadedCharts.Clear();
			newCharts.Clear();
			chartsToDelete.Clear();
			cleared.Clear();
			previousEmployee = new Employee();
			workcharttable.Reset();
		}

		public override void Destroy()
		{
			uow?.Dispose();
			CustomCancellationConfirmationDialogFunc = null;
			base.Destroy();
		}
	}
}
