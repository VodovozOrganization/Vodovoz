using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using QS.Dialog.Gtk;
using Vodovoz.Domain.Client;
using System.Linq;
using QS.DomainModel.UoW;
using Gamma.ColumnConfig;
using System.Data.Bindings;
using Gtk;
using Vodovoz.ViewModel;
using Vodovoz.Domain.Employees;
using System.Threading.Tasks;

namespace Vodovoz.Dialogs.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CRMTasksDlg : SingleUowDialogBase
	{
		GenericObservableList<BottleDebtor> observableDebtors;

		IList<BottleDebtor> bottleDebtors;
		private IList<BottleDebtor> BottleDebtors {
			set {
				bottleDebtors = value;
				observableDebtors = new GenericObservableList<BottleDebtor>(bottleDebtors);
				debtorsTreeView.SetItemsSource(observableDebtors);
			}
			get => bottleDebtors;
		}

		public IColumnsConfig ColumnsConfig { get; } = FluentColumnsConfig<BottleDebtor>.Create()
			.AddColumn("Статус").SetDataProperty(node => node.TaskState.GetEnumTitle())
			.AddColumn("Клиент").AddTextRenderer(node => node.Client.Name)
			.AddColumn("Долг по бутылям").AddTextRenderer(node => node.DebtByAdress.ToString())
			.AddColumn("Ответственный").AddTextRenderer(node => node.AssignedEmployee.ShortName)
			.AddColumn("Дата звонка").AddTextRenderer(node => node.NextCallDate.Date.ToString("dd / MM / yyyy "))
			.RowCells().AddSetter<CellRendererText>((c, n) => {
				if(n.IsTaskComplete)
					c.Foreground = "green";
				else if((DateTime.Now - n.NextCallDate).Days > 0)
					c.Foreground = "red";
				else
					c.Foreground = "black"; 
			 	})
			.Finish();

		public bool HasValueChanges { get; private set; } = false;


		public CRMTasksDlg()
		{
			this.Build();
			TaskstateButton.ItemsEnum = typeof(DebtorStatus);
			debtorsTreeView.ColumnsConfig = ColumnsConfig;
			debtorsTreeView.Selection.Mode = SelectionMode.Multiple;
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			this.TabName = "Журнал задач для обзвона";
			EmployeesVM employeeVM = new EmployeesVM();
			employeeVM.Filter.RestrictCategory = EmployeeCategory.office;
			yentryreferencevm1.RepresentationModel = employeeVM;
			UpdateNodes();
		}

		//FIXME : Переделать входные параметры ( добавить нормальный фильтр ) 
		private void UpdateFilters(bool? status = null, DateTime? startDate = null, DateTime? endDate = null)
		{
			UpdateNodes();
			if(status !=null)
				BottleDebtors = BottleDebtors.Where((debtor) => debtor.IsTaskComplete == status).ToList();
			if( startDate != null && endDate != null) {
				BottleDebtors = BottleDebtors.Where((debtor) => debtor.NextCallDate >= startDate && debtor.NextCallDate <= endDate).ToList();
				dateperiodpicker1.StartDate = startDate.Value;
				dateperiodpicker1.EndDate = endDate.Value;
			}
			taskCount.Text = BottleDebtors.Count.ToString();
		}

		public void UpdateNodes()
		{
			UoW.Session.Clear();
			BottleDebtors = UoW.Session.QueryOver<BottleDebtor>()
			.OrderBy(d => d.Id).Desc
			.List<BottleDebtor>();
			taskCount.Text = BottleDebtors.Count.ToString();
		}

		public override bool Save()
		{
			BottleDebtors.ToList().ForEach((debtor) => debtor.UoW.Commit());
			return true;
		}

		//TODO : реализовать открытие диалога в боковой панели 
		protected void OnHideRightPanelButtonClicked(object sender, EventArgs e)
		{
			//vboxEntityDlg.Visible = !vboxEntityDlg.Visible;
			//if(vboxEntityDlg.Visible)
			//	hideRightPanelButton.Label = "<<";
			//else
				//hideRightPanelButton.Label = ">>";
		}

#region WidgetEventHeandler

		protected void OnSearchentityTextChanged(object sender, EventArgs e)
		{
			debtorsTreeView.SearchHighlightText = searchentity.Text;
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			if(HasValueChanges)
				Save();
			OnCloseTab(false);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(HasValueChanges);
		}

		protected void OnDebtorsTreeViewRowActivated(object o, RowActivatedArgs args)
		{
			buttonEdit.Click();
		}

		#region BaseTreeViewButton

		protected void OnAddTaskButtonClicked(object sender, EventArgs e)
		{
			BottledDebtorDlg dlg = new BottledDebtorDlg();
			dlg.TaskChanges += (() => {
				this.HasValueChanges = true;
				UpdateNodes();
			});
			OpenSlaveTab(dlg);
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			var debtor = (debtorsTreeView.GetSelectedObjects()[0] as BottleDebtor);
			if(debtor != null) 
			{
				BottledDebtorDlg dlg = new BottledDebtorDlg(debtor.Id);
				dlg.TaskChanges += (() => {
					this.HasValueChanges = true;
					UpdateNodes();
				});
				OpenSlaveTab(dlg);
			}
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			debtorsTreeView.GetSelectedObjects().OfType<BottleDebtor>().ToList().ForEach((task) => {
				task.UoW.Delete(task);
				task.UoW.Commit();
			});
			HasValueChanges = true;
			UpdateNodes();
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			UpdateNodes();
		}

		protected void OnButtonFilterClicked(object sender, EventArgs e)
		{
			hboxDateFilter.Visible = !hboxDateFilter.Visible;
			hboxChangeEntities.Visible = !hboxChangeEntities.Visible;
		}

		#endregion

		#region ChangeEntityStateButton

		protected void OnNextCallDatePickerDateChangedByUser(object sender, EventArgs e)
		{
			debtorsTreeView.GetSelectedObjects().OfType<BottleDebtor>().ToList().ForEach((task) => {
				var debtor = new BottleDebtor();
				debtor.Client = task.Client;
				debtor.Address = task.Address;
				debtor.DateOfTaskCreation = DateTime.Now;
				debtor.NextCallDate = nextCallDatePicker.Date;
				debtor.Comment = task.Comment;
				debtor.AssignedEmployee = task.AssignedEmployee;
				task.IsTaskComplete = true;
				debtor.UoW = UnitOfWorkFactory.CreateWithoutRoot();
				debtor.UoW.Save(debtor);
				debtor.UoW.Commit();
				task.UoW.Save(task);
				task.UoW.Commit();
			});
			HasValueChanges = true;
			UpdateNodes();
		}

		protected void OnAssignedEmployeeButtonClicked(object sender, EventArgs e)
		{
			yentryreferencevm1.OpenSelectDialog("Ответственный :");
		}

		protected void OnYentryreferencevm1ChangedByUser(object sender, EventArgs e)
		{
			debtorsTreeView.GetSelectedObjects().OfType<BottleDebtor>().ToList().ForEach((task) => {
				task.AssignedEmployee = yentryreferencevm1.Subject as Employee;
				task.UoW.Save(task);
				task.UoW.Commit();
			});
			HasValueChanges = true;
		}

		protected void OnCompleteTaskButtonClicked(object sender, EventArgs e)
		{
			debtorsTreeView.GetSelectedObjects().OfType<BottleDebtor>().ToList().ForEach((task) => {
				task.IsTaskComplete = true;
				task.UoW.Save(task);
				task.UoW.Commit();
			});
			HasValueChanges = true;
		}

		protected void OnTaskstateButtonEnumItemClicked(object sender, QSOrmProject.EnumItemClickedEventArgs e)
		{
			debtorsTreeView.GetSelectedObjects().OfType<BottleDebtor>().ToList().ForEach((task) => {
				task.TaskState = (Vodovoz.Domain.Client.DebtorStatus)e.ItemEnum;
				task.UoW.Save(task);
				task.UoW.Commit();
			});
			HasValueChanges = true;
		}

		protected void OnNextCallDateChanges(object sender, EventArgs e)
		{
			debtorsTreeView.GetSelectedObjects().OfType<BottleDebtor>().ToList().ForEach((task) => task.NextCallDate = nextCallDatePicker.Date);
		}
		#endregion

		#region Filters

		protected void OnButtonDatePastClicked(object sender, EventArgs e)
		{
			UpdateFilters(false, DateTime.MinValue, DateTime.Now.AddDays(-1));
		}

		protected void OnButtonDateTodayClicked(object sender, EventArgs e)
		{
			UpdateFilters(startDate: DateTime.Now , endDate : DateTime.Now.AddDays(1));
		}

		protected void OnButtonDateTomorrowClicked(object sender, EventArgs e)
		{
			UpdateFilters(startDate: DateTime.Now.AddDays(1), endDate: DateTime.Now.AddDays(2));
		}

		protected void OnButtonDateThisWeekClicked(object sender, EventArgs e)
		{
			GetWeekPeriod();
		}

		protected void OnButtonDateNextWeekClicked(object sender, EventArgs e)
		{
			GetWeekPeriod(1);
		}

		protected void OnButtonDateAllClicked(object sender, EventArgs e)
		{
			UpdateNodes();
		}

		protected void OnDateperiodpicker1PeriodChangedByUser(object sender, EventArgs e)
		{
			UpdateFilters(startDate: dateperiodpicker1.StartDate, endDate: dateperiodpicker1.EndDate);
		}

		private void GetWeekPeriod(int weekIndex = 0)
		{
			DateTime date = DateTime.Now.Date.AddDays(weekIndex * 7);
			switch(date.DayOfWeek) {
				case DayOfWeek.Monday:
					UpdateFilters(startDate: date, endDate: date.AddDays(7));
					break;
				case DayOfWeek.Tuesday:
					UpdateFilters(startDate: date.AddDays(-1), endDate: date.AddDays(6));
					break;
				case DayOfWeek.Wednesday:
					UpdateFilters(startDate: date.AddDays(-2), endDate: date.AddDays(5));
					break;
				case DayOfWeek.Thursday:
					UpdateFilters(startDate: date.AddDays(-3), endDate: date.AddDays(4));
					break;
				case DayOfWeek.Friday:
					UpdateFilters(startDate: date.AddDays(-4), endDate: date.AddDays(3));
					break;
				case DayOfWeek.Saturday:
					UpdateFilters(startDate: date.AddDays(-5), endDate: date.AddDays(2));
					break;
				case DayOfWeek.Sunday:
					UpdateFilters(startDate: date.AddDays(-6), endDate: date.AddDays(1));
					break;
			}
		}

		#endregion

		#endregion
	}
}
