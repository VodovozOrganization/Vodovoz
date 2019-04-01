using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Tdi.Gtk;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.JournalFilters;
using Vodovoz.ViewModel;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TasksView : SingleUowDialogBase
	{
		GenericObservableList<CallTask> observableTasks;
		Dictionary<int, CallTask> tasksMemento = new Dictionary<int, CallTask>();
		TaskFilter taskFilter ;
		string searchString = null;

		IList<CallTask> tasks;
		private IList<CallTask> Tasks{
			set {
				tasks = value;
				observableTasks = new GenericObservableList<CallTask>(tasks);
				ytreeviewTasks.SetItemsSource(observableTasks);
			}
			get => tasks;
		}

		public IColumnsConfig ColumnsConfig { get; } = FluentColumnsConfig<CallTask>.Create()
			.AddColumn("№").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Статус").AddEnumRenderer(node => node.TaskState)
			.AddColumn("Клиент").AddTextRenderer(node => node.Address != null ? node.Address.Counterparty.Name : String.Empty)
			.AddColumn("Адрес").AddTextRenderer(node => node.Address != null ? node.Address.ShortAddress : String.Empty)
			.AddColumn("Долг по адресу").AddTextRenderer(node => node.DebtByAddress.ToString())
			.AddColumn("Долг по клиенту").AddTextRenderer(node => node.DebtByClient.ToString())
			.AddColumn("Телефены").AddTextRenderer(node => node.Phones)
			.AddColumn("Ответственный").AddTextRenderer(node => node.AssignedEmployee != null ? node.AssignedEmployee.ShortName : String.Empty)
			.AddColumn("Выполнить до").AddTextRenderer(node => node.Deadline.ToString("dd / MM / yyyy  HH:mm"))
			.RowCells().AddSetter<CellRendererText>((c, n) => {
				if(n.IsTaskComplete)
					c.Foreground = "green";
				else if(DateTime.Now > n.Deadline)
					c.Foreground = "red";
				else
					c.Foreground = "black";
			})
			.Finish();
			
		public TasksView()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			this.TabName = "Журнал задач для обзвона";
			ConfigureDlg();
		}

		public void ConfigureDlg()
		{
			taskFilter = taskfilter;
			taskFilter.FilterChanged += () => UpdateNodes();
			EmployeesVM employeeVM = new EmployeesVM();
			employeeVM.Filter.RestrictCategory = EmployeeCategory.office;
			entryreferencevmEmployeeFilter.RepresentationModel = employeeVM;
			taskStatusComboBox.ItemsEnum = typeof(CallTaskStatus);
			ytreeviewTasks.ColumnsConfig = ColumnsConfig;
			ytreeviewTasks.Selection.Mode = SelectionMode.Multiple;
			UpdateNodes();
		}

		public void UpdateNodes()
		{
			if(tasksMemento.Any()) 
			{
				MessageDialogHelper.RunInfoDialog("Для обновления необходимо закрыть подчиненные вкладки");
				return;
			}
			DeliveryPoint deliveryPointAlias = null;
			BottlesMovementOperation bottlesMovementAlias = null;
			CallTask resultAlias = null;
			Counterparty counterpartyAlias = null;
			Employee employeeAlias = null;

			var tasksQuery = UoW.Session.QueryOver(() => resultAlias);

			var bottleDebtByAddressQuery = UoW.Session.QueryOver(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			var bottleDebtByClientQuery = UoW.Session.QueryOver(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.Counterparty.Id == deliveryPointAlias.Counterparty.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			if(taskFilter.HideCompleted)
				tasksQuery = tasksQuery.Where((arg) => !arg.IsTaskComplete);
			if(taskFilter.StartDate != null && taskFilter.EndDate != null)
				tasksQuery = tasksQuery.Where((task) => task.Deadline >= taskFilter.StartDate && task.Deadline <= taskFilter.EndDate);
			if(taskFilter.Employee != null)
				tasksQuery = tasksQuery.Where((task) => task.AssignedEmployee.Id == taskFilter.Employee.Id);

			Tasks = tasksQuery
			.JoinAlias(c => c.Address, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.JoinAlias(c => c.Address.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.JoinAlias(c => c.AssignedEmployee, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.SelectList(list => list
				   .Select((c) => c.Address).WithAlias(() => resultAlias.Address)
				   .Select((c) => c.AssignedEmployee).WithAlias(() => resultAlias.AssignedEmployee)
				   .Select((c) => c.Comment).WithAlias(() => resultAlias.Comment)
				   .Select((c) => c.Deadline).WithAlias(() => resultAlias.Deadline)
				   .Select((c) => c.DateOfTaskCreation).WithAlias(() => resultAlias.DateOfTaskCreation)
				   .Select((c) => c.Id).WithAlias(() => resultAlias.Id)
				   .Select((c) => c.TaskState).WithAlias(() => resultAlias.TaskState)
				   .Select((c) => c.IsTaskComplete).WithAlias(() => resultAlias.IsTaskComplete)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByAddressQuery).WithAlias(() => resultAlias.DebtByAddress)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByClientQuery).WithAlias(() => resultAlias.DebtByClient)
					 )
			.TransformUsing(Transformers.AliasToBean<CallTask>())
			.List<CallTask>();

			if(!String.IsNullOrWhiteSpace(searchString))
				Tasks = Tasks.Where((arg) => arg.Address.CompiledAddress.Contains(searchString) || arg.Address.Counterparty.Name.Contains(searchString)).ToList();

			Tasks = tasks.OrderBy((arg) => arg.Address.Counterparty.Id).ToList();
			taskCountLabel.Text = "Кол-во задач : " + Tasks.Count.ToString();
		}

		public override bool Save() => true;

		#region BaseDlgButton

		protected void OnAddTaskButtonClicked(object sender, EventArgs e)
		{
			CallTaskDlg dlg = new CallTaskDlg(UoW);
			dlg.Removed += (o, args) => {
				if(dlg.SaveDlgState)
					observableTasks.Add(dlg.Entity);
			}; 
			OpenSlaveTab(dlg);
		}

		protected void OnDebtorsTreeViewRowActivated(object o, RowActivatedArgs args) => buttonEdit.Click();

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			List<CallTask> selected = ytreeviewTasks.GetSelectedObjects().OfType<CallTask>().ToList();
			CallTask callTask = selected.Any() ? selected[0] : null;

			if(callTask == null)
				return;
			if(tasksMemento.ContainsKey(callTask.Id))
			{
				MessageDialogHelper.RunInfoDialog("Задача уже открыта в подчиненной вкладке");
				return;
			}

			tasksMemento.Add(callTask.Id, callTask.CreateCopy());
			CallTaskDlg dlg = new CallTaskDlg(UoW , callTask);

			dlg.Removed += (o, args) => 
			{
				if(!dlg.SaveDlgState)
					callTask.LoadPreviousState(tasksMemento[callTask.Id]);
				tasksMemento.Remove(callTask.Id);
			};

			OpenSlaveTab(dlg);
		}

		protected void OnCheckShowFilterClicked(object sender, EventArgs e) => taskfilter.Visible = !taskfilter.Visible;

		protected void OnButtonEditSelectedClicked(object sender, EventArgs e) => hboxEditSelected.Visible = !hboxEditSelected.Visible;

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			ytreeviewTasks.GetSelectedObjects().OfType<CallTask>().ToList().ForEach(UoW.Delete);
			UoW.Commit();
			UpdateNodes();
		}

		protected void OnButtonSearchClicked(object sender, EventArgs e)
		{
			searchString = yentrySearch.Text;
			UpdateNodes();
		}

		protected void OnYentrySearchKeyReleaseEvent(object o, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Return)
				buttonSearch.Click();
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e) => UpdateNodes();

		#endregion

		#region ChangeEntityStateButton

		protected void OnAssignedEmployeeButtonClicked(object sender, EventArgs e) => entryreferencevmEmployeeFilter.OpenSelectDialog("Ответственный :");

		protected void OnEntryreferencevmEmployeeFilterChangedByUser(object sender, EventArgs e)
		{
			ytreeviewTasks.GetSelectedObjects().OfType<CallTask>().ToList().ForEach((task) => 
			{
				if(CheckOpedDlg(task)) 
				{
					task.AssignedEmployee = entryreferencevmEmployeeFilter.Subject as Employee;
					UoW.Save(task);
					UoW.Commit();
				}
			});
		}

		protected void OnCompleteTaskButtonClicked(object sender, EventArgs e)
		{
			ytreeviewTasks.GetSelectedObjects().OfType<CallTask>().ToList().ForEach((task) => 
			{
				if(CheckOpedDlg(task)) 
				{
					task.IsTaskComplete = true;
					UoW.Save(task);
					UoW.Commit();
				}
			});
		}

		protected void OnTaskstateButtonEnumItemClicked(object sender, EventArgs e)
		{
			ytreeviewTasks.GetSelectedObjects().OfType<CallTask>().ToList().ForEach((task) => 
			{
				if(CheckOpedDlg(task)) 
				{
					task.TaskState = (CallTaskStatus)taskStatusComboBox.SelectedItem;
					UoW.Save(task);
					UoW.Commit();
				}
			});
		}

		private bool CheckOpedDlg(CallTask callTask)
		{
			bool isOpen = tasksMemento.ContainsKey(callTask.Id);
			if(isOpen)
				MessageDialogHelper.RunInfoDialog(String.Format("Невозможно изменить задачу №{0} , т.к. задача открыта в отдельном окне ",callTask.Id));

			return !isOpen;
		}

		#endregion
	}
}